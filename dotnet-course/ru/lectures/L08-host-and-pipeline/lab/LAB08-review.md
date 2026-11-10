# Лабораторная 8 — заметки принимающему

Служебный файл. Студенту выдаётся [LAB08-task.md](LAB08-task.md); здесь — что имеет смысл
посмотреть и о чём спросить. Это рекомендации, а не регламент: порядок, набор вопросов и
строгость — на ваше усмотрение, ориентируйтесь на человека перед вами.

**Приёмка живая, отчёта нет.** Студент за своим терминалом, `Shop.Api` у него запущен.
Лабораторная продолжает седьмую: те же маршруты и тот же `OrderStore`, но вместо сервера на
сокетах — хост, Kestrel и конвейер. С неё сервис живёт до конца курса, поэтому здесь важнее
всего, чтобы скелет был правильным: ошибки в одном месте, логирование без дыр, фон, который
останавливается. Самая сильная проверка — **`curl` на бросающий адрес и взгляд в лог вместе
со студентом**, а потом одна правка: убрать `await` перед `next` или переставить `Use`.

Три главные вещи:

1. Исключение превращается в HTTP-ответ **ровно в одном месте** — в `ErrorHandlingMiddleware`,
   первом в конвейере. Обработчики про ошибки не знают: отсутствующий заказ — это
   `NotFoundException` из домена, а не `404`, собранный руками. Наружу для `500` — только
   `Internal error` и `traceId`, в лог — исключение целиком.
2. **Одна строка в логе на каждый запрос**, включая упавшие, и студент сам увидел, почему без
   фильтра исключения строки нет: код после `await next` не выполняется, когда исключение
   пролетает сквозь.
3. Фоновая задача — `PeriodicTimer` с токеном, `try` **внутри** цикла, отмена не глотается.
   По Ctrl+C процесс выходит за доли секунды, а не по таймауту.

**Как может выглядеть приёмка.** Начните с `curl -i` на `/orders`, `/orders/7` и
`/fail/boom` — и сразу в лог: по строке на запрос, полная трассировка у ошибок, нет текста
исключения в ответе на `500`. Дальше `curl --max-time 1` на `/fail/slow` — в логе
`Information`, не `Error`. Дальше Ctrl+C. Потом одна-две правки из шага 6 по слабым местам
рассказа, и вопросы «усложнённый» тем, кто уверенно ответил на основной.

## На что смотреть

| # | Что попросить показать | На что обратить внимание |
|---|---|---|
| 1 | `Shop.Api.csproj`, `Program.cs`, первые строки лога | `Sdk="Microsoft.NET.Sdk.Web"`, ссылка на `Shop.Domain`; шаблон `web`, а не `webapi`. Студент читает строки `Now listening on`, `Hosting environment`, `Content root path` |
| 2 | `OrderStore` и регистрация, сравнение двух серверов | `AddSingleton<OrderStore>()`; маршруты через `MapGet`; студент называет, что добавил Kestrel (`Date`, `Server`, `chunked`) и на что фреймворк ответил сам (`404`, `405` с `Allow`) |
| 3 | `NotFoundException`, `IOrderService`/`OrderService` | Наследник `DomainException`, `sealed`. `Get` возвращает `Order` и бросает; в обработчиках нет ни `Results.NotFound`, ни ручного `404`. Сервис принимает `int`, а не `HttpContext` |
| 4 | `ErrorHandlingMiddleware`, `UseErrorHandling` | Класс с `InvokeAsync`; первый `Use` после `Build`; `when (!context.Response.HasStarted)`; ветка отмены выше общей и пишет `Information`; `LogError(e, ...)` с исключением первым параметром; для `500` наружу нет `e.Message` |
| 5 | `RequestLoggingMiddleware` и лог четырёх запросов | Шаблон с параметрами, не `$"..."`. Строк ровно столько, сколько запросов; упавший запрос записан с типом исключения. Порядок `Use` не менялся |
| 6 | Любая правка из шага 6 по просьбе | Студент **до** запуска говорит, что увидит: `CS4014` и пустой `200`; `CS9113` и пустой `200` на всё; падение на старте без `InvokeAsync` |
| 7 | `OrderStatsService`, Ctrl+C | `PeriodicTimer` с `stoppingToken`; `try` внутри `while`; фильтр `when (e is not OperationCanceledException)`; в конструкторе `OrderStore`, не `IOrderService`. Остановка — мгновенно |
| 8 | Сборка, `git status` | Ноль предупреждений; `bin`/`obj` не в индексе; `/fail/{kind}` на месте, экспериментальных правок нет |

