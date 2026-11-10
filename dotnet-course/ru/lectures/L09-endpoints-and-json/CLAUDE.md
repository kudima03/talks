# Л9. Эндпоинты, контракт и JSON — house rules

Working notes for this lecture's directory. Course-wide rules — the palette,
the two-colour system, what may go on a slide, the build, the slide-cue
convention — live in [`../../../CLAUDE.md`](../../../CLAUDE.md). Read that
first; this file carries only what is specific to Л9.

## What is in here

| File | What it is |
|---|---|
| `L09-endpoints-and-json.md` | The spoken script. **Source of truth.** |
| `README.md` | Theses — a summary and a crib sheet for the lectern, with the frame each thesis lives on. |
| `slides/L09-endpoints-and-json-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |
| `lab/LAB09-task.md` | The lab, as handed to the student. |
| `lab/LAB09-review.md` | The same lab for whoever accepts it. |

The five move together. Every thesis in `README.md` carries a frame number
and `[—]` marks a thesis with no frame. **Adding, cutting or reordering a
frame invalidates those numbers** — renumber the cues in the script, then
re-derive the tags in `README.md`, in the same commit. The lab continues the
Л8 lab's `Shop.Api` (`OrderStore`, the `/orders` routes, `ErrorHandlingMiddleware`)
and asks for nothing the lecture does not cover. The `[ЛАБА 9 — …]` block that
used to close the script now lives in the two lab files; the spoken lab
announcement stays.

## Shape specific to Л9

- **47 frames**: §1 2–13, §2 14–23, §3 24–30, §4 31–41, §5 42–47. Six `\vote`
  frames (9, 18, 21, 27, 32, 44) — one per `[ВОПРОС В ЗАЛ]`, the question
  alone. Section timings 18 + 18 + 14 + 22 + 8 = 80.
- **The deck was built at 58 frames and cut to 47 in review** (PR #21). Material
  without a frame stays in the script as voice and is `[—]` in `README.md`. Do
  not grow the deck back by splitting frames. Removed by name in review, do not
  bring back: the «кто видел контроллеры» vote (its `[ВОПРОС В ЗАЛ]` marker
  went too; the paragraph stays), обработчики по ресурсам (`OrderEndpoints`),
  три шага, ограничение выбирает маршрут, что маршрутизация делает сама, свой
  тип в параметре (`Email.TryParse`), несколько DTO на ресурс, the endpoint
  filter listing, and the whole polymorphism block — its vote (marker gone too),
  the listing and «почему болит». The script keeps the listings of the cut
  frames.
- **Frames 5 and 17 colour the data by source** — asked for in review, and the
  one exception to the two-colour rule: `srcroute` (= `accent`), `srcbody`
  (= `alt`), `srcquery` (dark violet), `srcheader` (dark ochre), defined in the
  deck's preamble with `\src{colour}{text}`. Frame 5 colours `id` in the
  template and the parameter and `dto`; frame 17 colours the parts of two real
  `curl` requests and names each source in the same colour, with the .NET way
  to take it (`[FromRoute]`, `[FromQuery]`, `[FromHeader]`, `[FromBody]`,
  `[FromForm]`). Do not spread these colours to other frames.
- **Frame 6 highlights the pair**: `Ok<OrderDto>` and `TypedResults.Ok` in
  `accent`, `NotFound` and `TypedResults.NotFound` in `alt` (review: «так же,
  как 6» — an example with marks on it, not a comparison block).
- **Frame 7 is titled «Контроллер --- иная запись того же»** (review). Frame 8,
  the conventions, is two `cbul` lists like frame 11 (review).
- **Frame 30 has no «запрос сам по себе / относительно мира» pair** (review);
  the script says «валидатор проверяет сам запрос, логика — против текущих
  данных».
- **Wording:** «прикреплено», not «прицеплено» (review, frame 4 and the script).
- **Keylines close four sections** (13, 23, 41, 47). §3 ends on content (30);
  its «мысль раздела» paragraph stays in the script as voice.
- **The §5 closing thought was moved** before the lecture summary and the lab
  paragraph, so the last frame is the keyline (47).
- **Frame 38 is the deck's one `\statement`** — the serializer line promised
  in the course structure. There is no `\shout`.
- **Frame 22, «Зачем DTO», follows PR #21 review:** the renamed-column example
  was cut as a bad example; the reason is said plainly — DTO фиксирует
  контракт API. Do not bring the column example back.
- **Labels beside listings are text, not numbers.** Numbered marks with a key
  in the aside pushed every code frame into the footer; text labels sit in the
  free right-hand side of the code ground. Long lines were broken so the
  labels clear them — listings may be reformatted, output may not.
- **No speaker notes in the deck** — no `\note{}`, on request. Delivery lives
  in the script.
- **No lab mentions on slides.** The lab lives in `lab/`.
- **Listings that colour tokens or contain `||` use `` escapechar=` ``** (5, 6,
  17, 26): with `escapechar=|` the `||` disappears from the listing silently.
