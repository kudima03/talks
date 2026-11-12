# Лабораторная 13. Хранилище на голом ADO.NET: PostgreSQL вместо памяти

**К лекции:** [Л13. Данные: семейства баз и ADO.NET](../L13-databases-and-ado.md)
**Условие:** всё из терминала, запросы — `curl`. Нужны `Shop.AppHost` и `Shop.Api` из двенадцатой
лабораторной: эта её продолжает. Из прошлых берём `IOrderStore`, `InMemoryOrderStore` и `AddOrders()`
(десятая), token (одиннадцатая), запуск через AppHost и dashboard (двенадцатая). Нужен Docker.
Порт в примерах — 5090; `$AUTH` — ваш заголовок `-H "Authorization: Bearer ..."` с token администратора.
**Сдаётся вживую:** отчёта нет. Вы показываете сервис и отвечаете на вопросы по любому шагу.

## 1. PostgreSQL в AppHost и схема

1. Пакеты: `dotnet add Shop.AppHost package Aspire.Hosting.PostgreSQL` и
   `dotnet add Shop.Api package Aspire.Npgsql`.
2. В AppHost рядом с `cache`: `var shopdb = builder.AddPostgres("postgres").AddDatabase("shopdb");`,
   а у ресурса `Shop.Api` — `.WithReference(shopdb).WaitFor(shopdb)`.
3. В `Program.cs` сервиса до `Build`: `builder.AddNpgsqlDataSource("shopdb");`. Строка регистрирует
   `NpgsqlDataSource` из лекции; connection string приходит от AppHost как `ConnectionStrings:shopdb`.
4. Положите в `Shop.Api` файл `schema.sql`:

   ```sql
   create table if not exists orders (number serial primary key,
       customer_email text not null, customer_name text not null, status text not null);
   create table if not exists order_items (id serial primary key,
       order_number int not null references orders (number),
       product text not null, quantity int not null, price numeric(12,2) not null);

   insert into orders (customer_email, customer_name, status)
   select * from (values ('ann@example.com', 'Ann Smith', 'Paid'),
                         ('bob@example.com', 'Bob Brown', 'Created')) as seed
   where not exists (select 1 from orders);
   insert into order_items (order_number, product, quantity, price)
   select * from (values (1, 'Green tea', 2, 150.00), (2, 'Coffee', 1, 120.00)) as seed
   where not exists (select 1 from order_items);
   ```

5. Выполните его при старте, сразу после `Build`: текст файла — `File.ReadAllTextAsync` по пути от
   `app.Environment.ContentRootPath`, `NpgsqlDataSource` — из `app.Services`, connection —
   `OpenConnectionAsync()`, command — `new NpgsqlCommand(schema, connection)` и
   `ExecuteNonQueryAsync()`. Connection и command — под `await using`. Запустите AppHost: на
   dashboard появились `postgres` и `shopdb`, сервис стартовал без ошибок.

**Вопрос:** `NpgsqlDataSource` — singleton, а connection вы открыли и тут же закрыли. Что из двух
живёт всё время работы сервиса и что на самом деле делает `await using` с connection?

## 2. Чтение: parameter и Reader