**Сравнение двух серверов, шаг 2.** Эталонные `Shop.RawServer` (на `TcpListener`, порт 8088,
`Connection: close`) и `Shop.Api` на тех же двух заказах — Ann Smith, оплачен, 300.00; Bob
Brown, создан, 120.00:

```text
$ curl -i http://localhost:8088/orders/1
HTTP/1.1 200 OK
Content-Type: text/plain; charset=utf-8
Content-Length: 27
Connection: close

1  Ann Smith  Paid  300.00
$ curl -i http://localhost:5272/orders/1
HTTP/1.1 200 OK
Content-Length: 27
Content-Type: text/plain; charset=utf-8
Date: Tue, 10 Nov 2026 10:32:47 GMT
Server: Kestrel

1  Ann Smith  Paid  300.00
$ curl -i http://localhost:8088/orders/7
HTTP/1.1 404 Not Found
Content-Type: text/plain; charset=utf-8
Content-Length: 11
Connection: close

no order 7
$ curl -i http://localhost:5272/orders/7
HTTP/1.1 404 Not Found
Content-Length: 11
Content-Type: text/plain; charset=utf-8
Date: Tue, 10 Nov 2026 10:32:47 GMT
Server: Kestrel

no order 7
$ curl -i http://localhost:5272/orders
HTTP/1.1 200 OK
Content-Type: text/plain; charset=utf-8
Date: Tue, 10 Nov 2026 10:32:47 GMT
Server: Kestrel
Transfer-Encoding: chunked

1  Ann Smith  Paid  300.00
2  Bob Brown  Created  120.00
$ curl -i http://localhost:5272/nope
HTTP/1.1 404 Not Found
Content-Length: 0
Date: Tue, 10 Nov 2026 10:32:47 GMT
Server: Kestrel

$ curl -i -X POST http://localhost:5272/orders
HTTP/1.1 405 Method Not Allowed
Content-Length: 0
Date: Tue, 10 Nov 2026 10:32:47 GMT
Server: Kestrel
Allow: GET

```

Что из этого стоит услышать от студента:

- `Date` и `Server` добавил Kestrel. `Connection: close` нет: соединение остаётся открытым для
  следующего запроса — то, что в седьмой лабораторной было одним из дополнительных заданий.
- Обработчик, вернувший строку, отвечает **`chunked`** без `Content-Length`: фреймворк пишет
  тело кусками и длину заранее не считает. `Results.Text(...)` из шага 2 отвечает с
  `Content-Length` — оба способа честно сообщают, где кончается тело.
- `404` на неизвестный путь и `405` с `Allow: GET` — от маршрутизации, без строки кода. Тело у
  обоих пустое: это не наш формат ошибки, и его не перехватит `ErrorHandlingMiddleware` — это не
  исключение, а обычный ответ.
- `/orders/abc` — `404`, а не `400`: ограничение `{number:int}` не совпало, маршрута нет.

**Отсутствующий заказ без middleware, шаг 3.** В `Development` (`dotnet run`) клиент получает
трассировку текстом, первые строки:

```text
HTTP/1.1 500 Internal Server Error
Content-Type: text/plain; charset=utf-8
Date: Tue, 10 Nov 2026 10:19:35 GMT
Server: Kestrel
Transfer-Encoding: chunked

Shop.Domain.NotFoundException: no order 7
   at Shop.Api.OrderService.Get(Int32 number) in /home/user/Shop.Api/OrderService.cs:line 16
```

