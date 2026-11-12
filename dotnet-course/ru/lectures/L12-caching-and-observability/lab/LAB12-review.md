# Лабораторная 12 — заметки принимающему

Служебный файл; студенту выдаётся [LAB12-task.md](LAB12-task.md). Рекомендации, а не регламент.

**Приёмка живая, минут на десять.** Resources в dashboard, `curl` на `/orders/1` с тремя token,
Structured logs и Traces, затем одна правка вживую (см. «Правки вживую»). На что смотреть:

1. **Шаг 1.** В `AppHost.cs` ресурс назван `cache`, сервис ссылается на него через `WithReference`.
   В `Shop.Api` нет ни строки подключения, ни адреса dashboard — ни в коде, ни в `appsettings`.
   `AddServiceDefaults()` стоит первым; `MapDefaultEndpoints()` не вызван.
2. **Шаг 2.** В ключе есть пользователь; проверка владельца осталась и стоит внутри factory, а не
   после cache; срок — минуты; tag один и тот же в `GetOrCreateAsync` и в `RemoveByTagAsync`.
   В cache лежит `OrderDto`, а не `Order`.
3. **Шаг 3.** Сообщение — метод с `LoggerMessage`, без интерполяции; номер заказа приходит из
   scope, а не из шаблона; в log нет ни почты, ни token.

## Эталон

Всё ниже — с эталонного решения на dotnet SDK 10.0.114, Aspire 13.6.4 (AppHost, `Aspire.Hosting.Redis`,
`Aspire.StackExchange.Redis.DistributedCaching`), `Microsoft.Extensions.Caching.Hybrid` 10.11.0,
Redis 8.6 в Docker, окружение Development, порт 5090. Записи и spans прочитаны из dashboard.

**Шаг 1.** В деталях ресурса `api` — переменные окружения, которые передал AppHost (порты
случайные, пароль скрыт):

```text
ConnectionStrings__cache=localhost:40557,password=***,ssl=true
OTEL_EXPORTER_OTLP_ENDPOINT=https://localhost:21007
OTEL_SERVICE_NAME=api
```

**Шаг 2.** Владелец и admin — `200`, другой пользователь — `404` и
`{"error":"no order 1","traceId":"0HNQ0GDRSCNPM:00000001"}`. В Redis после двух успешных запросов —
по ключу на пользователя: `shop:order:v1:1:user:ann@example.com` и
`shop:order:v1:1:user:admin@example.com`; после `POST /orders` появляется `__MSFT_HCT__orders` —
отметка сброса tag. С ключом без `:user:{email}`: admin — `200`, затем другой пользователь —
`200` с чужим заказом, и `OrderOwnerHandler` при этом не вызывался.

**Шаг 3.** Две записи категории `Program` и запись об ошибке:

```text
Order served in 37.2954 ms    ElapsedMs=37.2954 RequestId=0HNQ0GDRSCNPK:00000001 RequestPath=/orders/1 OrderNumber=1
Order served in 0.1042 ms     ElapsedMs=0.1042 RequestId=0HNQ0GDRSCNPL:00000001 RequestPath=/orders/1 OrderNumber=1
Request /orders/1 failed with 404    Path=/orders/1 Status=404 RequestId=0HNQ0GDRSCNPM:00000001 RequestPath=/orders/1
```

Trace первого запроса — `GET /orders/{number:int}`, 108 мс, внутри spans Redis `HMGET` и `HGET`
(и три `CLIENT` на первом соединении). Trace второго — 3,7 мс и один span: ответ взят из памяти.
После `POST /orders` следующий запрос снова идёт в factory (`Order served in 4.7344 ms`).

**Дополнительное задание.** Span `order.get` — дочерний у span запроса: 39 мс в первом trace,
0,2 мс во втором.

## Правки вживую

| Правка | Что будет |
|---|---|
| Убрать `:user:{email}` из ключа | Чужой заказ с `200`, если владелец или admin запросил первым: factory с проверкой не вызвана |
| Убрать `builder.AddRedisDistributedCache("cache")` | Сборка чистая, ответы те же; cache только в памяти процесса: в traces нет spans Redis, в Redis нет ключей |
| Добавить `app.MapDefaultEndpoints()` | `/health` — `500`, `AmbiguousMatchException: The request matched multiple endpoints. Matches: Health checks, Health checks` |
| Убрать `BeginScope` | Запись `Order served` остаётся, поля `OrderNumber` в ней нет |
| Убрать `RemoveByTagAsync` | После `POST` повторный `GET` по-прежнему из cache: `ElapsedMs` — доли миллисекунды |

