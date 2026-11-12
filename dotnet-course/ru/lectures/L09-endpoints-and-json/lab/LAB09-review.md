# Лабораторная 9 — заметки принимающему

Служебный файл; студенту выдаётся [LAB09-task.md](LAB09-task.md). Рекомендации, а не регламент.

**Приёмка живая, минут на десять.** `curl -i` на `POST /orders` с правильным и невалидным телом,
на `/orders/abc` и `/orders?over=`, затем одна правка вживую (см. «Правки вживую»). На что смотреть:

1. **Шаг 1.** Наружу уходит только `OrderDto`; `CreateOrderDto` без поля номера; преобразования —
   методами расширения, а не в handler. `POST` отвечает `201` и `Location`. Настройки JSON —
   один раз до `Build`, а не в отдельном handler.
2. **Шаг 2.** В handlers нет ни одной проверки и ни одного `if (!result.IsValid)`; filter висит
   на группе; validator найден в контейнере, а не создан через `new`.
3. **Шаг 3.** Четыре ветки в одном switch; `errors` — только у `422`, имена полей как в JSON.
   Handlers про ошибки не знают: ни `try`, ни `Results.BadRequest`.

## Эталон

Всё ниже — с эталонного решения на dotnet SDK 10.0.114 и FluentValidation 12.2.0, окружение
Development, порт 5090; заказы Ann Smith (Paid, 300.00) и Bob Brown (Created, 120.00), как в седьмой
и восьмой лабораторных. Заголовки `Date`/`Server`/`Transfer-Encoding` сокращены до `...`.

**Шаг 1.**

```text
$ curl -i --json @new-order.json http://localhost:5090/orders
HTTP/1.1 201 Created
Content-Type: application/json; charset=utf-8
...
Location: /orders/3

{"number":3,"customerEmail":"cat@example.com","status":"Created","total":270.00}
$ curl http://localhost:5090/orders?over=200
[{"number":1,"customerEmail":"ann@example.com","status":"Paid","total":300.00},{"number":3,"customerEmail":"cat@example.com","status":"Created","total":270.00}]
```

С `TypedResults.Ok(order)` вместо DTO клиент получает домен целиком, с `Guid` и обёртками:

```text
{"id":{"value":"13d13f83-b321-47f0-ad63-b5a53b9d2bc8"},"customer":{"email":{"value":"ann@example.com"},"name":"Ann Smith"},"items":[{"product":"Green tea","quantity":2,"price":{"amount":150.00}}],"status":"Paid"}
```

**Шаг 3.**

```text
$ curl -i --json @bad-order.json http://localhost:5090/orders
HTTP/1.1 422 Unprocessable Entity
...
{"error":"Validation failed","traceId":"0HNPV42AFMI4V:00000001","errors":{"customerEmail":["'Customer Email' is not a valid email address."],"items[0].quantity":["'Quantity' must be greater than '0'."]}}
$ curl -i http://localhost:5090/orders/abc
HTTP/1.1 404 Not Found
Content-Length: 0
...
$ curl -i http://localhost:5090/orders?over=
HTTP/1.1 400 Bad Request
...
{"error":"Bad request","traceId":"0HNPV42AFMI52:00000001"}
$ curl http://localhost:5090/orders?over=150.5
[{"number":1,"customerEmail":"ann@example.com","status":"Paid","total":300.00}]
$ curl http://localhost:5090/orders?over=150,5
[]
```

Сломанный JSON (`--json '{bad'`) — тоже `400` и `Bad request` через ту же ветку. В log на каждый
ответ из middleware — `fail: Shop.Api.ErrorHandlingMiddleware[0]`, `Request /orders failed with 422`
и исключение; для `400` — `BadHttpRequestException: Failed to bind parameter "Nullable<decimal> over"
from "".`

**Дополнительное задание.** `curl -i -N` на `/orders/export`: `200`, `Transfer-Encoding: chunked`,
без `Content-Length`; без `[EnumeratorCancellation]` сборка даёт `warning CS8425: Async-iterator
'OrderExport.ExportAsync(OrderStore, CancellationToken)' has one or more parameters of type
'CancellationToken' but none of them is decorated with the 'EnumeratorCancellation' attribute, ...`

## Правки вживую

| Правка | Что будет |
|---|---|
| Убрать `AddValidatorsFromAssemblyContaining` | Сборка чистая; любой `POST /orders` — `500` `Internal error`, в log ``InvalidOperationException: No service for type 'FluentValidation.IValidator`1[Shop.Api.CreateOrderDto]' has been registered.`` |
| Убрать ветку `BadHttpRequestException` | `/orders?over=` — `500` `Internal error` в Development: ошибка привязки клиента выглядит как баг сервиса (открытый вопрос 1 в README) |

## Вопросы и короткие ответы

- **Почему `OrderDto`, а не `Order`?** *DTO фиксирует контракт API: домен меняется свободно, клиент
  видит то, что ему обещали. `Order` наружу — это `Guid`, обёртки `value`/`amount` и всё, что
  появится в домене завтра.*
- **Почему filter бросает, а не возвращает `400`?** *Невалидный запрос — ошибка контракта, как в
  Л4; ответ на неё собирается в одном месте. Проверка в handler — пять одинаковых строк в каждом и
  один забытый.*
- **Какие ответы собрал middleware?** *`422` и `400` — да; `404` на `/orders/abc` — нет: это
  routing, ограничение `int` не совпало, исключения не было, тело пустое.*
- **`150,5` (усложнённый)?** *Привязка читает число в invariant culture, запятая — разделитель
  тысяч: `150,5` → `1505`, и ни один заказ не дороже. Ошибки нет, потому что строка — валидное
  число. Лечится контрактом: точка в документации и клиенте.*

**Частые ошибки:** сущность отдана наружу; `CreateOrderDto` с полем `Number`/`Id`; проверка
validator в handler; `JsonSerializerOptions` в одном handler вместо `ConfigureHttpJsonOptions`;
`errors` в ответе на каждую ошибку, а не только на `422`.