В `Production` (`dotnet run -- --environment Production`) — пустой `500`, а трассировка только в
логе, под категорией `Microsoft.AspNetCore.Server.Kestrel[13]`, `An unhandled exception was
thrown by the application.`:

```text
HTTP/1.1 500 Internal Server Error
Content-Length: 0
Date: Tue, 10 Nov 2026 10:19:44 GMT
Server: Kestrel

```

Оба ответа — `500` на то, что по смыслу `404`. Это и есть мотивация шага 4.

**Сессия с эталонным решением, шаги 4–5.** Запуск `dotnet bin/Debug/net10.0/Shop.Api.dll --urls
http://localhost:5272` (окружение `Production`), ответы:

```text
$ curl -i http://localhost:5272/orders/7
HTTP/1.1 404 Not Found
Content-Type: application/json; charset=utf-8
Date: Tue, 10 Nov 2026 10:20:51 GMT
Server: Kestrel
Transfer-Encoding: chunked

{"error":"no order 7","traceId":"0HNPV2H5T7K53:00000001"}
$ curl -i http://localhost:5272/fail/boom
HTTP/1.1 500 Internal Server Error
Content-Type: application/json; charset=utf-8
Date: Tue, 10 Nov 2026 10:20:51 GMT
Server: Kestrel
Transfer-Encoding: chunked

{"error":"Internal error","traceId":"0HNPV2H5T7K54:00000001"}
$ curl -i --max-time 1 http://localhost:5272/fail/slow
$ echo $?
28
$ curl -i http://localhost:5272/fail/late
HTTP/1.1 200 OK
Date: Tue, 10 Nov 2026 10:20:52 GMT
Server: Kestrel
Transfer-Encoding: chunked

partial
$ echo $?
18
```

Лог тех же запросов (перед ними — `/health`, `/`, `/orders`, `/orders/1`, по строке на каждый):

```text
info: Shop.Api.RequestLoggingMiddleware[0]
      GET /orders/7 -> NotFoundException in 0.8999 ms
fail: Shop.Api.ErrorHandlingMiddleware[0]
      Request /orders/7 failed with 404
      Shop.Domain.NotFoundException: no order 7
         at Shop.Api.OrderService.Get(Int32 number) in /home/user/Shop.Api/OrderService.cs:line 16
         at Program.<>c.<<Main>$>b__0_2(Int32 number, IOrderService orders) in /home/user/Shop.Api/Program.cs:line 20
         at lambda_method3(Closure, Object, HttpContext)
         at Shop.Api.RequestLoggingMiddleware.InvokeAsync(HttpContext context) in /home/user/Shop.Api/Middleware.cs:line 41
         at Shop.Api.ErrorHandlingMiddleware.InvokeAsync(HttpContext context) in /home/user/Shop.Api/Middleware.cs:line 12
info: Shop.Api.RequestLoggingMiddleware[0]
      GET /fail/boom -> InvalidOperationException in 0.4446 ms
fail: Shop.Api.ErrorHandlingMiddleware[0]
      Request /fail/boom failed with 500
      System.InvalidOperationException: Test failure: boom
         at Program.<>c.<<<Main>$>b__0_3>d.MoveNext() in /home/user/Shop.Api/Program.cs:line 31
      --- End of stack trace from previous location ---
         at Shop.Api.RequestLoggingMiddleware.InvokeAsync(HttpContext context) in /home/user/Shop.Api/Middleware.cs:line 41
         at Shop.Api.ErrorHandlingMiddleware.InvokeAsync(HttpContext context) in /home/user/Shop.Api/Middleware.cs:line 12
info: Shop.Api.RequestLoggingMiddleware[0]
      GET /fail/slow -> TaskCanceledException in 1013.7496 ms
info: Shop.Api.ErrorHandlingMiddleware[0]
      Request /fail/slow aborted by the client
info: Shop.Api.RequestLoggingMiddleware[0]
      GET /fail/late -> InvalidOperationException in 0.5028 ms
fail: Microsoft.AspNetCore.Server.Kestrel[13]
      Connection id "0HNPV2H5T7K56", Request id "0HNPV2H5T7K56:00000001": An unhandled exception was thrown by the application.
      System.InvalidOperationException: Test failure: late
```