1. Класс `PgOrderStore(NpgsqlDataSource dataSource) : IOrderStore`. Один SQL на оба чтения и
   сборка заказов из строк — готовые, кроме двух мест с `...`: номера колонок через
   `reader.GetOrdinal` (один раз, до цикла) и проверка `null` для заказа без позиций.

   ```csharp
   private const string SelectOrders = """
       select o.number, o.customer_email, o.customer_name, o.status, i.product, i.quantity, i.price
       from orders o left join order_items i on i.order_number = o.number
       """;

   private static async Task<List<Order>> ReadOrdersAsync(NpgsqlCommand command, CancellationToken ct)
   {
       var (orders, statuses, current) = (new List<Order>(), new List<OrderStatus>(), 0);
       await using var reader = await command.ExecuteReaderAsync(ct);
       int number = ..., email = ..., name = ..., status = ..., product = ..., quantity = ..., price = ...;
       while (await reader.ReadAsync(ct))
       {
           if (reader.GetInt32(number) != current)
           {
               current = reader.GetInt32(number);
               orders.Add(new Order(new Customer(new Email(reader.GetString(email)), reader.GetString(name))));
               statuses.Add(Enum.Parse<OrderStatus>(reader.GetString(status)));
           }
           if (...)   // у заказа есть позиция
               orders[^1].Add(new OrderItem(reader.GetString(product), reader.GetInt32(quantity),
                   new Money(reader.GetDecimal(price))));
       }
       for (var i = 0; i < orders.Count; i++)
           if (statuses[i] != OrderStatus.Created) orders[i].MoveTo(statuses[i]);
       return orders;
   }
   ```

2. `AllAsync`: connection через `await using`, command с `SelectOrders + " order by o.number, i.id"`,
   результат `ReadOrdersAsync`. `FindAsync`: то же с `" where o.number = @number order by i.id"` и
   `command.Parameters.AddWithValue("number", number)`; первый заказ или `null`. `AddAsync` пока
   бросает `NotImplementedException`.
3. В `AddOrders()` замените одну регистрацию: `AddSingleton<IOrderStore, PgOrderStore>()`.
4. `curl -i $AUTH` на `/orders`, `/orders/1` и `/orders/7`.

**Вопрос:** сколько раз `GET /orders` ходит в базу на сто заказов? Сколько было бы, если позиции
читать отдельным запросом на каждый заказ, и как это называется?

## 3. Запись в транзакции

1. `AddAsync` — одна transaction на connection: `await using var transaction = await
   connection.BeginTransactionAsync(ct);`, каждая command создаётся с `(sql, connection, transaction)`.
2. Заказ — `ExecuteScalarAsync`, результат приводится к `int`:
   `insert into orders (customer_email, customer_name, status) values (@email, @name, @status) returning number`.
3. Каждая позиция — `ExecuteNonQueryAsync`:
   `insert into order_items (order_number, product, quantity, price) values (@number, @product, @quantity, @price)`.
4. В конце — `await transaction.CommitAsync(ct);` и `return number;`. Ни `try`, ни `RollbackAsync`.
5. `curl -i $AUTH --json @new-order.json` на `/orders` (файл из девятой лабораторной), затем `curl`
   на адрес из `Location`. На dashboard откройте trace этого `POST`: в нём spans запросов к базе.

**Сломайте нарочно.** В SQL позиции замените `@product` на `'{item.Product}'` (строка стала
интерполированной, `AddWithValue("product", ...)` уберите) и отправьте `evil-order.json`:

```json
{"customerEmail":"cat@example.com","customerName":"Cat Lee","items":[{"product":"Tea', 1, 1); delete from order_items; --","quantity":1,"price":1.00}]}
```

Посмотрите на `total` в `GET /orders`. Верните parameter, перезапустите AppHost (база создастся
заново) и отправьте тот же файл ещё раз.

**Вопрос:** что случилось с позициями чужих заказов и почему с parameter та же строка безвредна?

Сборка без предупреждений, коммит.

## Дополнительное задание (по желанию)

**Сортировка по имени колонки.** Метод `PgOrderStore.AllSortedAsync(string by, CancellationToken ct)`:
`by` — `number`, `customer_name` или `status`; имя колонки выбирается через `switch` из констант в
коде, неизвестное значение — `ArgumentException`. Маршрут для проверки — на ваш выбор.
**Вопрос:** что вернёт запрос, если написать `order by @by` и передать имя колонки как parameter?

## Как проходит приёмка

Принимающий выполняет `curl` на ваши маршруты, смотрит с вами SQL в `PgOrderStore` и trace на
dashboard и может попросить изменить одну строку — убрать `CommitAsync` или `await using` у
connection — и объяснить, что сломалось.
