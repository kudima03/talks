# Лабораторная 14. Тот же магазин на EF Core: модель, migration и SQL рядом

**К лекции:** [Л14. EF Core: модель, миграции, запросы](../L14-ef-core.md)
**Условие:** всё из терминала, запросы — `curl`. Нужно решение из тринадцатой лабораторной: эта её
продолжает — `IOrderStore`, `PgOrderStore`, `AddOrders()`, `NpgsqlDataSource` из контейнера и
`Shop.AppHost` с базой `shopdb`. Token — из одиннадцатой. Порт в примерах — 5090.
**Сдаётся вживую:** отчёта нет. Вы показываете сервис и отвечаете на вопросы по любому шагу.

Вопрос **«усложнённый»** — только для тех, у кого основной ответ уже есть.

## 1. Shop.Data: модель и context

```sh
dotnet new classlib --name Shop.Data && dotnet sln add Shop.Data
dotnet add Shop.Data reference Shop.Domain && dotnet add Shop.Api reference Shop.Data
cd Shop.Data
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package EFCore.NamingConventions
dotnet add package Microsoft.EntityFrameworkCore.Relational --version "10.*"
dotnet add package Microsoft.EntityFrameworkCore.Design --version "10.*"
```

1. В `Shop.Domain` добавьте в `Order` закрытый конструктор без параметров — для EF Core:
   `private Order() { Customer = null!; }`. Больше домен не меняется.
2. `Shop.Data/Configurations/OrderConfiguration.cs` — класс `OrderConfiguration :
   IEntityTypeConfiguration<Order>` на те же таблицы, что в тринадцатой. Номера заказа в домене
   нет, поэтому ключ — shadow property. Тело `Configure`:

   ```csharp
   builder.ToTable("orders");
   builder.Property<int>("Number");
   builder.HasKey("Number");
   builder.Ignore(o => o.Id);                       // Guid домена в таблице не хранится
   builder.Property(o => o.Status).HasConversion<string>();
   builder.OwnsOne(o => o.Customer, customer =>     // customer_email, customer_name
       customer.Property(c => c.Email).HasConversion(e => e.Value, v => new Email(v)));
   builder.Navigation(o => o.Customer).IsRequired();
   builder.HasMany(o => o.Items).WithOne()
       .HasForeignKey("OrderNumber").IsRequired()
       .OnDelete(DeleteBehavior.Cascade);
   ```

3. Рядом напишите `OrderItemConfiguration` сами: таблица `order_items`, shadow-ключ `Id`, а
   `Price` — через converter `Money` ↔ `decimal` и `HasPrecision(12, 2)`.
4. `ShopDbContext` — по листингу лекции: primary constructor с `DbContextOptions<ShopDbContext>`,
   один `DbSet<Order> Orders` и `ApplyConfigurationsFromAssembly` в `OnModelCreating`.

**Вопрос:** почему всё про таблицы лежит в `Shop.Data`, а не атрибутами на `Order`? На какие
пакеты ссылается `Shop.Domain` после этого шага?

## 2. Migration вместо SQL-скрипта

1. Из папки решения: `dotnet new tool-manifest` (если его ещё нет) и
   `dotnet tool install dotnet-ef --version "10.*"`.
2. `dotnet ef` должен создать context, не запуская сервис. Положите в `Shop.Data`:

   ```csharp
   public sealed class ShopDbContextFactory : IDesignTimeDbContextFactory<ShopDbContext>
   {
       public ShopDbContext CreateDbContext(string[] args) =>
           new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>()
               .UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStrings__shopdb"))
               .UseSnakeCaseNamingConvention()
               .Options);
   }
   ```

3. `dotnet ef migrations add Initial --project Shop.Data`. Откройте файл в `Shop.Data/Migrations`
   и сравните со скриптом тринадцатой: найдите то, чего вы не писали ни там, ни в конфигурации.