Что на этом экране проверить:

- `traceId` из ответа в логе **не найти**: шаблон обработчика ошибок пишет путь и статус, а
  консольный логгер по умолчанию идентификатор запроса не печатает. Хороший ответ на вопрос
  шага 4 — добавить `context.TraceIdentifier` параметром шаблона; автоматически это делает
  область логирования, о которой Л12.
- `/fail/slow`: `curl` вышел по своему таймауту (код 28), в логе — одна строка `Information`
  от обработчика ошибок, ни одного `fail`. Без ветки отмены на этом месте `fail` с
  `TaskCanceledException` и попытка написать `500` клиенту, которого уже нет.
- `/fail/late`: ответ начал уходить, фильтр `when (!HasStarted)` пропустил исключение мимо,
  Kestrel оборвал соединение — `curl` завершился с кодом 18, тело неполное. Это честнее, чем
  полуответ со статусом `200`, приклеенным к телу ошибки.

**Строка, которой нет, шаг 5.** Без фильтра в `RequestLoggingMiddleware` те же четыре запроса
дают в логе две строки логирования — для `/` и `/orders/1`. Для `/orders/7` и `/fail/boom` есть
только записи `ErrorHandlingMiddleware`: исключение пролетело сквозь логирование, и код после
`await next(context)` не выполнился. Это открытый вопрос №1 из `README.md` лекции, и
лабораторная решает его, не трогая порядок из лекции.

**Выбор эталонного решения — фильтр исключения.** `catch (Exception e) when (LogFailed(...))`,
где `LogFailed` пишет строку и возвращает `false`. Плата: для упавшего запроса в строке тип
исключения, а не статус — статус выбирает обработчик ошибок снаружи, и он виден в его записи.
Обе альтернативы проверены на эталоне, и обе — допустимые ответы на усложнённый вопрос, если
студент называет их цену:

| Вариант | Что записано для `/orders/7` | Чем платит |
|---|---|---|
| Фильтр `when (...)`, возвращающий `false` (эталон) | `GET /orders/7 -> NotFoundException in 0.8999 ms` | нет итогового статуса в строке |
| Логирование **перед** обработкой ошибок | `GET /orders/7 -> 404 in 42.7438 ms` | правило «ошибки первыми» нарушено: исключение из самого логирования никто не поймает |
| `finally` вокруг `await next` | `GET /orders/7 -> 200 in 5.0453 ms` | **неправда в логе**: статус ещё не выставлен, записан `200` по умолчанию |

Если у студента вариант с `finally` и `200` в строке упавшего запроса — это не засчитывается:
лог, который врёт про статус, хуже, чем лог без строки.

**Забытый `next`, шаг 6.** Без `await` перед `next(context)` эталонное решение на `/fail/slow`
отвечает **сразу**:

```text
HTTP/1.1 200 OK
Content-Length: 0
Date: Tue, 10 Nov 2026 10:24:56 GMT
Server: Kestrel

```

а в логе `GET /fail/slow -> 200 in 0.5812 ms`: конвейер вернулся раньше обработчика, ответ ушёл
пустым, исключение обработчика через 10 секунд уже никому не нужно. Синхронные маршруты
(`/orders/1`, `/orders/7`) при этом выглядят нормально — поэтому баг и живёт долго. Без вызова
`next` вовсе — пустой `200` на любой адрес, включая `/fail/boom`; в логе `GET /fail/boom -> 200
in 0.0078 ms`.

**Фоновая задача, шаг 7.** На эталоне остановка сигналом `SIGTERM` (тот же путь, что Ctrl+C)
заняла **0.03 с**, процесс вышел с кодом 0, последняя строка лога — `Application is shutting
down...`. С `Thread.Sleep(TimeSpan.FromSeconds(10))` вместо таймера остановка, запрошенная на
второй секунде сна, заняла **5.04 с**, и после `Application is shutting down...` в логе
появилась ещё одна строка `Orders by status: Created=1, Paid=1` — итерация доработала после
сигнала. Компилятор при этом молчит: ноль предупреждений.

