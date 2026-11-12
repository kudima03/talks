# Лабораторная 10. Сборка сервиса: DI, options и typed client

**К лекции:** [Л10. DI, исходящие вызовы и конфигурация](../L10-di-and-configuration.md)
**Условие:** всё из терминала, запросы — `curl`. Нужны `Shop.Domain` и `Shop.Api` из девятой
лабораторной: `OrderStore`, группа `/orders`, `OrderStatsService`, `CreateOrderDtoValidator`,
`ErrorHandlingMiddleware`. Порт в примерах — 5090; ваш — в строке `Now listening on`.
**Сдаётся вживую:** отчёта нет. Вы показываете сервис и отвечаете на вопросы по любому шагу.

Вопрос **«усложнённый»** — только для тех, у кого основной ответ уже есть.

## 1. `IOrderStore` и модуль `AddOrders()`

1. Переименуйте `OrderStore` в `InMemoryOrderStore` и выделите из него интерфейс. Данные те же,
   методы — `Task.FromResult(...)` под `lock`:

   ```csharp
   public interface IOrderStore
   {
       Task<IReadOnlyList<Order>> AllAsync(CancellationToken ct);
       Task<Order?> FindAsync(int number, CancellationToken ct);
       Task<int> AddAsync(Order order, CancellationToken ct);
   }
   ```

2. Handlers группы `/orders` и `OrderStatsService` принимают `IOrderStore`; handlers становятся
   `async` и передают в store свой `CancellationToken ct`.
3. Регистрации заказов уезжают из `Program.cs` в метод расширения
   `public static IServiceCollection AddOrders(this IServiceCollection services)` класса
   `OrdersModule`: `services.AddSingleton<IOrderStore, InMemoryOrderStore>();`, регистрация
   validators из девятой лабораторной и `return services;`. В `Program.cs` остаётся
   `builder.Services.AddOrders();`. Проверьте `curl` на `/orders`.
4. Сломайте нарочно: замените `AddSingleton` на `AddScoped`, выполните `dotnet run`, прочитайте
   сообщение об ошибке и верните singleton.

**Вопрос:** кто в вашем сервисе оказался singleton, который держит scoped? Что было бы с
заказами, если бы контейнер это пропустил, — и где он пропускает?

## 2. `ShopOptions` с проверкой на старте

1. Секция в `appsettings.json` (порт — ваш):

   ```json
   "Shop": { "Currency": "BYN", "RatesUrl": "http://localhost:5090/stub/" }
   ```

2. Класс настроек и его регистрация — в `AddOrders()`:

   ```csharp
   public sealed class ShopOptions
   {
       public const string Section = "Shop";
       [Required]
       public string Currency { get; set; } = "";
       [Required, Url]
       public string RatesUrl { get; set; } = "";
   }

   services.AddOptions<ShopOptions>()
       .BindConfiguration(ShopOptions.Section)
       .ValidateDataAnnotations()
       .ValidateOnStart();
   ```

**Вопрос:** в `RatesUrl` опечатка. Когда вы узнаете об этом с `ValidateOnStart()` и когда — без
него? Что в это время отвечает `/health`?

## 3. Typed client и маршрут `/orders/{number}/total`

1. `dotnet add package Microsoft.Extensions.Http.Resilience`. Курсы отдаёт заглушка в том же
   сервисе — маршрут вне группы `/orders`:

   ```csharp
   public sealed record RateResponse(string Currency, decimal Rate);
   app.MapGet("/stub/rates/{currency}", (string currency) => currency switch
   {
       "USD" => Results.Ok(new RateResponse("USD", 0.31m)),
       "EUR" => Results.Ok(new RateResponse("EUR", 0.27m)),
       _ => Results.NotFound(),
   });
   ```

2. Напишите `RatesClient` по листингу лекции: конструктор `(HttpClient http)`, метод
   `Task<decimal> GetRateAsync(string currency, CancellationToken ct)`, внутри —
   `http.GetFromJsonAsync<RateResponse>($"rates/{currency}", ct)`.
3. Регистрация в `AddOrders()` — адрес из настроек, а не по ключу:

   ```csharp
   services.AddHttpClient<RatesClient>((provider, client) =>
           client.BaseAddress = new Uri(provider.GetRequiredService<IOptions<ShopOptions>>().Value.RatesUrl))
       .AddStandardResilienceHandler();
   ```

4. Маршрут `orders.MapGet("/{number:int}/total", ...)` с параметрами `int number`,
   `string? currency`, `IOrderStore`, `RatesClient`, `IOptions<ShopOptions>`, `CancellationToken`.
   Нет заказа — `NotFoundException`. Валюта не задана или равна `Currency` из настроек — сумма
   как есть; иначе сумма × курс, `Math.Round(..., 2)`. Ответ —
   `record OrderTotalDto(int Number, decimal Total, string Currency)`.
5. Выполните `curl` на `/orders/1/total` и `/orders/1/total?currency=USD`. Перезапустите сервис
   командой `Shop__Currency=EUR dotnet run` и повторите первый запрос.

**Вопрос:** в `appsettings.json` по-прежнему `BYN`. Откуда взялось `EUR` и почему в имени
переменной два подчёркивания?

**Вопрос (усложнённый):** выполните `curl -i` на `/orders/1/total?currency=XXX`. Какой статус
получил клиент, что в log, и где по лекции должна жить эта ошибка?

Сборка без предупреждений, коммит.

## Дополнительное задание (по желанию)

**Настройки без перезапуска.** В handler `/total` замените `IOptions<ShopOptions>` на
`IOptionsMonitor<ShopOptions>` и `Value` на `CurrentValue`. Не останавливая сервис, поменяйте
`Currency` в `appsettings.json` и повторите `curl` на `/orders/1/total`.

**Вопрос:** почему тот же приём не сработает для `Shop__Currency`?

## Как проходит приёмка

Принимающий выполняет `curl` на ваши маршруты, смотрит с вами в log и может попросить изменить
одну строку — убрать `ValidateOnStart()`, испортить `Shop__RatesUrl` или направить его на
свободный порт — и заранее сказать, что произойдёт.
