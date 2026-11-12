# Л10. DI, исходящие вызовы и конфигурация — house rules

Working notes for this lecture's directory. Course-wide rules live in
[`../../../CLAUDE.md`](../../../CLAUDE.md); read that first.

| File | What it is |
|---|---|
| `L10-di-and-configuration.md` | The spoken script. **Source of truth.** |
| `README.md` | Theses with the frame each one lives on (`[—]` = voice only). |
| `slides/L10-di-and-configuration-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |
| `lab/LAB10-task.md`, `lab/LAB10-review.md` | The lab for the student and for whoever accepts it. |

The five move together: add, cut or reorder a frame → renumber the cues, then the
tags in `README.md`, in the same commit.

## Shape

- **59 frames**: §1 2–25, §2 26–36, §3 37–52, §4 53–59. Timings 30 + 15 + 25 + 10.
- **Twelve `\vote` frames** (3, 11, 14, 21, 22, 27, 33, 38, 45, 47, 50, 57), one
  per `[ВОПРОС В ЗАЛ]` in the script. Cutting one means removing its marker too.
- **Keylines close all four sections** (25, 36, 52, 59). **Frame 10 is the one
  `\statement`** («единственная честная точка входа»). No `\shout`, no `\note{}`.
- **Frame order follows the script's order of speech**, not the tidiest
  grouping: «Три слова» (5) sits between «Было» (4) and «Стало» (6); «Как
  выбрать» (16) precedes «Пять аварий» (17); the retry vote (33) comes after
  «Три инструмента» (32); «Разница --- в значениях» (44) precedes its vote (45).
- **The closing thought of §4 stays last in the script**, after the summary and
  the lab paragraph, so the last frame is keyline 59. The `[ЛАБА 10 — …]` block
  moved into `lab/`; the spoken lab paragraph stays.
- Two cues were moved, not just numbered: the tree (8) up to «Разрешение», «Три
  инструмента» (32) up to «Внешний сервис ненадёжен».
- Listings on 7 and 18 are composed from the script's prose (it has no code for
  them). Frame 41 runs `dotnet HelloWorld.dll` where the script's listing says
  `OrdersService.dll` — house sample name, and frame 42 is its real output.
  Frame 54 shortens the consumer to its signature so the listing clears the footer.
- `term` lines must stay under ~99 characters at `\termsm` or they leave the ground.

## How the slides were checked

Scratch `dotnet new web -n HelloWorld` (not in the repo) with every C# listing and
`Microsoft.Extensions.Http.Resilience` 11.0.0; built and run inside
`mcr.microsoft.com/dotnet/sdk:10.0` (SDK 10.0.403) at `/home/user/HelloWorld`, port
5090, curl 8.5.0 — in a container only because port 5090 was taken on the host.

| Frames | How |
|---|---|
| 4, 6, 7, 18, 19, 23, 30, 54, 56 | Compiled, 0 warnings. |
| 15 | `AddSingleton<PriceCache>()` over a scoped repository: `dotnet run` output as shown; the same build started normally with `ASPNETCORE_ENVIRONMENT=Production`. |
| 35 | Log of `GET /price/USD` against a stub route in the same app; lines between the two shown are cut (`...`). |
| 42 | `/config` returns `GetDebugView()`; run from `bin/Debug/net10.0` with the two exports and the argument of frame 41 (plus `--urls`). |
| 51 | `dotnet user-secrets set` and `/config` in Development. `init` was run first; its output line is too long for the slide and is named in the bullet. |
| 58 | `Rates__BaseUrl= dotnet run`, output as shown. |
| 34 | Strategy order is from the docs; only the retries were observed (lab run). |

## Lab

About twenty minutes, three steps plus a commit; do not grow it. Starts from the
LAB09 state and leaves the chain contract's LAB10 state: `IOrderStore` (`AllAsync`,
`FindAsync`, `AddAsync`), `InMemoryOrderStore`, `AddOrders()`, `ShopOptions` (section
`Shop`: `Currency`, `RatesUrl`, `ValidateOnStart`), `RatesClient`, `GET
/orders/{number:int}/total?currency=`. The rates stub is a `/stub/rates/{currency}`
route in the same service; nothing later depends on it. The one break experiment in
the task is the captive dependency; start-up validation, the dead neighbour and the
missing `ValidateOnStart` are live prompts in the review file. Keyed services,
secrets and a custom rates exception were cut on purpose.

Everything quoted in `LAB10-review.md` comes from a reference solution (stub
`Shop.Domain`, `Shop.Api` from the LAB09 state) built on SDK 10.0.114 and run with
`dotnet run` in the same image, Development, port 5090. The `IOptionsMonitor` reload was seen only there (polling file watcher).
