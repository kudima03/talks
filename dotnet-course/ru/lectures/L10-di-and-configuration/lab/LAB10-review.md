# Лабораторная 10 — заметки принимающему

Служебный файл; студенту выдаётся [LAB10-task.md](LAB10-task.md). Рекомендации, а не регламент.

**Приёмка живая, минут на десять.** `curl` на `/orders`, `/orders/1/total` с валютой и без, затем
одна правка вживую (см. «Правки вживую»). На что смотреть:

1. **Шаг 1.** Handlers и `OrderStatsService` принимают `IOrderStore`, а не `InMemoryOrderStore`;
   в `Program.cs` нет ни `new` для store, ни строк с `Add...` про заказы — только `AddOrders()`.
   Store — singleton, и студент может сказать почему: состояние общее и защищено `lock`.
2. **Шаг 2.** `ShopOptions` — только свойства и атрибуты; имя секции — константа в классе; в
   цепочке есть и `ValidateDataAnnotations()`, и `ValidateOnStart()`.
3. **Шаг 3.** Нигде нет `new HttpClient` и `builder.Configuration["Shop:RatesUrl"]`; handler
   принимает `RatesClient`; адрес берётся из `IOptions<ShopOptions>` внутри регистрации;
   `ct` доходит до `GetFromJsonAsync`.

## Эталон

Всё ниже — с эталонного решения: сборка на dotnet SDK 10.0.114, запуск `dotnet run` в контейнере
`mcr.microsoft.com/dotnet/sdk:10.0` (SDK 10.0.403), FluentValidation 12.2.0,
Microsoft.Extensions.Http.Resilience 11.0.0, окружение Development, порт 5090, curl 8.5.0; заказы
Ann Smith (Paid, 300.00) и Bob Brown (Created, 120.00). Заголовки ответов и стек исключений
сокращены до `...`.

**Шаг 1, поломка.** `AddScoped<IOrderStore, InMemoryOrderStore>()`, `dotnet run`:

```text
Unhandled exception. System.AggregateException: Some services are not able to be constructed (Error while validating the service descriptor 'ServiceType: Microsoft.Extensions.Hosting.IHostedService Lifetime: Singleton ImplementationType: OrderStatsService': Cannot consume scoped service 'IOrderStore' from singleton 'Microsoft.Extensions.Hosting.IHostedService'.)
```

**Шаг 3.**

```text
$ curl -s http://localhost:5090/orders/1/total
{"number":1,"total":300.00,"currency":"BYN"}
$ curl -s 'http://localhost:5090/orders/1/total?currency=USD'
{"number":1,"total":93.00,"currency":"USD"}
```

В log на второй запрос — строки фабрики и набора устойчивости:

```text
info: System.Net.Http.HttpClient.RatesClient.LogicalHandler[100]
      Start processing HTTP request GET http://localhost:5090/stub/rates/USD
...
info: Polly[3]
      Execution attempt. Source: 'RatesClient-standard//Standard-Retry', Operation Key: '', Result: '200', Handled: 'False', Attempt: '0', Execution Time: 48.8056ms
info: System.Net.Http.HttpClient.RatesClient.LogicalHandler[101]
      End processing HTTP request after 67.5806ms - 200
```

После `Shop__Currency=EUR dotnet run`:

```text
$ curl -s http://localhost:5090/orders/1/total
{"number":1,"total":300.00,"currency":"EUR"}
```

Усложнённый вопрос, `?currency=XXX`:

```text
HTTP/1.1 500 Internal Server Error
...
{"error":"Internal error","traceId":"0HNQ0G89HI60H:00000001"}
```

```text
fail: ErrorHandlingMiddleware[0]
      Request /orders/1/total failed with 500
      System.Net.Http.HttpRequestException: Response status code does not indicate success: 404 (Not Found).
```

**Дополнительное задание.** С `IOptionsMonitor` и `BYN` → `EUR` в файле без перезапуска второй
`curl` отдаёт `"currency":"EUR"` (проверено в контейнере, где изменения файлов ловятся опросом,
через несколько секунд).

## Правки вживую

| Правка | Что будет |
|---|---|
| `Shop__RatesUrl=abc dotnet run` | Сервис не стартует: `Hosting failed to start`, `OptionsValidationException: DataAnnotation validation failed for 'ShopOptions' members: 'RatesUrl' with the error: 'The RatesUrl field is not a valid fully-qualified http, https, or ftp URL.'.` С пустым `Shop__RatesUrl=` — `'The RatesUrl field is required.'` |
| Убрать `ValidateOnStart()`, затем `Shop__RatesUrl=abc dotnet run` | Сервис стартует, `/health` — `200 Healthy`; `/orders/1/total` — `500 Internal error`, в log то же `OptionsValidationException`, но уже на запросе |
| `Shop__RatesUrl=http://localhost:5999/ dotnet run`, `curl` с `?currency=USD` | Ответ `500` через семь секунд (`real 0m7.304s`); в log четыре попытки `Attempt: '0'`…`'3'` с `Connection refused` и три `OnRetry`. Без `?currency` — мгновенные `200`: клиент не вызывался |
| `AddScoped` вместо `AddSingleton` для store | См. шаг 1: сервис не стартует в Development |

## Вопросы и короткие ответы

- **Кто держит scoped?** *`OrderStatsService`: hosted service — singleton, а store стал scoped.
  Пропусти это контейнер — у фонового сервиса был бы свой store, у каждого запроса свой, и `POST`
  терял бы заказы. Проверка работает только в Development; в Production тот же код стартует
  молча.*
- **Когда узнаете об опечатке?** *С `ValidateOnStart()` — при запуске, процесс не поднимется. Без
  него — на первом запросе, которому нужны настройки; `/health` всё это время отвечает `200`, и
  оркестратор считает сервис живым.*
- **Откуда `EUR`?** *Переменные окружения — слой выше `appsettings.json`: позднее перекрывает
  раннее. Ключ в приложении — `Shop:Currency`; двоеточие в имени переменной писать нельзя, его
  заменяют два подчёркивания.*
- **`XXX` (усложнённый)?** *Заглушка ответила `404`, `GetFromJsonAsync` бросил
  `HttpRequestException`, middleware превратил его в `500`. Набор устойчивости `404` не
  повторяет. По лекции ошибки чужого API превращает в свои исключения typed client: например,
  `NotFoundException("no rate for XXX")`, и тогда клиент получит `404` с понятным текстом.*
- **Почему не `Shop__Currency` (доп. задание)?** *Перечитываются файлы; переменные окружения
  процесс читает один раз при старте.*

**Частые ошибки:** `new InMemoryOrderStore()` или `new HttpClient()` в handler;
`builder.Configuration["Shop:RatesUrl"]` в регистрации клиента или в handler; `IServiceProvider`
в параметрах handler вместо конкретных зависимостей; `RatesUrl` без завершающей `/` — относительный
путь `rates/USD` заменит последний сегмент адреса; `IOptionsSnapshot` в singleton; порт в
`RatesUrl` не совпадает с портом сервиса.
