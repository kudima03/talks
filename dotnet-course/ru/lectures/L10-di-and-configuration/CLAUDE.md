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
- **75 frames**: §1 2–32, §2 33–51, §3 52–57, §4 58–75. Timings 30 + 25 + 10 + 15.
- **§1 runs in four steps: идея → схема → времена жизни → код.** No listing
  before frame 22; the idea and the container are drawn (5, 7, 11, 12). Do not
  move code back up. **Keyed services are not shown at all**, on a slide or by voice.
- **Ten plan frames** (`\roadmapdi`, `\roadmapcfg`, `\roadmaphttp`: 3, 10, 14, 21,
  34, 40, 45, 59, 65, 70), one before each step of §1, §2 and §4, with the
  current step picked out. Each has its own `[СЛАЙД n — план раздела …]` cue. §3
  is ten minutes and has none. Adding a step means a new entry in the macro's
  list and one more plan frame.
- **Twelve `\vote` frames** (4, 9, 17, 28, 29, 35, 43, 46, 49, 55, 60, 72), one
  per `[ВОПРОС В ЗАЛ]` in the script. Cutting one means removing its marker too.
- **Keylines close all four sections** (32, 51, 57, 75). **Frame 8 is the one
  `\statement`** («единственная честная точка входа»). No `\shout`, no `\note{}`.
- **The service locator gets three frames**: the listing (25), the real output of
  a forgotten registration with the locator and with the constructor (26), the
  price (27).
- **§4 step 1 explains the per-request client in four frames**: the code and what
  is inside `HttpClient` (61), the timeline (62), the measurement (63), the two
  errors side by side (64).
- **The closing thought of §4 stays last in the script**, after the summary and
  the lab paragraph, so the last frame is keyline 75.
- **Wording: no «в проде», «прод».** The environment is called Production where
  environments are compared; elsewhere say «после выкладки», «на сервере»,
  «развёрнутый сервис». «Ошибки», not «аварии», for the lifetime mistakes.
  Hostnames such as `db.prod.internal` in listings and real output stay.
- Listings on 23 and 24 are composed from the script's prose (it has no code for
  them). Frame 38 runs `dotnet HelloWorld.dll` where the script's listing says
  `OrdersService.dll` — house sample name, and frame 39 is its real output.
  Frame 53 shortens the consumer to its signature so the listing clears the footer.
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
| 22, 23, 24, 25, 61, 67, 68 | Compiled on SDK 11.0.100, 0 warnings (25 in both forms, behind `#if`). |
| 30, 53 | Compiled on SDK 10 in the first pass; unchanged. |
| 18 | `AddSingleton<PriceCache>()` over a scoped repository: `dotnet run` output as shown; the same build started normally with `ASPNETCORE_ENVIRONMENT=Production`. First pass. |
| 26 | SDK 11. The `INotifier` registration commented out. Constructor form: `dotnet run` as shown. Locator form: `dotnet run > app.log` in Development, then the two commands as shown. |
| 39 | `/config` returns `GetDebugView()`; run from `bin/Debug/net10.0` with the two exports and the argument of frame 38 (plus `--urls`). First pass. |
| 50 | `dotnet user-secrets set` and `/config` in Development. `init` was run first; its output line is too long for the slide and is named in the bullet. First pass. |
| 56 | `Rates__BaseUrl= dotnet run`, output as shown; the message is the same on SDK 11. |
| 63 | SDK 11. A console probe (`per-call`: `new HttpClient()` in the loop; `shared`: one client) sends 2000 sequential `GET /stub/rates/USD` to the scratch service on localhost:5090. Output as shown; more than a minute was left between the two runs so the first run's `TIME_WAIT` sockets expired (0 before each run). Over loopback Linux reuses such ports, so the run cannot exhaust them: the slide shows time and socket count, not a failure. |
| 73 | Strategy order is from the docs. Observed on SDK 11: a `POST` to a stub answering 503 is sent four times (`Attempt: '0'`…`'3'`); with `options.Retry.DisableForUnsafeHttpMethods()` once. The first script said the standard handler retries only safe methods — it does not. |
| 74 | Log of `GET /price/USD` against a stub route in the same app; lines between the two shown are cut (`...`). First pass. |

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
