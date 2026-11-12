# Лабораторная 12. Cache, logs и traces: сервис под Aspire

**К лекции:** [Л12. Кэширование, логирование и наблюдаемость](../L12-caching-and-observability.md)
**Условие:** всё из терминала, запросы — `curl`; dashboard открывается в браузере. Нужны `Shop.Domain`
и `Shop.Api` из одиннадцатой лабораторной: `IOrderStore`, `AddOrders()`, группа `/orders` под
`RequireAuthorization()`, `OrderOwnerHandler` и три token из `dotnet user-jwts` — владельца заказа 1,
другого пользователя и admin. Нужен запущенный Docker. Порт в примерах — 5090.
**Сдаётся вживую:** отчёта нет. Вы показываете сервис и dashboard и отвечаете на вопросы по любому шагу.

Вопрос **«усложнённый»** — только для тех, у кого основной ответ уже есть.

## 1. AppHost и Redis

```sh
# в папке, где лежат Shop.Domain и Shop.Api
dotnet new install Aspire.ProjectTemplates
dotnet new aspire-apphost --name Shop.AppHost
dotnet new aspire-servicedefaults --name Shop.ServiceDefaults
dotnet add Shop.AppHost reference Shop.Api
dotnet add Shop.AppHost package Aspire.Hosting.Redis
dotnet add Shop.Api reference Shop.ServiceDefaults
```

1. `Shop.AppHost/AppHost.cs` целиком:

   ```csharp
   var builder = DistributedApplication.CreateBuilder(args);

   var cache = builder.AddRedis("cache");

   builder.AddProject<Projects.Shop_Api>("api")
       .WithReference(cache)
       .WaitFor(cache);

   builder.Build().Run();
   ```

2. В `Program.cs` сервиса, сразу после `CreateBuilder`: `builder.AddServiceDefaults();`.
   `MapDefaultEndpoints()` не вызывайте — `/health` у вас уже есть.
3. `dotnet run --project Shop.AppHost`. Откройте `Login URL` из вывода, на странице Resources
   дождитесь `Running` у `cache` и `api` и выполните `curl -i http://localhost:5090/health`.
   С этого шага и до конца курса сервис запускается только так.

**Вопрос:** в `Shop.Api` нет ни адреса Redis, ни адреса dashboard. Откуда сервис их узнал? Ответ
виден в dashboard, в деталях ресурса `api`.

## 2. HybridCache на заказе

```sh
dotnet add Shop.Api package Aspire.StackExchange.Redis.DistributedCaching
dotnet add Shop.Api package Microsoft.Extensions.Caching.Hybrid --version "10.*"
```

1. До `Build` — integration для ресурса `cache` и сам cache:

   ```csharp
   builder.AddRedisDistributedCache("cache");
   builder.Services.AddHybridCache();
   ```

2. Handler `GET /orders/{number:int}` получает параметром `HybridCache cache`. Всё, что он делал
   в одиннадцатой лабораторной, уходит в factory; `email` — claim из token, как там же:

   ```csharp
   OrderDto dto = await cache.GetOrCreateAsync(
       $"shop:order:v1:{number}:user:{email}",
       async token =>
       {
           // как было: FindAsync, проверка владельца, NotFoundException в обоих отказах
           return order.ToDto(number);
       },
       new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(1) },
       tags: ["orders"],
       cancellationToken: ct);
   ```

3. В handler `POST /orders` после `AddAsync`: `await cache.RemoveByTagAsync("orders", ct);`.
   Перезапустите AppHost и выполните `curl` на `/orders/1` с token владельца, admin и другого
   пользователя: `200`, `200`, `404`, как в одиннадцатой.
4. **Сломайте нарочно:** уберите из ключа `:user:{email}`, перезапустите и запросите `/orders/1`
   сначала с token admin, потом с token другого пользователя. Верните ключ.

**Вопрос:** `OrderOwnerHandler` никто не трогал. Почему чужой заказ вернулся с `200`, и какое
правило лекции про ключ это нарушает?

## 3. Structured logs и traces

1. Handler получает ещё `ILogger<Program> logger` и открывает scope на всё своё тело:

   ```csharp
   using (logger.BeginScope(new Dictionary<string, object> { ["OrderNumber"] = number }))
   {
       long started = Stopwatch.GetTimestamp();
       // GetOrCreateAsync из шага 2
       logger.OrderServed(Stopwatch.GetElapsedTime(started).TotalMilliseconds);
       return TypedResults.Ok(dto);
   }
   ```

2. Сообщение объявлено один раз, в конце `Program.cs`:

   ```csharp
   static partial class Log
   {
       [LoggerMessage(Level = LogLevel.Information, Message = "Order served in {ElapsedMs} ms")]
       public static partial void OrderServed(this ILogger logger, double elapsedMs);
   }
   ```

3. Перезапустите AppHost, дважды выполните `curl` на `/orders/1` с token владельца и один раз —
   с token другого пользователя. В dashboard откройте Structured logs, найдите обе записи
   `Order served` и из каждой перейдите в её trace.

**Вопрос:** в шаблоне сообщения номера заказа нет — откуда в записи поле `OrderNumber`? Чем trace
второго запроса отличается от первого, и что об этом говорит `ElapsedMs`?

**Вопрос (усложнённый):** в ответе `404` есть `traceId`. Найдите по нему запись об ошибке в
Structured logs. В каком поле записи он лежит, и почему это не поле `TraceId`?

Сборка без предупреждений, коммит.

## Дополнительное задание (по желанию)

**Свой span.** Статический `ActivitySource` с именем `Shop.Api` — `Shop.ServiceDefaults` уже слушает
source с именем приложения — и `using var activity = source.StartActivity("order.get");` с tag
`order.number` вокруг `GetOrCreateAsync`. Найдите span в traces первого и второго запроса.
**Вопрос:** почему `activity?.SetTag(...)`, а не `activity.SetTag(...)`?

## Как проходит приёмка

Принимающий выполняет `curl` на `/orders/1` с разными token, смотрит с вами в Resources, Structured
logs и Traces и может попросить изменить одну строку — ключ, срок, tag или scope — и показать в
dashboard, что изменилось.
