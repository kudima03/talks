# Лабораторная 8. Первый сервис: тот же магазин на ASP.NET Core

**К лекции:** [Л8. ASP.NET Core: хост и конвейер запроса](../L08-host-and-pipeline.md)
**Условие:** всё из терминала, запросы — `curl`. Нужны `Shop.Domain` и `Shop.RawServer` из седьмой
лабораторной. Проект `Shop.Api`, который вы создадите, живёт до конца курса.
**Сдаётся вживую:** отчёта нет. Вы показываете сервис и отвечаете на вопросы по любому шагу.

Вопрос **«усложнённый»** — только для тех, у кого основной ответ уже есть.

## 1. Тот же сервис на ASP.NET Core

```sh
dotnet new web --name Shop.Api
cd Shop.Api
dotnet add reference ../Shop.Domain
```

1. Скопируйте `OrderStore` из `Shop.RawServer` в `Shop.Api` (сервер на сокетах остаётся как был).
2. Соберите `Program.cs`: `AddHealthChecks()` и `AddSingleton<OrderStore>()` до `Build`,
   `MapHealthChecks("/health")` и три `MapGet` с теми же текстами, что в седьмой лабораторной,
   после. `OrderStore` handler получает параметром: `(int number, OrderStore store) => ...` на
   маршруте `/orders/{number:int}`.
3. Запустите оба сервера и сравните `curl -i` на `/nope` и `curl -i -X POST` на `/orders` у каждого.

**Вопрос:** в седьмой лабораторной `404` и `405` с `Allow` писали вы. Кто отвечает теперь, и что
ещё добавил Kestrel в заголовки?

**Вопрос (усложнённый):** сервер на сокетах обслуживал одно соединение за раз и принимал тело
любого размера. Что из этого Kestrel делает иначе — найдите в лекции.

## 2. Ошибки в одном месте

1. В `Shop.Domain` добавьте исключение домена:

   ```csharp
   public sealed class NotFoundException(string message) : DomainException(message);
   ```

2. В handler `/orders/{number:int}` вместо ответа `404` бросайте его:

   ```csharp
   Order order = store.Find(number) ?? throw new NotFoundException($"no order {number}");
   ```

3. Напишите `ErrorHandlingMiddleware` — класс с конструктором `(RequestDelegate next,
   ILogger<ErrorHandlingMiddleware> logger)` и методом `InvokeAsync(HttpContext context)`, по
   листингу лекции: `try` вокруг `await next(context)`, `catch (Exception e) when
   (!context.Response.HasStarted)`, switch по типу исключения — `NotFoundException` → `404` и его
   сообщение, всё остальное → `500` и `Internal error`; `logger.LogError(e, ...)` с исключением
   целиком; ответ — JSON `{ error, traceId }`, где `traceId` — `context.TraceIdentifier`.
4. Подключите его **первым** после `Build`: `app.UseMiddleware<ErrorHandlingMiddleware>();`.
   Добавьте тестовый endpoint:
   `app.MapGet("/fail", () => { throw new InvalidOperationException("Test failure"); });`.
5. Выполните `curl -i` на `/orders/7` и `/fail` и посмотрите в log.

**Вопрос:** почему в ответе на `/fail` нет текста исключения, а в ответе на `/orders/7` есть? Где
искать подробности про `500`?

## 3. Background service

Напишите `OrderStatsService` по листингу лекции и зарегистрируйте его
`builder.Services.AddHostedService<OrderStatsService>();`:

```csharp
public sealed class OrderStatsService(OrderStore store, ILogger<OrderStatsService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                // count orders by status with LINQ and log one line
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                logger.LogError(e, "Order stats iteration failed");
            }
        }
    }
}
```

Тело `try` — ваше: посчитайте заказы по статусам запросом LINQ из пятой лабораторной и запишите
одной строкой, например `Orders by status: Created=1, Paid=1`. Запустите, дождитесь двух строк и
нажмите Ctrl+C.

**Вопрос:** почему `try` внутри цикла, а не вокруг? Что будет с сервером, если исключение вылетит
из `ExecuteAsync`?

Сборка без предупреждений, `git init`, `dotnet new gitignore`, коммит.

## Дополнительное задание (по желанию)

**Строка на каждый запрос.** Напишите `RequestLoggingMiddleware`: засечь время
(`Stopwatch.GetTimestamp()`), `await next(context)`, записать метод, путь, статус и длительность
шаблоном с параметрами. Подключите вторым, после обработки ошибок, и выполните `curl` на
`/orders/1`, `/orders/7`, `/fail` — сколько строк в log? Не меняя порядок, оберните `await
next(context)` в `try` с exception filter, который пишет строку и возвращает `false`:
`catch (Exception e) when (LogFailed(context, started, e)) { }`.

**Отмена.** Ветка в `ErrorHandlingMiddleware` выше общей: `context.RequestAborted` отменён и
прилетело `OperationCanceledException` — не отвечать, в log одна строка `Information`. Проверьте
`curl --max-time 1` на endpoint с `await Task.Delay(TimeSpan.FromSeconds(10), context.RequestAborted)`.

**Вопрос:** почему без filter у упавших запросов нет строки? Почему клиент, закрывший вкладку, —
не `Error`?

## Как проходит приёмка

Принимающий выполняет `curl` на ваши маршруты, смотрит с вами в log, просит нажать Ctrl+C и может
попросить изменить одну строку — например, убрать вызов `next` — и объяснить, что сломалось.
