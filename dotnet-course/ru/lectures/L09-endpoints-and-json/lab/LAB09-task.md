# Лабораторная 9. Контракт сервиса: DTO, валидация и формат ошибки

**К лекции:** [Л9. Эндпоинты, контракт и JSON](../L09-endpoints-and-json.md)
**Условие:** всё из терминала, запросы — `curl`. Нужны `Shop.Domain` и `Shop.Api` из восьмой
лабораторной: эта её продолжает. Порт в примерах — 5090; ваш — в строке `Now listening on`.
**Сдаётся вживую:** отчёта нет. Вы показываете сервис и отвечаете на вопросы по любому шагу.

Вопрос **«усложнённый»** — только для тех, у кого основной ответ уже есть.

## 1. DTO и группа маршрутов

1. Положите в `Shop.Api` файл `Contracts.cs` с тремя DTO и допишите два метода расширения:

   ```csharp
   public sealed record OrderDto(int Number, string CustomerEmail, OrderStatus Status, decimal Total);
   public sealed record OrderItemDto(string Product, int Quantity, decimal Price);
   public sealed record CreateOrderDto(string CustomerEmail, string CustomerName, List<OrderItemDto> Items);

   public static class OrderMapping
   {
       public static OrderDto ToDto(this Order order, int number) => ...;   // Total() из пятой лабораторной
       public static Order ToOrder(this CreateOrderDto dto) { ... }          // Customer, Add по позициям
   }
   ```

2. Добавьте в `OrderStore` метод `int Add(Order order)`: он кладёт заказ в список под `lock` и
   возвращает его номер — `orders.Count`.
3. В `Program.cs` замените два handler `/orders` из восьмой лабораторной группой:
   `RouteGroupBuilder orders = app.MapGroup("/orders");`. В ней:
   - `orders.MapGet("/", ...)` — `TypedResults.Ok` со списком `OrderDto`; необязательный
     параметр `decimal? over` из query string оставляет заказы дороже суммы, как `list over` в
     четвёртой лабораторной;
   - `orders.MapGet("/{number:int}", ...)` — как раньше бросает `NotFoundException`, но
     возвращает `TypedResults.Ok(order.ToDto(number))`;
   - `orders.MapPost("/", (CreateOrderDto dto, OrderStore store) => ...)` —
     `TypedResults.Created($"/orders/{number}", ...)`.
4. Статус — строкой, а не числом: одна строка до `Build`, настройки JSON для всего сервиса.

   ```csharp
   builder.Services.ConfigureHttpJsonOptions(options =>
       options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
   ```

5. Положите рядом `new-order.json` и выполните `curl -i --json @new-order.json` на `/orders`,
   затем `curl -i` на адрес из `Location`:

   ```json
   {"customerEmail":"cat@example.com","customerName":"Cat Lee","items":[{"product":"Cocoa","quantity":3,"price":90.00}]}
   ```

**Вопрос:** почему наружу уходит `OrderDto`, а не `Order`? Верните из `/orders/1`
`TypedResults.Ok(order)` и посмотрите, что получил клиент.

## 2. Валидация в endpoint filter

1. `dotnet add package FluentValidation.DependencyInjectionExtensions`.
2. Validator — правила формы, без обращения к данным:

   ```csharp
   public sealed class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
   {
       public CreateOrderDtoValidator()
       {
           RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress();
           RuleFor(x => x.CustomerName).NotEmpty();
           RuleFor(x => x.Items).NotEmpty().WithMessage("Order must have at least one item");
           RuleForEach(x => x.Items).ChildRules(item =>
           {
               item.RuleFor(i => i.Product).NotEmpty();
               item.RuleFor(i => i.Quantity).GreaterThan(0);
               item.RuleFor(i => i.Price).GreaterThanOrEqualTo(0);
           });
       }
   }
   ```

3. Filter из лекции, обобщённый по типу DTO: класс `ValidationFilter<T> : IEndpointFilter`, в
   `InvokeAsync` — `context.Arguments.OfType<T>().FirstOrDefault()`, validator из
   `context.HttpContext.RequestServices`, при ошибках `throw new ValidationException(result.Errors)`,
   иначе `return await next(context)`.
4. Регистрация: `builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderDtoValidator>();`
   до `Build` и `.AddEndpointFilter<ValidationFilter<CreateOrderDto>>()` на группе.

**Вопрос:** почему filter бросает исключение, а не возвращает `400`, и почему проверки нет в
handler `MapPost`?

## 3. Формат ошибки — в одном месте

1. В switch `ErrorHandlingMiddleware` из восьмой лабораторной добавьте две ветки выше `_`:
   `ValidationException` → `422` и `Validation failed`; `BadHttpRequestException bad` →
   `bad.StatusCode` и `Bad request`.
2. Для `422` добавьте в ответ поле `errors` — словарь «поле → сообщения». Имя поля — как в JSON:

   ```csharp
   string field = string.Join('.', failure.PropertyName.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
   ```

3. Положите `bad-order.json` — тот же заказ с почтой `not-an-email` и `"quantity":0` — и
   выполните `curl -i` на: `--json @bad-order.json` на `/orders`, `/orders/abc`, `/orders?over=`.

**Вопрос:** какие из трёх ответов собрал ваш middleware, а какой — нет? Что вернул бы
`/orders?over=` без ветки `BadHttpRequestException`?

**Вопрос (усложнённый):** сравните `/orders?over=150.5` и `/orders?over=150,5`. Что получил клиент
во втором случае и почему ошибки нет?

Сборка без предупреждений, коммит.

## Дополнительное задание (по желанию)

**Экспорт потоком.** Метод расширения `async IAsyncEnumerable<OrderDto> ExportAsync(this OrderStore
store, [EnumeratorCancellation] CancellationToken ct = default)` с `await Task.Delay(500, ct)` перед
каждым `yield return` и маршрут `orders.MapGet("/export", ...)`. Выполните
`curl -i -N` и посмотрите на заголовки и на то, как приходит тело.

**Вопрос:** какой заголовок говорит, что длина заранее неизвестна? Что скажет сборка без
`[EnumeratorCancellation]`?

## Как проходит приёмка

Принимающий выполняет `curl` на ваши маршруты — правильные, невалидные и со сломанным JSON,
смотрит с вами в log и может попросить изменить одну строку — убрать регистрацию validators или
ветку `BadHttpRequestException` — и объяснить, что сломалось.
