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

The three move together. Every thesis in `README.md` carries a frame number
and `[—]` marks a thesis with no frame. **Adding, cutting or reordering a
frame invalidates those numbers** — renumber the cues in the script, then
re-derive the tags in `README.md`, in the same commit. There is no `lab/`
directory yet: the lab is described only by the `[ЛАБА 9 — …]` block at the
end of the script, and it continues the Л8 lab's service.

## Shape specific to Л9

- **58 frames** (asked for 50–60): §1 2–15, §2 16–30, §3 31–38, §4 39–52,
  §5 53–58. Eight `\vote` frames (3, 10, 24, 27, 34, 40, 43, 55) — one per
  `[ВОПРОС В ЗАЛ]`, the question alone. Section timings 18 + 18 + 14 + 22 + 8
  = 80.
- **Keylines close four sections** (15, 30, 52, 58). §3 ends on content (38);
  its «мысль раздела» paragraph stays in the script as voice.
- **The §5 closing thought was moved** before the lecture summary and the lab
  paragraph, so the last frame is the keyline (58).
- **Frame 49 is the deck's one `\statement`** — the serializer line promised
  in the course structure. There is no `\shout`.
- **Frame 28, «Зачем DTO», follows PR #21 review:** the renamed-column example
  was cut as a bad example; the reason is said plainly — DTO фиксирует
  контракт API. Do not bring the column example back.
- **Labels beside listings are text, not numbers.** Numbered marks with a key
  in the aside pushed every code frame into the footer; text labels sit in the
  free right-hand side of the code ground. Long lines were broken so the
  labels clear them — listings may be reformatted, output may not.
- **No speaker notes in the deck** — no `\note{}`, on request. Delivery lives
  in the script.
- **No lab mentions on slides.** The lab lives in the script's
  `[ЛАБА 9 — …]` block.
- **Listings with `||` use `` escapechar=` ``** (23, 33): with the usual
  `escapechar=|` the `||` disappears from the listing silently.
- **Script listings keep Russian comments**; slide listings are ASCII. The
  validator messages are English in both, because they are the code's output.

## What was corrected in the script against real runs

The first draft of the script made claims that the runs did not support.
These are now fixed in the script; do not revert them:

- `12,5` in a `decimal` query parameter binds to **125** on every machine
  (invariant culture, comma as thousands separator), not «depends on the
  locale». The vote answer on 24 and the table on 25 rest on this. The locale
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
| 4, 5, 8, 14, 21, 29 | Compiled; frame 14's `MapOrders` is the running app's route table. |
| 7 | `TypedResults.Created` sets `Location: /orders/4` (run). |
| 12 | The dictionary-of-delegates loop compiles; the RDG fallback is from the docs, not observed. |
| 19 | `/orders/abc` → 404, empty body. Two `{id:int}` / `{number:int}` routes: ASP0022 at build, `AmbiguousMatchException` on the request. `/orders/latest` wins over `{id:int}`. |
| 20 | `/Orders/1`, `/orders/1/` → 200; `?Page=3` binds `page`. `PUT` → 405 `Allow: GET`; `curl -I` → 405. |
| 22 | `curl -d @order.json` (form content type) → 415. `ids=1&ids=2` → `int[]`. |
| 23 | `Email.TryParse` used by binding; `email=bad` → 400 in Production. |
| 25 | Run in Production with `LANG=ru_RU.UTF-8` and with `en-US`: `12,5` → `125` both times. `03/04/2026` → `2026-03-04`. `status=99` → 99. `status=paid`, `page=` and `flag=1` fail to bind (`BadHttpRequestException`; `page=` seen as 400 in Production). |
| 33, 36, 37 | `--json @bad-order.json` in Development → 422 with `customerEmail` and `items[0].quantity`. Field names are camel-cased by the scratch middleware per path segment. |
| 41 | Run with the stock web JSON options (no converters): enum as number, `null` written, `decimal` keeps `12.50`. |
| 44, 45 | Serialized `CardPayment` as shown; `kind` last → 500 with the quoted message; `kind:"crypto"` → `Read unrecognized type discriminator id`. |
| 46 | With both `Respect…` options: `null` `customerEmail` → `doesn't allow null values`; missing field → `missing required properties`. |
| 47 | Without the converter `{"email":{"value":…}}`, with it a string. |
| 50, 51 | Export over a 300 ms-per-row fake repository; `Transfer-Encoding: chunked`. Throwing on row 3 → 200, two elements, `curl` exits 18. |
| 54 | `jq` on `/openapi/v1.json`: `GET /orders/{id}` has `200` and `404`. |

Listings are ASCII only, including comments, same as every other lecture.
Labels beside listings are positioned with `remember picture` marks, so the
deck needs two pdflatex passes — `latexmk` does that. After editing such a
listing, check the labels still clear the code in the built PDF.

## Open items

The author's call. Do not quietly "fix" these:

See «Открытые вопросы» in `README.md` — the same list, in Russian:

1. The `BadHttpRequestException` branch is new to the Л8 middleware (verified:
   without it, Development turns binding errors into 500).
2. `RespectRequiredConstructorParameters` forces `DeliveryDate = null` in
   `CreateOrderDto`.
3. The Latin Modern fallback has not been checked; the deck builds with PT Sans.