4. SQL-скрипт больше не нужен: уберите его подключение из `Shop.AppHost` и запустите AppHost —
   база должна быть пустой (data volume из тринадцатой, если он был, удалите). Connection string
   базы `shopdb` возьмите в dashboard и примените migration отдельной командой:

   ```sh
   export ConnectionStrings__shopdb='Host=localhost;Port=...;Username=postgres;Password=...;Database=shopdb'
   dotnet ef database update --project Shop.Data
   dotnet ef migrations list --project Shop.Data
   ```

   В PowerShell — `$env:ConnectionStrings__shopdb = '...'`. Без data volume база живёт, пока
   работает AppHost: после перезапуска повторите `database update`.

**Вопрос:** почему migration применяет отдельная команда, а не `Shop.Api` на старте? Что
случится при двух экземплярах сервиса?

## 3. EfOrderStore и сравнение SQL

1. В `Shop.Api` рядом с `PgOrderStore` (он остаётся) положите `EfOrderStore` и допишите
   `AllAsync`: `Include`, сортировка по `Number`, `AsNoTracking`, `ToListAsync`.

   ```csharp
   public sealed class EfOrderStore(ShopDbContext context) : IOrderStore
   {
       public Task<Order?> FindAsync(int number, CancellationToken ct) =>
           context.Orders
               .Include(o => o.Items)
               .AsNoTracking()
               .FirstOrDefaultAsync(o => EF.Property<int>(o, "Number") == number, ct);

       public async Task<int> AddAsync(Order order, CancellationToken ct)
       {
           context.Orders.Add(order);
           await context.SaveChangesAsync(ct);
           return context.Entry(order).Property<int>("Number").CurrentValue;
       }
   }
   ```

2. В `AddOrders()` зарегистрируйте context и замените одну регистрацию store:

   ```csharp
   services.AddDbContext<ShopDbContext>((sp, options) => options
       .UseNpgsql(sp.GetRequiredService<NpgsqlDataSource>())
       .UseSnakeCaseNamingConvention());
   services.AddScoped<IOrderStore, EfOrderStore>();   // было: PgOrderStore
   ```

3. Запустите AppHost, создайте два заказа (`curl --json @new-order.json` с token на `/orders`) и
   запросите `/orders/2`. SQL — в log сервиса, категория
   `Microsoft.EntityFrameworkCore.Database.Command`; ответ из cache двенадцатой SQL не даёт —
   смотрите первый запрос заказа. Откройте рядом `PgOrderStore.FindAsync` и сравните.
4. **Сломайте нарочно:** уберите `.Include(o => o.Items)` из `FindAsync`, создайте ещё один заказ
   и запросите его. Посмотрите на `total` и на SQL, затем верните строку.

**Вопрос:** чем сгенерированный SQL для `GET /orders/{number}` отличается от вашего из
тринадцатой: сколько запросов, какие колонки, откуда `LIMIT` и подзапрос?

**Вопрос (усложнённый):** без `Include` сервис ответил `"total":0` — почему нет ни ошибки, ни
второго запроса? Что изменилось бы при включённом lazy loading на `GET /orders`?

Сборка без предупреждений, коммит — вместе с папкой `Migrations`.

## Дополнительное задание (по желанию)

**Вторая migration.** В lambda `OwnsOne` добавьте `customer.HasIndex(c => c.Email);`, выполните
`dotnet ef migrations add AddOrderCustomerEmailIndex --project Shop.Data`, а до `database update` —
`dotnet ef migrations script --idempotent --project Shop.Data`, и прочитайте скрипт.
**Вопрос:** что он сделает при втором запуске? Почему нельзя было дописать index в `Initial`?

## Как проходит приёмка

Принимающий выполняет `curl` на ваши маршруты, смотрит с вами SQL в log и файл migration и может
попросить изменить одну строку — зарегистрировать `EfOrderStore` как singleton или убрать
`UseSnakeCaseNamingConvention()` из регистрации — и объяснить, что сломалось.