- **Script listings keep Russian comments**; slide listings are ASCII. The
  validator messages are English in both, because they are the code's output.

## What was corrected in the script against real runs

The first draft of the script made claims that the runs did not support.
These are now fixed in the script; do not revert them:

- `12,5` in a `decimal` query parameter binds to **125** on every machine
  (invariant culture, comma as thousands separator), not «depends on the
  locale». The vote answer on 18 and the table on 19 rest on this. The locale
  trap stays, moved to code that parses strings itself.
- `03/04/2026` binds to **4 March** on every machine.
- **Query-string parameter names are case-insensitive** (`Page` = `page`).
- **`HEAD` on a `MapGet` route is 405**, not served by the GET handler.
- **Ambiguous routes are flagged by analyzer ASP0022** when both are visible
  in one place; the exception still comes on the request.
- An enum name in the query string is **case-sensitive** (`paid` → 400).
- Web defaults **already read numbers from strings**.
- Polymorphic JSON is written discriminator first, then the derived type's
  properties, then the base: `{"kind":"card","last4":"4242","amount":100}`.
- A discriminator that is not first is a **500** (`NotSupportedException`),
  not a 400.

## What is on the slides — how it was checked

Every C# listing on the slides was compiled together in one scratch
`dotnet new web -n HelloWorld` project on dotnet SDK **10.0.114** (Linux), with
`FluentValidation.DependencyInjectionExtensions` **12.2.0** and
`Microsoft.AspNetCore.OpenApi` **10.0.14**; the project is not in the repo.
It carries the Л8 `ErrorHandlingMiddleware` with the two new branches.
Terminal output was captured from runs of that project on port 5090; `curl`
was 8.5.0. The progress meter `curl -i` prints into a pipe is not shown — a
terminal does not print it.

