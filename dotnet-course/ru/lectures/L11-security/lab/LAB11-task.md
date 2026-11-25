# Лабораторная 11. Кто пришёл и что ему можно: JWT, policy и доступ к ресурсу

**К лекции:** [Л11. Безопасность: аутентификация и авторизация](../L11-security.md)
**Условие:** всё из терминала, запросы — `curl`. Нужен `Shop.Api` из десятой лабораторной: `IOrderStore`,
`AddOrders()`, группа `/orders` с маршрутом `/{number:int}/total`. Порт в примерах — 5090; ваш — в
строке `Now listening on`. Сервис работает в одном терминале, `curl` — в другом.
**Сдаётся вживую:** отчёта нет. Вы показываете сервис и отвечаете на вопросы по любому шагу.

Вопрос **«усложнённый»** — только для тех, у кого основной ответ уже есть.

## 1. Token и authentication

```sh
cd Shop.Api
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version "10.0.*"
ANN=$(dotnet user-jwts create --name ann --claim email=ann@example.com -o token)
BOB=$(dotnet user-jwts create --name bob --claim email=bob@example.com -o token)
ROOT=$(dotnet user-jwts create --name root --claim email=root@example.com --role admin -o token)
```

Без `--version` придёт пакет под .NET 11, и restore остановится на `NU1202`.

1. В `Program.cs` до `Build` — scheme и authorization; группа `/orders` требует пользователя,
   `/health` остаётся как был:

   ```csharp
   builder.Services.AddAuthentication().AddJwtBearer();
   builder.Services.AddAuthorization();

   RouteGroupBuilder orders = app.MapGroup("/orders").RequireAuthorization();
   ```

2. Выполните `curl -i` на `/orders/1` и на `/health` без token, затем с token:
   `curl -i -H "Authorization: Bearer $ANN" http://localhost:5090/orders/1`.
3. Посмотрите, что внутри token: `dotnet user-jwts list`, затем `dotnet user-jwts print <ID> --show-all`.
4. **Сломайте нарочно:** допишите к token один символ — `"Authorization: Bearer ${ANN}x"` — и
   повторите запрос.

**Вопрос:** `401` в вашем коде нет нигде. Кто его отправил и чем ответ на испорченный token
отличается от ответа без token?

**Вопрос (усложнённый):** `user-jwts` изменил два файла проекта — `git diff` покажет какие, — но
ключа подписи в них нет. Где он лежит — `dotnet user-secrets list` — и почему не в репозитории?

## 2. Policy по role

1. Замените `AddAuthorization()` на builder с policy; имя policy — константой, а не строкой в двух
   местах:

   ```csharp
   builder.Services.AddAuthorizationBuilder()
       .AddPolicy("Admin", policy => policy.RequireRole("admin"));
   ```

2. Список всех заказов — только администратору: на `orders.MapGet("/", ...)` добавьте
   `.RequireAuthorization("Admin")`.
3. Выполните `curl -i` на `/orders` с `$ANN` и с `$ROOT`.

**Вопрос:** почему Ann получила `403`, а не `401`? Какой из двух ответов — challenge, какой — forbid?

## 3. Доступ к ресурсу, а не только к маршруту

Сначала убедитесь, что проблема есть: `curl -H "Authorization: Bearer $BOB" http://localhost:5090/orders/1` —
Bob читает заказ Ann.

1. Requirement и handler — файл `OrderOwner.cs`. Условие допишите сами: владелец — тот, у кого claim
   `email` совпадает с почтой покупателя заказа; администратору можно всё.

   ```csharp
   public sealed class OrderOwnerRequirement : IAuthorizationRequirement;

   public sealed class OrderOwnerHandler : AuthorizationHandler<OrderOwnerRequirement, Order>
   {
       protected override Task HandleRequirementAsync(
           AuthorizationHandlerContext context, OrderOwnerRequirement requirement, Order order)
       {
           string? email = context.User.FindFirstValue(ClaimTypes.Email);
           // owner or admin -> context.Succeed(requirement);
           return Task.CompletedTask;
       }
   }
   ```

2. Регистрация — в `AddOrders()`: `services.AddSingleton<IAuthorizationHandler, OrderOwnerHandler>();`.
3. В handlers `/{number:int}` и `/{number:int}/total` добавьте параметры `ClaimsPrincipal user,
   IAuthorizationService auth` и сразу после загрузки заказа — проверку. Чужой заказ отвечает так же,
   как несуществующий:

   ```csharp
   AuthorizationResult access = await auth.AuthorizeAsync(user, order, new OrderOwnerRequirement());
   if (!access.Succeeded) throw new NotFoundException($"no order {number}");
   ```

4. `POST /orders` берёт почту покупателя из token, а не из тела (`ClaimsPrincipal user` — параметром):

   ```csharp
   string email = user.FindFirstValue(ClaimTypes.Email)
       ?? throw new BadHttpRequestException("Token has no email claim", StatusCodes.Status403Forbidden);
   Order order = (dto with { CustomerEmail = email }).ToOrder();
   ```

5. Выполните `curl -i` на `/orders/1` с `$BOB`, `$ANN` и `$ROOT`, затем создайте заказ от имени Bob
   файлом из девятой лабораторной: `curl -i -H "Authorization: Bearer $BOB" --json @new-order.json
   http://localhost:5090/orders`.

**Вопрос:** почему владельца проверяет handler после загрузки заказа, а не `RequireAuthorization` на
маршруте? Чья почта оказалась в созданном заказе и почему не та, что в файле?

Сборка без предупреждений, коммит.

## Дополнительное задание (по желанию)

**Второй issuer без изменения кода.** Поднимите Keycloak в контейнере (`quay.io/keycloak/keycloak`,
`start-dev`), заведите realm, client и пользователя с почтой. Переключите сервис на него только
значениями configuration в секции `Authentication:Schemes:Bearer` — адрес issuer и audience.

**Вопрос:** что пришлось поменять, кроме двух значений, и где у Keycloak лежат roles?

## Как проходит приёмка

Принимающий выполняет `curl` на ваши маршруты без token, с чужим и с испорченным token, смотрит с
вами в log и может попросить изменить одну строку — убрать регистрацию `OrderOwnerHandler` или
`RequireAuthorization()` с группы — и объяснить, что сломалось.
