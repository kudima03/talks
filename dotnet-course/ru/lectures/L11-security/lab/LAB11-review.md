# Лабораторная 11 — заметки принимающему

Служебный файл; студенту выдаётся [LAB11-task.md](LAB11-task.md). Рекомендации, а не регламент.

**Приёмка живая, минут на десять.** `curl -i` на `/orders/1` без token, с token владельца, с чужим и
с испорченным; на `/orders` с обычным token и с `admin`; затем одна правка вживую (см. «Правки
вживую»). На что смотреть:

1. **Шаг 1.** `401` и `403` нигде не написаны руками: handlers о статусах не знают. `/health`
   открыт, вся группа `/orders` закрыта одной строкой, а не `RequireAuthorization()` на каждом
   маршруте. Ключа подписи нет ни в одном файле репозитория.
2. **Шаг 2.** Policy объявлена один раз, имя — константой; на маршруте имя policy, а не
   `RequireRole` или `if (user.IsInRole(...))` в handler.
3. **Шаг 3.** Сравнение почты живёт только в `OrderOwnerHandler`; оба маршрута с загруженным заказом
   вызывают его через `IAuthorizationService`. Чужой и несуществующий заказ отвечают одинаково. Почта
   нового заказа — из token.

## Эталон

Всё ниже — с эталонного решения на dotnet SDK 10.0.114, `Microsoft.AspNetCore.Authentication.JwtBearer`
10.0.15 и FluentValidation 12.2.0, окружение Development, порт 5090; заказы Ann Smith (Paid, 300.00) и
Bob Brown (Created, 120.00). Заголовки `Date`/`Server`/`Transfer-Encoding` сокращены до `...`.
`dotnet add package` без версии действительно падает: `error: NU1202: Package
Microsoft.AspNetCore.Authentication.JwtBearer 11.0.0 is not compatible with net10.0`.

**Шаг 1.**

```text
$ curl -s -i http://localhost:5090/orders/1
HTTP/1.1 401 Unauthorized
Content-Length: 0
...
WWW-Authenticate: Bearer
$ curl -s -i -H "Authorization: Bearer $ANN" http://localhost:5090/orders/1
HTTP/1.1 200 OK
...
{"number":1,"customerEmail":"ann@example.com","status":"Paid","total":300.00}
$ curl -s -i -H "Authorization: Bearer ${ANN}x" http://localhost:5090/orders/1
HTTP/1.1 401 Unauthorized
Content-Length: 0
...
WWW-Authenticate: Bearer error="invalid_token", error_description="The signature key was not found"
```

`/health` без token — `200` и `Healthy`. В payload token: `unique_name` и `sub` — `ann`, `email`,
`aud` со всеми адресами из `launchSettings.json`, `iss` — `dotnet-user-jwts`, срок — три месяца;
header — `{"alg":"HS256","typ":"JWT"}`. `user-jwts` дописал `UserSecretsId` в файл проекта и секцию
`Authentication:Schemes:Bearer` (`ValidAudiences`, `ValidIssuer`) в `appsettings.Development.json`;
ключ — в `dotnet user-secrets list`, строка `Authentication:Schemes:Bearer:SigningKeys:0:Value`.

**Шаг 2.**

```text
$ curl -s -i -H "Authorization: Bearer $ANN" http://localhost:5090/orders
HTTP/1.1 403 Forbidden
Content-Length: 0
...
$ curl -s -i -H "Authorization: Bearer $ROOT" http://localhost:5090/orders
HTTP/1.1 200 OK
...
[{"number":1,"customerEmail":"ann@example.com","status":"Paid","total":300.00},{"number":2,"customerEmail":"bob@example.com","status":"Created","total":120.00}]
```

**Шаг 3.**

