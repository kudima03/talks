# Лабораторная 15. Тесты сервиса: unit, integration и analyzers

**К лекции:** [Л15. Тестирование и качество](../L15-testing.md)
**Условие:** всё из терминала. Нужно решение из четырнадцатой лабораторной: `Shop.Domain`, `Shop.Data`
(`ShopDbContext` и migrations), `Shop.Api` (`EfOrderStore`, `CreateOrderDtoValidator`, policy `Admin`,
`OrderOwnerHandler`); connection names `shopdb` и `cache` — как в AppHost. Docker запущен. Запросы
здесь шлёт не `curl`, а test; сервис и AppHost запускать не нужно.
**Сдаётся вживую:** отчёта нет. Вы показываете `dotnet test` и отвечаете на вопросы по любому шагу.

## 1. Unit tests: domain и validator

`dotnet new xunit --name Shop.Tests`, `dotnet sln add Shop.Tests`, `dotnet add Shop.Tests reference Shop.Api`.

1. Класс `OrderTests`. `[Fact]`: новый заказ после `MoveTo(OrderStatus.Paid)` — в статусе `Paid`.
   `[Theory]` с двумя `[InlineData]`: из `Created` нельзя в `Shipped` и в `Delivered` —
   `Assert.Throws<InvalidTransitionException>`. Если ваши методы называются иначе — берите свои.
2. Класс `CreateOrderDtoValidatorTests`. Validator создаётся через `new`; рядом — private static
   метод `Dto(int quantity)`, который собирает корректный `CreateOrderDto` с одной позицией. `[Fact]`:
   `Dto(3)` — `IsValid`. `[Theory]` для `0` и `-1`: в `Errors` есть `PropertyName == "Items[0].Quantity"`.
3. Правила лекции: имя — `Метод_Условие_Ожидание`, в теле три абзаца, один assert на test. Запуск:
   `dotnet test`, затем `dotnet test --filter "FullyQualifiedName~OrderTests"`.

**Вопрос:** сколько строк в отчёте дал `[Theory]` с двумя `[InlineData]`? Почему переход в `Paid` —
отдельный `[Fact]`, а не третья строка данных?

## 2. Integration tests: настоящий host, настоящий PostgreSQL

Пакеты в `Shop.Tests`: `Microsoft.AspNetCore.Mvc.Testing`, `Testcontainers.PostgreSql`, `Testcontainers.Redis`.

1. Fixture — готовый код. Containers стартуют один раз на класс, migrations применяются с нуля,
   JWT заменён test scheme. Claims назовите так, как их читает ваш код из одиннадцатой лабораторной.

   ```csharp
   public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
   {
       private readonly PostgreSqlContainer db = new PostgreSqlBuilder("postgres:17-alpine").Build();
       private readonly RedisContainer cache = new RedisBuilder("redis:7-alpine").Build();

       public async Task InitializeAsync()
       {
           await Task.WhenAll(db.StartAsync(), cache.StartAsync());
           using IServiceScope scope = Services.CreateScope();
           await scope.ServiceProvider.GetRequiredService<ShopDbContext>().Database.MigrateAsync();
       }
       public new async Task DisposeAsync()
       {
           await base.DisposeAsync();
           await db.DisposeAsync();
           await cache.DisposeAsync();
       }
       protected override void ConfigureWebHost(IWebHostBuilder builder)
       {
           builder.UseSetting("ConnectionStrings:shopdb", db.GetConnectionString());
           builder.UseSetting("ConnectionStrings:cache", cache.GetConnectionString());
           builder.ConfigureTestServices(services => services.AddAuthentication("Test")
               .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { }));
       }
   }

   public sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
       ILoggerFactory logger, UrlEncoder encoder)
       : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
   {
       protected override Task<AuthenticateResult> HandleAuthenticateAsync()
       {
           string? email = Request.Headers["X-Test-User"];
           if (email is null) return Task.FromResult(AuthenticateResult.NoResult());
           string role = Request.Headers["X-Test-Role"].ToString();
           Claim[] claims = [new(ClaimTypes.Email, email), new(ClaimTypes.Role, role)];
           ClaimsPrincipal user = new(new ClaimsIdentity(claims, Scheme.Name));
           return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(user, Scheme.Name)));
       }
   }
   ```

2. Класс `OrdersApiTests(ApiFactory factory) : IClassFixture<ApiFactory>`. Первым — самый простой
   test: `GET /health` через `factory.CreateClient()` отвечает `200`. Пока он красный, дальше не идите.
3. Четыре contract tests — запрос, статус, тело; ни одного обращения к `ShopDbContext`. Пользователь
   — header `X-Test-User` в `client.DefaultRequestHeaders`:
   - `GET /orders/1` без header — `401`;
   - `POST /orders` от `ann@example.com` (`PostAsJsonAsync`, тело — как `new-order.json` из девятой
     лабораторной) — `201`; `GET` по `Location` возвращает то же тело;
   - тот же `POST` с `quantity = 0` — `422`, и в `errors` есть ключ `items[0].quantity`;
   - заказ, созданный `ann@example.com`, для `bob@example.com` — `404`.

**Вопрос:** что в этих tests настоящее, а что подменено? Какие части сервиса из лабораторных 8–14
проверил один test на `422`?

## 3. Analyzers: правила при сборке

1. В корне решения — `Directory.Build.props`: `<Project>`, в нём `<PropertyGroup>` с тремя свойствами
   из лекции, но `AnalysisLevel` — `latest-recommended`. С `latest-all` существующий код даёт десятки
   ошибок сразу, а в такой проект, как советует лекция, правила вводят по одному.

2. Рядом — `.editorconfig`: одно правило поднято до ошибки, одно выключено для tests, с обоснованием.

   ```ini
   root = true
   [*.cs]
   dotnet_diagnostic.CA1849.severity = error
   # test names are Method_Condition_Expected
   [Shop.Tests/**.cs]
   dotnet_diagnostic.CA1707.severity = none
   ```

3. `dotnet build`. Каждую ошибку исправьте в коде; подавлять можно только то, что готовы обосновать.
4. Сломайте нарочно: в `async` handler `POST /orders` добавьте `using StreamReader reader =
   new("appsettings.json");` и `reader.ReadLine();`. Соберите, прочитайте ошибку, уберите обе строки.

**Вопрос:** какое правило сработало на `ReadLine` и что оно предлагает? Почему `CA1707` выключено
только в `Shop.Tests`, а не во всём решении?

Сборка без предупреждений, `dotnet test` зелёный, commit.

## Дополнительное задание (по желанию)

**Benchmark.** Проект `Shop.Benchmarks` (`dotnet new console`, пакет `BenchmarkDotNet`): класс с
`[MemoryDiagnoser]` и `[Params(10, 1_000)]` позиций в заказе; `Total()` из пятой лабораторной —
`[Benchmark(Baseline = true)]`, тот же подсчёт циклом `foreach` — второй. Запуск: `dotnet run -c Release`.

**Вопрос:** разница между вариантами больше ошибки? Что ответил инструмент без `-c Release`?

## Как проходит приёмка

Принимающий запускает `dotnet test`, просит показать один test и назвать его три абзаца, и может
попросить изменить одну строку — убрать `MigrateAsync`, поменять местами аргументы `Assert.Equal`
или вернуть `ReadLine` — и заранее сказать, что станет красным.