Первые три правки запускались на эталоне; последние две — по смыслу, без запуска.

Ещё вопросы на месте: поставить `Expiration` в пять секунд и показать третий запрос; найти в
Resources консольный вывод `api`; спросить, что будет со вторым экземпляром сервиса без Redis.

## Вопросы и короткие ответы

- **Откуда сервис узнал адреса?** *AppHost передал их переменными окружения:
  `ConnectionStrings__cache` — из `WithReference(cache)`, `OTEL_EXPORTER_OTLP_ENDPOINT` — адрес
  dashboard. Integration читает connection string по имени `cache`, ServiceDefaults включает
  экспорт, когда видит адрес. В проде те же имена заполнит оркестратор.*
- **Почему чужой заказ с `200`?** *Ключ определяет, что считается «тем же самым». Без пользователя
  ответ admin лёг под общий ключ, и следующий запрос получил его, не дойдя до проверки. Правило:
  ответ зависит от того, кто спрашивает, — он в ключе; cache стоит после authorization.*
- **Откуда `OrderNumber`?** *Из scope: поле добавляется ко всем записям внутри `using`, шаблон о
  нём не знает. Во втором trace нет spans Redis и нет factory — ответ из памяти процесса;
  `ElapsedMs` падает с десятков миллисекунд до долей.*
- **`traceId` из ответа (усложнённый)?** *Это `context.TraceIdentifier` из Л8 — идентификатор
  запроса Kestrel. В записи он лежит в поле `RequestId`, которое добавил scope фреймворка. Поле
  `TraceId` — идентификатор trace из W3C `traceparent`, другое значение.*
- **`activity?.` (дополнительное)?** *`StartActivity` возвращает `null`, когда source никто не
  слушает или trace не попал в выборку.*

**Частые ошибки:** ключ без пользователя; проверка владельца вынесена из factory и забыта;
интерполяция в сообщении logger; почта или token в записи; `Order` вместо `OrderDto` в cache;
`MapDefaultEndpoints()` поверх своего `/health`; разные имена ресурса в AppHost и в
`AddRedisDistributedCache`.

## Что знать заранее

- **Версия HybridCache.** `dotnet add package Microsoft.Extensions.Caching.Hybrid` без версии ставит
  11.0.0; в проекте на net10.0 вместе с `LoggerMessage` это `error CS0757: A partial method may not
  have multiple implementing declarations`. В задании версия `10.*`.
- **Записи внутри factory не получают ни scope, ни `TraceId`.** HybridCache запускает factory вне
  контекста вызывающего запроса: предупреждение из `OrderOwnerHandler` приходит без `OrderNumber`
  и без trace. Поэтому `OrderServed` стоит после `GetOrCreateAsync`, а не внутри.
- **Запись в Redis идёт в фоне.** Spans `HMSET` и `EXPIRE` могут оказаться в чужом trace — у нас
  они попали в trace первого запроса `/health`. В trace запроса видны чтения: `HMGET`, `HGET`.
- **Redis под Aspire — с паролем и TLS.** Посмотреть ключи:
  `docker exec <cache-...> sh -c 'redis-cli -a "$REDIS_PASSWORD" --tls --insecure keys "*"'`.
- **Не проверено.** Шаблон 13.6.5 создаёт AppHost с `AspireUseCliBundle=true`: первый
  `dotnet run` сам ставит Aspire CLI bundle. На эталонной машине эта установка не выполнялась;
  эталон собран на 13.6.4 с `AspireUseCliBundle=false`. Если установка не прошла, это свойство в
  `Shop.AppHost.csproj` возвращает прежнюю схему с пакетами. Профиль `http` у AppHost требует
  `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true`; по умолчанию берётся `https`, и браузер спросит про
  сертификат разработки.
- Token читает claim почты как в одиннадцатой лабораторной; audience у `user-jwts` привязана к
  адресам из `launchSettings.json` сервиса, AppHost их не меняет.
