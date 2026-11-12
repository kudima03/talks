# Л12. Кэширование, логирование и наблюдаемость — house rules

Course-wide rules live in [`../../../CLAUDE.md`](../../../CLAUDE.md). Read that first; this file
carries only what is specific to Л12.

| File | What it is |
|---|---|
| `L12-caching-and-observability.md` | The spoken script. **Source of truth.** |
| `README.md` | Theses with the frame each one lives on; `[—]` = voice only. |
| `slides/L12-caching-and-observability-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |
| `lab/LAB12-task.md`, `lab/LAB12-review.md` | The lab for the student and for whoever accepts it. |

Adding, cutting or reordering a frame means renumbering the cues and the README tags in the same commit.

## Shape

- **59 frames**: §1 2–23, §2 24–39, §3 40–52, §4 53–59. Timings 25 + 25 + 20 + 10 = 80.
- **Eight `\vote` frames**: 3, 11, 18, 25, 35, 41, 49, 54. The script had twelve `[ВОПРОС В ЗАЛ]`;
  four go by voice (создание заказа, файл в контейнере, горячий путь, метрика или лог) and their
  markers were removed, the paragraphs stay. Do not turn them back into frames: the cap is 60.
- **Keylines close all four sections** (23, 39, 52, 59). Frame 4 is the deck's one `\statement`
  (the cache definition). No `\shout`, no speaker notes, no lab mentions on slides.
- **The `[ЛАБА 12 — …]` block is gone from the script**; its content is in `lab/`. The §4 closing
  thought was already the last paragraph, so the last frame is its keyline.
- Labels beside listings are text placed with `\mk` marks (two pdflatex passes). The `jq` listings
  on 32 and 37 contain `|`, so 37 uses `` escapechar=` `` and 32 has none.
- Frame 36 (interleaved log lines) is a table, not terminal output: it is schematic by design.
- Frame 15 says `ValueTask`, the script listing says `Task` (which does not compile). Frame 14
  does not carry the script's «сброс уведомлением» claim. Both are in README «Открытые вопросы».

## How the slides were checked

Scratch `dotnet new web -n HelloWorld`, SDK 10.0.114, `Microsoft.Extensions.Caching.Hybrid` 11.0.0,
OpenTelemetry 1.21.0; not in the repo. Compiled there: frames 15, 31 (second form), 33, 37, 46, 48
and the policy on 8 (the slide's handler body `catalog.All()` is illustrative). Frame 57 compiled
in the lab reference. Frame 55 (`AddPostgres`, `Projects.OrdersService`) was not compiled as
written; the reference AppHost has the Redis half.

Real output: 28 (console formatter), 32 and 37 (`jq` over `app.log`, the JSON console formatter
with `IncludeScopes`, one `POST /orders/42`; two records because the scratch logs both the
template and the `LoggerMessage` form). Observed but not shown: output cache returns `Age` on the
second request and skips requests with `Authorization`. The `traceparent` on 50 is assembled from
the real ids on 37.

## Lab

About twenty minutes, three steps: AppHost + ServiceDefaults + Redis `cache`; `HybridCache` on
`GET /orders/{number:int}` with the user in the key, tag `orders`, `RemoveByTagAsync` on `POST`,
and the one break experiment (key without the user); scope `OrderNumber` + one `LoggerMessage`,
read in the dashboard. Leaves the state the chain contract fixes for LAB13: service started
through the AppHost, Redis resource `cache`. Cut from the composer's block: output caching, JSON
formatter, level override by environment, `Meter`, second service. Do not grow it back.

Checked on a stub LAB11-state solution (Aspire **13.6.4**, `AspireUseCliBundle=false`, port 5090),
telemetry read from the dashboard's API. Verified: env vars, 200/200/404, the leak, Redis keys,
log fields, trace difference, tag removal, the extra-task span, three of the five live edits.
**Not verified:** the 13.6.5 template's CLI-bundle first run, the dashboard UI itself (pages and
labels named in the task), the last two live edits. `OrderServed` is logged after
`GetOrCreateAsync` on purpose: inside the factory neither scope nor trace flows.
