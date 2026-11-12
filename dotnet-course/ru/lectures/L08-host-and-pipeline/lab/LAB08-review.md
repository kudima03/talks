# Лабораторная 8 — заметки принимающему

Служебный файл; студенту выдаётся [LAB08-task.md](LAB08-task.md). Рекомендации, а не регламент.

**Приёмка живая, минут на десять.** `curl -i` на `/orders/7` и `/fail` и сразу в log, Ctrl+C, затем
одна правка вживую — **забытый `next`** (лекция называет его самым частым багом, см. «Правки
вживую»). На что смотреть:

1. **Шаг 1.** Шаблон `web`; `AddSingleton<OrderStore>()`; три `MapGet`. `404` на неизвестный путь
   и `405` с `Allow` студент больше не пишет — их отдаёт routing.
2. **Шаг 2.** `NotFoundException` — `sealed`-наследник `DomainException`; в handlers нет ручного
   `404`; `ErrorHandlingMiddleware` — первый после `Build`, с `when (!context.Response.HasStarted)`;
   для `500` наружу только `Internal error` и `traceId`, в log — исключение целиком.
3. **Шаг 3.** `PeriodicTimer` со `stoppingToken`; `try` внутри `while`; filter на
   `OperationCanceledException`; Ctrl+C — сразу. Сборка без предупреждений, `bin`/`obj` не в git.

## Эталон

Всё ниже — с эталонного решения на dotnet SDK 10.0.114; заказы Ann Smith (Paid, 300.00) и Bob
Brown (Created, 120.00), как в седьмой лабораторной. Номера строк у студента свои.

**Шаг 1.** `Shop.Api` на 5272 (`Shop.RawServer` на 8088 отвечает так, как в седьмой лабораторной:
`Content-Type`, `Content-Length`, `Connection: close` и текстовое тело):

```text
$ curl -i http://localhost:5272/nope
HTTP/1.1 404 Not Found
Content-Length: 0
Date: Tue, 10 Nov 2026 11:15:07 GMT
Server: Kestrel

$ curl -i -X POST http://localhost:5272/orders
HTTP/1.1 405 Method Not Allowed
Content-Length: 0
Date: Tue, 10 Nov 2026 11:15:07 GMT
Server: Kestrel
Allow: GET

```

Тело у `404` и `405` пустое: это ответ routing, не исключение, `ErrorHandlingMiddleware` его не трогает.

**Шаг 2.** Ответы и log (окружение `Production`):

```text
$ curl -i http://localhost:5272/orders/7
HTTP/1.1 404 Not Found
Content-Type: application/json; charset=utf-8
...
{"error":"no order 7","traceId":"0HNPV3EKAIKDM:00000001"}
$ curl http://localhost:5272/fail
{"error":"Internal error","traceId":"0HNPV3EKAIKDN:00000001"}
```

```text
fail: Shop.Api.ErrorHandlingMiddleware[0]
      Request /fail failed with 500
      System.InvalidOperationException: Test failure
         at Program.<>c.<<Main>$>b__0_3() in /home/user/Shop.Api/Program.cs:line 25
         at lambda_method4(Closure, Object, HttpContext)
         at Shop.Api.ErrorHandlingMiddleware.InvokeAsync(HttpContext context) in /home/user/Shop.Api/Middleware.cs:line 12
```

`traceId` из ответа в этом log **не найти**: консольный logger его не печатает, а шаблон лекции
его не содержит. Хороший ответ на вопрос шага 2 — добавить `context.TraceIdentifier` параметром
шаблона; автоматически это делает logging scope, Л12.

**Шаг 3.** Две строки `Orders by status: Created=1, Paid=1` с интервалом 10 с; остановка по
`SIGTERM` (тот же путь, что Ctrl+C) — 0.03 с, код выхода 0, последняя строка
`Application is shutting down...`.

## Правки вживую

| Правка | Что будет |
|---|---|
| Убрать вызов `next(context)` в `ErrorHandlingMiddleware` | `warning CS9113: Parameter 'next' is unread.`; пустой `200` на любой адрес, включая `/fail` |
| Убрать `await` перед `next(context)` | `warning CS4014: Because this call is not awaited, ...`. На синхронных handlers `/orders/7` и `/fail` всё **по-прежнему работает**: исключение бросается до первого `await`. Баг проявится на первом асинхронном handler — поэтому он и живёт долго |

## Вопросы и короткие ответы

- **Кто отвечает `404` и `405`?** *Routing: пути нет — `404`; путь есть, метода нет — `405` и
  `Allow`. Kestrel добавляет `Date`, `Server` и держит соединение открытым.*
- **Что Kestrel делает иначе (усложнённый)?** *Обслуживает соединения одновременно; тело сверх
  30 000 000 байт и заголовки сверх лимита отсекает — у сервера на сокетах один клиент мог занять
  всю память.*
- **Почему у `500` нет текста, а у `404` есть?** *Сообщение `NotFoundException` написали мы, оно
  про домен; сообщение произвольного исключения может нести SQL или путь. Подробности — в log.*
- **Почему `try` внутри цикла?** *Упавшая итерация не должна остановить следующие. Исключение из
  `ExecuteAsync` останавливает весь host, и процесс выходит с кодом 0 — сервис лежит, а код
  выхода говорит «всё хорошо».*

## Дополнительное задание

С эталона, `RequestLoggingMiddleware` вторым. **Без exception filter** на `/orders/1`, `/orders/7`,
`/fail` в log одна строка logging — `GET /orders/1 -> 200 in 8.843 ms`; у упавших только записи
`ErrorHandlingMiddleware`: код после `await next` не выполняется, когда исключение пролетает
сквозь. **С filter** — `GET /orders/7 -> NotFoundException in 1.3267 ms` и
`GET /fail -> InvalidOperationException in 0.1067 ms`: тип вместо статуса, статус ещё не выбран.
`try/finally` вместо filter записал бы `200` — **не засчитывается**: log врёт про статус. С веткой
отмены `curl --max-time 1` выходит с кодом 28, а в log `Request /slow aborted by the client`
уровня `Information`, ни одного `fail`.

**Частые ошибки:** middleware ошибок не первый или `try`/`catch` прямо в handler (спросите,
сколько мест править при смене формата); `e.Message` наружу для `500`; `catch` в background
service без filter на отмену — Ctrl+C ждёт.