| Frames | How it was checked |
|---|---|
| 3, 4, 5, 6, 7, 16 | Compiled; 5 and 6 are their own snippets in the scratch project. |
| 6 | `TypedResults.Created` sets `Location: /orders/4` (run). |
| 11 | The dictionary-of-delegates loop compiles; the RDG fallback is from the docs, not observed. |
| 17 | `GET /orders/1/items?page=1&size=5` with `X-Request-Id` and `POST --json @order.json` run in Production; output as shown. `curl -d @order.json` (form content type) → 415. |
| 19 | Run in Production with `LANG=ru_RU.UTF-8` and with `en-US`: `12,5` → `125` both times. `03/04/2026` → `2026-03-04`. `status=99` → 99. `status=paid`, `page=` and `flag=1` fail to bind (`BadHttpRequestException`; `page=` seen as 400 in Production). |
| 26, 29 | `--json @bad-order.json` in Development → 422 with `customerEmail` and `items[0].quantity`. Field names are camel-cased by the scratch middleware per path segment. |
| 33 | Run with the stock web JSON options (no converters): enum as number, `null` written, `decimal` keeps `12.50`. |
| 35 | With both `Respect…` options: `null` `customerEmail` → `doesn't allow null values`; missing field → `missing required properties`. |
| 36 | Without the converter `{"email":{"value":…}}`, with it a string. |
| 39, 40 | Export over a 300 ms-per-row fake repository; `Transfer-Encoding: chunked`. Throwing on row 3 → 200, two elements, `curl` exits 18. |
| 43 | `jq` on `/openapi/v1.json`: `GET /orders/{id}` has `200` and `404`. |
| voice | Routing probes behind the cut frames: `/orders/abc` → 404; ASP0022 and `AmbiguousMatchException` for two `{int}` routes; `/Orders/1`, `/orders/1/`, `?Page=` match; `PUT` → 405 `Allow: GET`, `curl -I` → 405. Polymorphism: `kind` last → 500 (`must specify a type discriminator`), `kind:"crypto"` → `Read unrecognized type discriminator id`. |

Listings are ASCII only, including comments, same as every other lecture.
Labels beside listings are positioned with `remember picture` marks, so the
deck needs two pdflatex passes — `latexmk` does that. After editing such a
listing, check the labels still clear the code in the built PDF.

## Lab — how it was checked

The lab is sized for **about twenty minutes**, like labs 6–8: three steps
(DTO and a route group with `POST` → `201`; FluentValidation in a generic
endpoint filter; two new branches in the Л8 middleware) plus a commit, one
question per step and one «усложнённый» in the whole lab. Boilerplate (DTO
records, the validator, the camel-case line) is given as code. Do not grow it
back. Cut from the composer's block on purpose: `PUT`/`DELETE`, paging, the
`Email` converter, the `Respect…` options (so open item 2 does not touch the
lab), typed `Results<…>` unions and OpenAPI. The `12,5` trap is kept as the
`over` filter on the list (`150,5` → `1505` → empty list), the export stream
as the extra task.

Everything quoted in `LAB09-review.md` comes from a minimal reference — a
stub `Shop.Domain` (labs 3–5 shape, plus `NotFoundException`) and `Shop.Api`
in the state the task describes — built and run on dotnet SDK **10.0.114**
with `FluentValidation.DependencyInjectionExtensions` **12.2.0**, Development,
port 5090, curl 8.5.0; scratch directory, not committed. Seeded orders are
Л7's: Ann Smith (Paid, 300.00), Bob Brown (Created, 120.00).

| Claim | How it was checked |
|---|---|
| Step 1 | `POST --json @new-order.json` → `201`, `Location: /orders/3`; `?over=200` → orders 1 and 3. `TypedResults.Ok(order)` → the domain JSON quoted. Build clean, no `!`. |
| Step 3 | `bad-order.json` → `422` with `customerEmail` and `items[0].quantity`; `/orders/abc` → empty `404`; `?over=` → `400` via `BadHttpRequestException`; `--json '{bad'` → `400`; `150.5` → order 1, `150,5` → `[]`. |
| Live edits | Without `AddValidatorsFromAssemblyContaining` → `500`, ``No service for type 'FluentValidation.IValidator`1[…]'``. Without the `BadHttpRequestException` branch → `?over=` is `500` in Development. |
| Extra task | `/orders/export` with `curl -N` → `chunked`, no `Content-Length`; no `[EnumeratorCancellation]` → `CS8425`. |

## Open items

The author's call. Do not quietly "fix" these:

See «Открытые вопросы» in `README.md` — the same list, in Russian:

1. The `BadHttpRequestException` branch is new to the Л8 middleware (verified:
   without it, Development turns binding errors into 500).
2. `RespectRequiredConstructorParameters` forces `DeliveryDate = null` in
   `CreateOrderDto`.
3. The Latin Modern fallback has not been checked; the deck builds with PT Sans.
