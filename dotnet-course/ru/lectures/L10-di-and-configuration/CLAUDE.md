# Л10. DI, конфигурация и исходящие вызовы — house rules

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

- **Section order is DI → configuration → options → HTTP** (review of the first
  deck). §4 is built on §1 and §3: the typed client is registered from
  `RatesOptions`, so there is no «read by key now, fix later» step any more.
- **55 frames**: §1 2–24, §2 25–36, §3 37–41, §4 42–55. Timings 30 + 25 + 10 + 15.
  **Target size is 50–60 frames** (second review: «чуть покороче»); a new frame
  means cutting another.
- **§1 goes идея → схема → времена жизни → код.** No listing before frame 16;
  the idea and the container are drawn (3, 5, 7, 8). Do not move code back up.
  **Keyed services are not shown at all**, on a slide or by voice.
- **No navigation frames.** A recurring «план раздела» frame before each step was
  tried and removed in the second review. The order of a section is said aloud
  in its opening paragraph; `[ПАУЗА]` marks the transitions.
- **Exactly two `\vote` frames** (12: captive dependency; 43: HTTP request from a
  method), one per `[ВОПРОС В ЗАЛ]`. The second review asked for «буквально пару
  вопросов в зал, не усложняем»: the other ten became plain narration. Do not add more.
- **Keylines close all four sections** (24, 36, 41, 55). **Frame 6 is the one
  `\statement`** («единственная честная точка входа»). No `\shout`, no `\note{}`.
- **The service locator gets three frames**: the listing (19), the real output of
  a forgotten registration with the locator and with the constructor (20), the
  price (21).
- **§4 explains the per-request client in four frames**: the code and what is
  inside `HttpClient` (44), the timeline (45), the measurement (46), the two
  errors side by side (47).
- **The closing thought of §4 stays last in the script**, after the summary and
  the lab paragraph, so the last frame is keyline 55.
- **Wording: no «в проде», «прод».** The environment is called Production where
  environments are compared; elsewhere say «после выкладки», «на сервере»,
  «развёрнутый сервис». «Ошибки», not «аварии», for the lifetime mistakes.
  Hostnames such as `db.prod.internal` in listings and real output stay.
- Listings on 17 and 18 are composed from the script's prose (it has no code for
  them). Frame 28 runs `dotnet HelloWorld.dll` where the script's listing says
  `OrdersService.dll` — house sample name, and frame 29 is its real output.
  Frame 38 shortens the consumer to its signature so the listing clears the footer.
- `term` lines must stay under ~99 characters at `\termsm` or they leave the ground.

## How the slides were checked

Two passes. The first deck: scratch `dotnet new web -n HelloWorld` (not in the
repo) with every C# listing and `Microsoft.Extensions.Http.Resilience` 11.0.0,
built and run inside `mcr.microsoft.com/dotnet/sdk:10.0` (SDK 10.0.403) at
`/home/user/HelloWorld`, port 5090, curl 8.5.0. The review pass: the same scratch
project retargeted to `net11.0`, **SDK 11.0.100 on the host** (Linux 7.0, default
`ip_local_port_range` 32768–60999, `tcp_tw_reuse` 2), same port and curl.

| Frames | How |
|---|---|
| 16, 17, 18, 19, 44, 49, 50 | Compiled on SDK 11.0.100, 0 warnings (25 in both forms, behind `#if`). |
| 22, 38 | Compiled on SDK 10 in the first pass; unchanged. |
| 13 | `AddSingleton<PriceCache>()` over a scoped repository: `dotnet run` output as shown; the same build started normally with `ASPNETCORE_ENVIRONMENT=Production`. First pass. |
| 20 | SDK 11. The `INotifier` registration commented out. Constructor form: `dotnet run` as shown. Locator form: `dotnet run > app.log` in Development, then the two commands as shown. |
| 29 | `/config` returns `GetDebugView()`; run from `bin/Debug/net10.0` with the two exports and the argument of frame 28 (plus `--urls`). First pass. |
| 35 | `dotnet user-secrets set` and `/config` in Development. `init` was run first; its output line is too long for the slide and is named in the bullet. First pass. |
| 40 | `Rates__BaseUrl= dotnet run`, output as shown; the message is the same on SDK 11. |
| 46 | SDK 11. A console probe (`per-call`: `new HttpClient()` in the loop; `shared`: one client) sends 2000 sequential `GET /stub/rates/USD` to the scratch service on localhost:5090. Output as shown; more than a minute was left between the two runs so the first run's `TIME_WAIT` sockets expired (0 before each run). Over loopback Linux reuses such ports, so the run cannot exhaust them: the slide shows time and socket count, not a failure. |
| 53 | Strategy order is from the docs. Observed on SDK 11: a `POST` to a stub answering 503 is sent four times (`Attempt: '0'`…`'3'`); with `options.Retry.DisableForUnsafeHttpMethods()` once. The first script said the standard handler retries only safe methods — it does not. |
| 54 | Log of `GET /price/USD` against a stub route in the same app; lines between the two shown are cut (`...`). First pass. |

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
