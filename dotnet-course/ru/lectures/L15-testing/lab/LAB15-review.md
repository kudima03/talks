# Лабораторная 15 — заметки принимающему

Служебный файл; студенту выдаётся [LAB15-task.md](LAB15-task.md). Рекомендации, а не регламент.

**Приёмка живая, минут на десять.** `dotnet test` целиком, затем один unit test и один contract test
вслух по трём абзацам, затем одна правка вживую (см. «Правки вживую»). На что смотреть:

1. **Шаг 1.** Имена отвечают на «что сломалось» без чтения тела; один assert; в `[Theory]` нет `if` по
   параметру; ожидаемое значение — константа, а не расчёт. Validator создан через `new`, без host.
2. **Шаг 2.** Первым написан test на `/health`. В tests нет `ShopDbContext`, `IOrderStore` и
   подстановки хранилища: только запрос, статус, тело. Каждый test создаёт свой заказ сам и не
   рассчитывает на заказ другого test; номер заказа берётся из `Location`, а не пишется константой.
3. **Шаг 3.** Три свойства лежат в `Directory.Build.props`, а не в каждом `csproj`. Ошибки
   исправлены в коде; у каждого подавления в `.editorconfig` есть комментарий с причиной.

## Эталон

Всё ниже — с эталонного решения на dotnet SDK 10.0.114, xUnit 2.9.3, `Microsoft.AspNetCore.Mvc.Testing`
10.0.14, Testcontainers 4.16.0, Docker 29.9; образы `postgres:17-alpine` и `redis:7-alpine`. Эталон —
сокращённое состояние LAB14 (без AppHost и `RatesClient`): 11 tests задания и один лишний на `403`.

```text
$ dotnet test
Passed!  - Failed:     0, Passed:    12, Skipped:     0, Total:    12, Duration: 6 s - Shop.Tests.dll (net10.0)
$ dotnet test --filter "FullyQualifiedName~OrderTests"
Passed!  - Failed:     0, Passed:     3, Skipped:     0, Total:     3, Duration: 31 ms - Shop.Tests.dll (net10.0)
$ dotnet test --filter "FullyQualifiedName~OrderTests" --logger "console;verbosity=normal"
  Passed Shop.Tests.OrderTests.MoveTo_PaidFromCreated_ChangesStatusToPaid [74 ms]
  Passed Shop.Tests.OrderTests.MoveTo_FromCreated_Throws(next: Shipped) [7 ms]
  Passed Shop.Tests.OrderTests.MoveTo_FromCreated_Throws(next: Delivered) [< 1 ms]
```

Первый прогон с загрузкой образов — 47 секунд, следующие — 6: containers стартуют один раз на класс.
`Program` виден из `Shop.Tests` без строки `public partial class Program;` — в .NET 10 она не нужна
(эталон собран без неё; с ней тоже собирается).

**Шаг 3.** С `latest-all` эталон даёт 69 предупреждений по 12 правилам (`CA2007`, `CA1062`, `CA1515`,
`CA1707`, …). С `latest-recommended` — единицы: в эталоне `CA1725` (имя параметра в
`IEntityTypeConfiguration`), `CA1512` (`ThrowIfNegative`), `CA1848` (`LoggerMessage`, знаком по
двенадцатой лабораторной) и `CA1707` в tests. Поломка из пункта 4:

```text
Shop.Api/Program.cs(48,5): error CA1849: 'StreamReader.ReadLine()' synchronously blocks. Await 'StreamReader.ReadLineAsync()' instead. (https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1849)
```

Без строки в `.editorconfig` это правило молчит: в `latest-recommended` оно выключено.

**Дополнительное задание.** Числа у каждого свои; смотреть на колонки `Error`, `Ratio`, `Allocated`.
Без `-c Release` инструмент не меряет: `Assembly Benchmarks which defines benchmarks is non-optimized.
Benchmark was built without optimization enabled (most probably a DEBUG configuration). Please, build
it in RELEASE.`

## Правки вживую

| Правка | Что будет |
|---|---|
| Убрать `MigrateAsync` из `ApiFactory` | Красный один: `POST` → `Expected: Created`, `Actual: InternalServerError`. `422`, `401` и `/health` зелёные — до базы не доходят. **Test на `404` тоже зелёный, но не по делу:** `POST` упал, `Location` пуст, и `404` пришёл не от проверки владельца |
| Поменять местами аргументы `Assert.Equal` | Ничего не покраснеет. Разница видна только при падении: сообщение `Expected`/`Actual` говорит наоборот |
| Вернуть `ReadLine` в `async` handler | Сборка красная, `CA1849`; до tests дело не доходит |
| `new PostgreSqlBuilder()` без образа | `CS0618` (конструктор устарел) — с `TreatWarningsAsErrors` это ошибка сборки |
| Убрать `[Shop.Tests/**.cs]` из `.editorconfig` | `CA1707: Remove the underscores from member name …` на каждом test |

## Вопросы и короткие ответы

- **Сколько строк дал `[Theory]`?** *Две, по одной на `[InlineData]`, с входом в имени. `Paid` из
  `Created` — другое поведение и другое утверждение; `if` по параметру в `[Theory]` — два tests,
  склеенные в один.*
- **Что настоящее, что подменено?** *Настоящие pipeline, routing, binding, validation, error handling,
  сериализация, DI, EF Core, migrations и PostgreSQL. Подменена только аутентификация: test scheme
  вместо JWT. Test на `422` прошёл через filter, validator, `ErrorHandlingMiddleware` и имена полей JSON.*
- **Какое правило и почему `CA1707` только в tests?** *`CA1849`: синхронный вызов в `async` методе,
  предлагает `ReadLineAsync`. Имя `Метод_Условие_Ожидание` — соглашение tests; в остальном коде
  подчёркивания в именах — ошибка, и глобальное отключение её бы спрятало.*

**Спросить устно, если есть время:** tests пишут в одну базу и не чистят её — почему не мешают друг
другу (каждый создаёт свой заказ и читает его по `Location`; cache key включает пользователя) и какой
test начал бы мешать (`GET /orders` от admin с проверкой количества). Почему не SQLite и не
`InMemoryOrderStore` в integration tests. Что test на `404` проверяет на самом деле и как сделать,
чтобы он не зеленел без базы (сначала убедиться, что `POST` вернул `201`).

**Частые ошибки:** tests зависят от порядка (один создаёт заказ, другой его читает); пять `Assert` в
одном test; подстановка `IOrderStore` или SQLite в integration tests; `EnsureCreated` вместо
`MigrateAsync`; «заказ номер 1» константой; container на каждый test вместо `IClassFixture`;
правила отключены в `.editorconfig` вместо исправления; свойства analyzers в одном `csproj`.

**Если host не стартует в test.** Options с `ValidateOnStart` из десятой лабораторной требуют секцию
`Shop`: если она приходит только из AppHost, добавьте `builder.UseSetting("Shop:RatesUrl", ...)` в
`ConfigureWebHost`. `CS1705` про разные версии `Microsoft.EntityFrameworkCore` — выровнять версию
пакета в `Shop.Data`. Claims: если код LAB11 читает `email`/`role` без `ClaimTypes`, поменять две
константы в `TestAuthHandler`.

**За рамками задания (по желанию, устно):** test, перебирающий все маршруты без header; FsCheck на
сериализацию DTO туда и обратно с печатью seed; CI с обязательной зелёной проверкой.