Исключение, вылетевшее из `ExecuteAsync`, останавливает весь хост:

```text
fail: Microsoft.Extensions.Hosting.Internal.Host[9]
      BackgroundService failed
      System.InvalidOperationException: stats failed
         at Shop.Api.OrderStatsService.ExecuteAsync(CancellationToken stoppingToken) in /home/user/Shop.Api/OrderStatsService.cs:line 12
         at Microsoft.Extensions.Hosting.Internal.Host.TryExecuteBackgroundServiceAsync(BackgroundService backgroundService)
crit: Microsoft.Extensions.Hosting.Internal.Host[10]
      The HostOptions.BackgroundServiceExceptionBehavior is configured to StopHost. A BackgroundService has thrown an unhandled exception, and the IHost instance is stopping. To avoid this behavior, configure this to Ignore; however the BackgroundService will not be restarted.
```

и `echo $?` даёт **0**.

## Что говорит компилятор

Всё ниже — с эталонного решения на dotnet SDK 10.0.114. Часть ошибок компилятор не видит
вовсе, и это само по себе предмет разговора: они проявляются на старте или на запросе.

| Правка | Результат |
|---|---|
| Убрать `await` перед `next(context)` | `warning CS4014: Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.` |
| Убрать вызов `next(context)` целиком | `warning CS9113: Parameter 'next' is unread.` — у первичного конструктора; на запросе пустой `200` |
| Переименовать `InvokeAsync` в `Handle` | сборка чистая; на старте `System.InvalidOperationException: No public 'Invoke' or 'InvokeAsync' method found for middleware of type 'Shop.Api.RequestLoggingMiddleware'.` из `UseMiddleware` |
| `IOrderService` в конструкторе `OrderStatsService` | сборка чистая; в `Development` падение на `Build`: `Cannot consume scoped service 'Shop.Api.IOrderService' from singleton 'Microsoft.Extensions.Hosting.IHostedService'.`, код выхода 134. В `Production` сервис **стартует молча** |
| Убрать `when (!context.Response.HasStarted)`, запрос на `/fail/late` | сборка чистая; в логе `System.InvalidOperationException: StatusCode cannot be set because the response has already started.` из нашего `catch` |
| `Thread.Sleep` в цикле `ExecuteAsync` без единого `await` | сборка чистая, ни одного предупреждения; остановка ждёт конца сна |
| Тело 31 МБ в эндпоинт, читающий тело | `BadHttpRequestException: Request body too large. The max request body size is 30000000 bytes.` попадает в ветку `_` — клиент получает `500` и `Internal error` вместо `413` |

Строка про `IOrderService` — хороший повод вслух спросить про окружения: проверка контейнера
на `Build` включена только в разработке, и это ровно то «падать сразу», о котором говорила
лекция.

## Вопросы, из которых можно выбирать

Ответы даны коротко — важна суть, а не формулировка.

**Про хост и Kestrel**

- Чем `csproj` отличается от консольного?
  *`Sdk="Microsoft.NET.Sdk.Web"`: веб-часть рантайма, неявные `using` для веба, копирование
  `appsettings.json` в вывод.*
- Откуда порт и как поменять без кода?
  *Из `Properties/launchSettings.json`, который читает `dotnet run`. Снаружи — `--urls` или
  `ASPNETCORE_URLS`; аргумент побеждает переменную. Адрес — свойство окружения.*
- Кто ответил `404` на `/nope` и `405` на `POST /orders`?
  *Маршрутизация: пути нет — `404`; путь есть, метода нет — `405` и `Allow` со списком
  методов. В седьмой лабораторной это были ваши ветки.*