```text
$ curl -s -i -H "Authorization: Bearer $BOB" http://localhost:5090/orders/1
HTTP/1.1 404 Not Found
...
{"error":"no order 1","traceId":"0HNQ0GGFLD177:00000001"}
$ curl -s -i -H "Authorization: Bearer $ROOT" http://localhost:5090/orders/1
HTTP/1.1 200 OK
...
{"number":1,"customerEmail":"ann@example.com","status":"Paid","total":300.00}
$ curl -s -i -H "Authorization: Bearer $BOB" --json @new-order.json http://localhost:5090/orders
HTTP/1.1 201 Created
...
Location: /orders/3
...
{"number":3,"customerEmail":"bob@example.com","status":"Created","total":270.00}
```

В `new-order.json` почта `cat@example.com`; в заказе — `bob@example.com`. Ann на `/orders/3` получает
тот же `404`, что Bob на `/orders/7`, которого нет. В log сервиса заголовка `Authorization` нет.
Маршрут `/{number:int}/total` и `RatesClient` в эталон не входили — проверка там та же, не запускалась.

## Правки вживую

| Правка | Что будет |
|---|---|
| Убрать регистрацию `OrderOwnerHandler` из `AddOrders()` | Сборка чистая; **владелец** получает `404` на свой заказ: handler не найден, requirement никто не выполнил, а молчание — отказ |
| Убрать `RequireAuthorization()` с группы | `/orders/1` без token — `404` вместо `401` (проверка на ресурсе отказала анониму); `/orders` — по-прежнему `401` от policy `Admin`; `POST` — `403` из handler. Две проверки на двух уровнях, каждая ловит своё |
| Запустить с `--environment Production` | Любой правильный token — `401`, `The signature key was not found`: секреты пользователя и `appsettings.Development.json` читаются только в Development |
| Выдать token с `--audience reports-api` | `user-jwts` перезаписал `ValidAudiences`: новый token проходит, **старые** — `401`, `The audience '(null)' is invalid`. Починка — выдать token без `--audience` |

Без запуска, вопросами: что изменит `SetFallbackPolicy` с `RequireAuthenticatedUser` для `/health`;
почему token с `--valid-for 1s` ещё несколько минут принимается (допуск на часы); что увидит тот,
кто залогирует заголовки запроса.

## Вопросы и короткие ответы

- **Кто отправил `401`?** *Scheme: группа потребовала пользователя, его нет — challenge. Без token в
  `WWW-Authenticate` только `Bearer`; с испорченным — ещё `error="invalid_token"` и причина.*
- **Где ключ (усложнённый)?** *В секретах пользователя, в домашней папке, вне репозитория. В
  репозитории — только идентификатор секретов и имена issuer и audience: это не секреты.*
- **Почему `403`, а не `401`?** *Кто пришёл, известно: token принят. Правило говорит «нельзя» —
  forbid. `401` заставил бы клиента входить заново, и это ничего бы не изменило.*
- **Почему проверка после загрузки?** *На маршруте виден пользователь и адрес, но не заказ: его ещё
  нет. Владение — свойство ресурса. Почта в заказе — из token: телу клиент хозяин, token — нет.*

## Дополнительное задание

Не запускалось. Ожидаемое: `Authority` — адрес realm (`http://localhost:8080/realms/<realm>`),
audience — то, что Keycloak кладёт в `aud` (по умолчанию `account`, пока не добавлен audience
mapper); для `http` понадобится `RequireHttpsMetadata: false`. Roles лежат во вложенном
`realm_access.roles` — policy `Admin` без claims transformation их не увидит; это и есть ответ.

**Частые ошибки:** сравнение почты прямо в handler маршрута, да ещё только в одном из двух;
`401` или `403` руками через `Results.StatusCode`; чужой заказ — `403`, несуществующий — `404`
(перебором видно, какие номера заняты); `FindFirstValue("email")` — пусто, имя claim после
переименования — `ClaimTypes.Email`; почта нового заказа из тела; token или ключ подписи в
репозитории «чтобы принимающему было удобно».
