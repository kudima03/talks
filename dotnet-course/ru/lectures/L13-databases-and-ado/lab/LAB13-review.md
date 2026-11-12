# Лабораторная 13 — заметки принимающему

Служебный файл; студенту выдаётся [LAB13-task.md](LAB13-task.md). Рекомендации, а не регламент.

**Приёмка живая, минут на десять.** `curl -i` на `/orders`, `/orders/1`, `/orders/7` и `POST /orders`,
SQL в `PgOrderStore` глазами, trace `POST` на dashboard, затем одна правка вживую (см. «Правки
вживую»). На что смотреть:

1. **Шаг 1.** `NpgsqlDataSource` приходит из контейнера, нигде нет `new NpgsqlConnection(...)` и
   connection string в коде. Схема — в `schema.sql`, а не в строках C#.
2. **Шаг 2.** Каждый connection, command и Reader — под `await using`; connection открывается внутри
   метода, а не хранится в поле. `GetOrdinal` — до цикла. Значение `number` — только parameter.
   В `AddOrders()` изменилась одна регистрация; handlers и `IOrderStore` не тронуты.
3. **Шаг 3.** Обе command получают `transaction`; `CommitAsync` один, в конце; ни `try`/`catch`, ни
   `RollbackAsync`. В SQL нет ни одной интерполяции и ни одного `+` со значением.

## Эталон

Всё ниже — с эталонного решения на dotnet SDK 10.0.114, Aspire 13.6.4 (`Aspire.Hosting.PostgreSQL`),
`Aspire.Npgsql` 13.6.5, Npgsql 10.0.3, PostgreSQL 18 в контейнере, порт 5090. Эталон — минимальный:
`Shop.Domain`, `Shop.Api` с `IOrderStore` и тремя маршрутами `/orders`, `Shop.AppHost` с одним
PostgreSQL; **без** JWT из одиннадцатой и без cache из двенадцатой (их лабораторные писались
параллельно), поэтому в выводе нет `$AUTH`. Заголовки сокращены до `...`.

```text
$ curl http://localhost:5090/orders
[{"number":1,"customerEmail":"ann@example.com","status":"Paid","total":300.00},{"number":2,"customerEmail":"bob@example.com","status":"Created","total":120.00}]
$ curl -i --json @new-order.json http://localhost:5090/orders
HTTP/1.1 201 Created
...
Location: /orders/3

{"number":3,"customerEmail":"cat@example.com","status":"Created","total":270.00}
$ curl http://localhost:5090/orders/3
{"number":3,"customerEmail":"cat@example.com","status":"Created","total":270.00}
```

`/orders/7` — `404` (в эталоне пустой; у студента — JSON из `ErrorHandlingMiddleware`).

**«Сломайте нарочно».** С интерполяцией `evil-order.json` отвечает `201`, а позиций нет ни у кого:

```text
$ curl http://localhost:5090/orders
[{"number":1,"customerEmail":"ann@example.com","status":"Paid","total":0},{"number":2,"customerEmail":"bob@example.com","status":"Created","total":0},{"number":3,"customerEmail":"cat@example.com","status":"Created","total":0}]
```

(В этом запуске перед ним был отправлен `O'Brien tea`, так что третий заказ в базе — номер 4: см.
«Номера после отката».) С parameter тот же файл — обычный заказ с `"total":1.00`, а в `order_items.product` лежит строка
`Tea', 1, 1); delete from order_items; --` целиком. Товар `O'Brien tea` с интерполяцией — `500`,
`PostgresException: 42601: syntax error at or near "Brien"`. После перезапуска AppHost база пустая и
засеяна заново: у контейнера нет volume (проверено двумя запусками подряд).

## Правки вживую

| Правка | Что будет |
|---|---|
| Убрать `CommitAsync` | Сборка чистая; `POST /orders` — `201` и `Location: /orders/N`, `GET` по этому адресу — `404`, в списке заказа нет: transaction откатилась при `Dispose` |
| Убрать `await using` у connection в `AllAsync`, затем `for i in $(seq 102); do curl -s -o /dev/null -w '%{http_code}\n' $AUTH localhost:5090/orders; done` | Сто ответов `200`, дальше каждый ждёт 15 секунд и `500`: `NpgsqlException: The connection pool has been exhausted, either raise 'Max Pool Size' (currently 100) or 'Timeout' (currently 15 seconds) in your connection string.` База при этом свободна |
| Привести `returning number` к `long` вместо `int` | `500`, `InvalidCastException`: `serial` — четырёхбайтовое целое; у `count(*)` наоборот |
| Спросить: что, если `ReadOrdersAsync` вызвать дважды на одной command, не закрыв первый Reader | `NpgsqlOperationInProgressException: A command is already in progress` — один Reader на connection |

`AllAsync` выбран для утечки не случайно: `GET /orders/{number}` после двенадцатой отвечает из
cache и до базы не доходит. По той же причине `total` после «сломайте нарочно» смотрят в списке.

## Вопросы и короткие ответы

- **Что живёт всё время, что делает `await using`?** *Пул настоящих соединений внутри
  `NpgsqlDataSource`. Объект connection — обёртка на секунды; `Dispose` возвращает соединение в пул,
  а не закрывает его.*
- **Сколько поездок?** *Одна: `left join`. С запросом позиций на каждый заказ — сто одна, это N + 1.*
- **Спросить сверх задания: почему конкатенация с `SelectOrders` в `FindAsync` — не инъекция?** *Обе части — константы из
  кода. Правило — ни одного байта снаружи в тексте; `number` уходит как parameter.*
- **Что с чужими позициями?** *Значение стало текстом программы: кавычка закрыла литерал, дальше
  выполнился `delete`, остаток строки закомментирован. Parameter уходит базе отдельно от текста и
  как SQL не разбирается.*
- **`order by @by` (дополнительное)?** *Ошибки нет и сортировки нет: это сортировка по константе.
  Проверено: `order by $1 desc` с `'customer_name'` вернул строки в прежнем порядке. Имена колонок
  не параметризуются — только белый список.*

## На что ещё обратить внимание

- **Номера после отката.** `serial` не возвращает номер при откате: после `500` или правки с
  `CommitAsync` в нумерации дыра. Если `GET /orders` нумерует заказы по индексу в списке (как
  `InMemoryOrderStore`), номера в списке разойдутся с `Location`. Лечится перезапуском AppHost; настоящее
  исправление — номер из базы — приходит с `Shop.Data` в четырнадцатой.
- **Статус при чтении.** `MoveTo` вызывается после позиций и только для статуса не `Created`: в
  домене нет конструктора «восстановить из базы». Если `MoveTo` у студента проверяет переходы,
  `Shipped` и дальше потребуют цепочки вызовов — в лабораторной таких заказов нет.
- **Позиции — по одной command.** Три позиции — три поездки внутри transaction. Batch из лекции
  здесь не требуется; хороший вопрос сильному студенту.
- **Spans на dashboard не проверялись на эталоне** (запуск был без браузера). `Aspire.Npgsql`
  подключает tracing драйвера сам; если spans базы в trace нет, проверьте, что `AddServiceDefaults()`
  из двенадцатой вызывается.

**Частые ошибки:** connection в поле класса или на весь HTTP-запрос; `GetOrdinal` или чтение по
имени внутри цикла; command без `transaction` в третьем шаге; забытый `CommitAsync`; `catch` с
`RollbackAsync` вместо `await using`; интерполяция «только для числа»; `select *`; SQL в
`Program.cs` вместо `schema.sql` и `PgOrderStore`.