- Что Kestrel делает иначе, чем сервер на сокетах (усложнённый)?
  *Нет `Connection: close`, длина известна (`Content-Length` или `chunked` с нулевым куском) —
  клиент шлёт следующий запрос в то же соединение; открыть соединение дорого, Л7. Соединения
  обслуживаются одновременно, а не по одному. Тело сверх 30 000 000 байт и заголовки сверх
  лимита отсекаются — у сервера на сокетах один клиент мог занять всю память.*

**Про исключение домена и сервис**

- Что получил клиент в `Development` и в `Production`?
  *В разработке — трассировку текстом, от страницы ошибок разработчика; в проде — пустой
  `500`, трассировка только в логе. Первое наружу нельзя: пути, имена, внутренности.*
- Почему сервис принимает номер, а не `HttpContext`?
  *Контекст не уходит дальше эндпоинта: логика с `int` работает и из фоновой задачи, и из
  теста. Пользователь достаётся из контекста на границе и передаётся значением.*
- Что изменится в `HelloWorld` с появлением `NotFoundException` (усложнённый)?
  *`catch (DomainException)` поймает и его — ради этого база и абстрактная. Хорошо: новая
  ошибка домена обработана без правки цикла. Плохо, если нужна разная реакция — тогда ловить
  конкретный тип выше.*

**Про обработку ошибок**

- Почему обработчик ошибок первый?
  *Его `try` вокруг `next`, а значит — вокруг всего, что после. Что стоит до него, снаружи
  `try`: исключение оттуда уйдёт в сервер, клиент получит пустой `500` без нашего формата.*
- Почему у `500` нет текста, а у `404` есть?
  *Сообщение `NotFoundException` написали мы, оно про домен. Сообщение произвольного
  исключения может нести SQL, путь, строку подключения. Наружу — `traceId`, по нему ищут лог.*
- Нашёлся ли `traceId` в логе?
  *Нет: консольный логгер его не печатает, а шаблон его не содержит. Добавить параметром
  шаблона — сейчас; область логирования на запрос — Л12.*
- Почему отмена — `Information`?
  *Клиент ушёл — это сигнал, а не ошибка сервиса (Л6). `Error` на каждую закрытую вкладку
  забьёт лог и разбудит дежурного зря. Отвечать некому.*
- Что ломается без `when (!HasStarted)` (усложнённый)?
  *Заголовки уже ушли; наш `catch` пытается поставить статус и падает с `StatusCode cannot be
  set because the response has already started.` Это исключение уже никто не ловит, Kestrel
  обрывает соединение и пишет свою запись.*
- `413` или `500` на тело больше лимита (усложнённый)?
  *`500`: Kestrel бросает `BadHttpRequestException` на первом чтении тела внутри конвейера, и
  ветка `_` превращает его во «внутреннюю ошибку». Ветка
  `BadHttpRequestException bad => (bad.StatusCode, ...)` вернёт `413`. Это открытый вопрос №2
  лекции.*
- Какой статус для `InvalidTransitionException` (усложнённый)?
  *`409 Conflict`: запрос понятен, но противоречит текущему состоянию ресурса. Не `500` — это
  не сбой сервиса, и не `400` — запрос сформирован правильно.*

**Про логирование**

- Почему без фильтра нет строки?
  *Исключение пролетело сквозь `await next(context)`: строки после него не выполнились. Код
  «после» в middleware выполняется только при нормальном возврате.*
- Фильтр против `catch` с `throw;`?
  *Фильтр выполняется до того, как стек раскрутится, и исключение не ловит — нечего
  перебрасывать. `catch` с `throw;` ловит и перебрасывает; с `throw e;` трассировка начнётся с
  логирования — `CA2200` из четвёртой лабораторной.*
- Почему шаблон, а не интерполяция?
  *Параметры остаются полями записи — по ним потом ищут и считают. Подробно — Л12, привычка —
  с сегодняшнего дня.*

**Про забытый `next` и порядок**

- Пустой `200` — как узнать?
  *Пустой ответ на любой адрес — компонент не вызвал `next`. Пустой `200` только у асинхронных
  обработчиков и длительность в доли миллисекунды — `next` вызван без `await`. В обоих случаях
  компилятор предупреждал.*
- Почему `InvokeAsync` проверяется только на старте?
  *`UseMiddleware` ищет метод по имени через рефлексию при сборке конвейера — это не интерфейс,
  компилятору проверять нечего.*

**Про фон**

- Почему `OrderStore`, а не `IOrderService`?
  *Фоновый сервис — singleton на всё время работы, `IOrderService` — scoped, на запрос, а
  запроса нет. В разработке `Build` проверяет это и падает; в проде — стартует и держит один
  экземпляр навсегда. Правильно — область на итерацию, Л10.*
- Почему `try` внутри цикла?
  *Упавшая итерация не должна остановить следующие. Исключение из `ExecuteAsync` останавливает
  весь хост с кодом выхода 0 — сервис лежит, а мониторинг кода выхода молчит.*
- Почему фильтр на отмену?
  *Отмена — сигнал остановки. Проглоченная в `catch`, она не выпустит цикл, и хост будет ждать
  до таймаута.*
- Что стало с остановкой при `Thread.Sleep` (усложнённый)?
  *Сон не слышит токен: остановка ждёт конца сна, итерация дорабатывает после сигнала. Поток
  пула занят всё время сна. Компилятор не предупреждает.*

**Про два экземпляра и расписание** (дополнительное задание)

- Сколько раз сработала задача на двух экземплярах? (На эталоне проверен один экземпляр на
  `--urls http://localhost:5001`: аргумент побеждает порт из профиля запуска.)
  *Дважды: у каждого свой таймер. Счета ушли бы дважды — задача должна быть идемпотентной,
  блокировка или кластерный режим не отменяют этого.*
- Сколько запусков одновременно с `[DisallowConcurrentExecution]`?
  *Один: следующий запуск ждёт конца текущего, в логе «started» и «finished» чередуются. Без
  атрибута «started» идут подряд, запуски перекрываются.*

## Возможные ошибки

**Обработчик ошибок не первый.** `UseRequestLogging()` выше `UseErrorHandling()` — работает, и
даже даёт в логе итоговый статус. Спросите, что будет с исключением из самого логирования и
почему лекция ставит ошибки первыми.

**`try`/`catch` в обработчике.** `/orders/{n}` ловит `NotFoundException` и возвращает `404` сам.
Работает, но это то самое «сто границ». Спросите, где теперь формат ошибки и сколько мест
править, если он изменится.

**`e.Message` наружу для `500`.** Ветка `_` отдаёт сообщение исключения. Попросите показать
ответ на `/fail/boom` и спросите, что было бы, если бы там была строка подключения.

**`catch` без `when` на `HasStarted`.** Заметно только на `/fail/late`. Попросите выполнить и
найти в логе вторую ошибку.

**Нет ветки отмены или она `Error`.** `curl --max-time 1` на `/fail/slow` даёт `fail` с
`TaskCanceledException`. Спросите, кто виноват в этой «ошибке».

**Логирование через `finally`.** Строка на каждый запрос есть, но у `/orders/7` в ней `200`.
Попросите прочитать строку вслух рядом с ответом `curl`.

**Интерполяция в логе.** `logger.LogInformation($"{method} {path}...")`. Работает, но это не
шаблон. Сборка на это не предупреждает — на эталоне интерполированное сообщение собралось без
единого предупреждения, — поэтому спросите вслух.

**`catch` отмены в фоновом цикле.** `catch (Exception)` без фильтра — Ctrl+C обрабатывается
дольше или задача пишет ошибку при каждой остановке. Попросите нажать Ctrl+C и прочитать лог.

**`Thread.Sleep` или `Task.Delay` без токена.** Остановка ждёт. Попросите засечь время Ctrl+C.

**Рассказ не про свой экран.** Определение middleware звучит дословно, а в логе нет строк для
упавших запросов. Лечится просьбой выполнить `curl` на `/orders/7` и посчитать строки вслух.
