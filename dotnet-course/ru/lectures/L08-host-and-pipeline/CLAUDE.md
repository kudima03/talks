# Л8. ASP.NET Core: хост и конвейер запроса — house rules

Working notes for this lecture's directory. Course-wide rules — the palette,
the two-colour system, what may go on a slide, the build, the slide-cue
convention — live in [`../../../CLAUDE.md`](../../../CLAUDE.md). Read that
first; this file carries only what is specific to Л8.

## What is in here

| File | What it is |
|---|---|
| `L08-host-and-pipeline.md` | The spoken script. **Source of truth.** |
| `README.md` | Theses — a summary and a crib sheet for the lectern, with the frame each thesis lives on. |
| `slides/L08-host-and-pipeline-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |

The three move together. Every thesis in `README.md` carries a frame number
and `[—]` marks a thesis with no frame. **Adding, cutting or reordering a
frame invalidates those numbers** — renumber the cues in the script, then
re-derive the tags in `README.md`, in the same commit. There is no `lab/`
directory yet: the lab is described only by the `[ЛАБА 8 — …]` block at the
end of the script.

## Shape specific to Л8

- **55 frames**: §1 2–14, §2 15–18, §3 19–35, §4 36–42, §5 43–54, then the
  lab (55). Five `\vote` frames (3, 10, 29, 38, 50) — one per `[ВОПРОС В
  ЗАЛ]`, the question alone. Section timings 20 + 10 + 25 + 10 + 15 = 80.
- **50–60 frames is the agreed size.** A first version had 98; the author
  asked to bring it to 50–60 («сейчас слишком много»). Material without a
  frame stays in the script as voice and is `[—]` in `README.md`. Do not grow
  the deck back by splitting frames.
- **Cut on request, do not bring back:** the configuration-sources frame and
  the «`Build` проверяет» frame (ValidateOnBuild) — both removed by name.
  Frame 6 carries a custom registration (`AddScoped<IOrderService,
  OrderService>()`, `AddSingleton(TimeProvider.System)`) next to the
  framework modules, with the three lifetimes — asked for in review.
- **Merged frames:** 8 is the two phases plus what `Build` does; 11 is the
  shutdown sequence plus the timeout and `Dispose`; 37 is `HttpContext` as a
  table of its properties (no separate Request/Response/Items frames); 44 is
  `IHostedService` plus the start trap; 46 is the four loop decisions plus
  «exception stops the host, exit code 0».
- **The deck is what the lecturer reads from.** Slides carry short marks on
  the important things (tables, asides, labels beside listings) rather than
  bare titles — that was the request. Keep marks short; delivery lives in the
  script.
- **No speaker notes in the deck** — no `\note{}`, on request.
- **Cut in review (PR #20), do not bring back:** the whole forwarded-headers
  block (`X-Forwarded-*`, `UseForwardedHeaders`, `KnownProxies`, the HTTPS
  redirect/HSTS paragraph and the §2 closing line about proxies) — «Про
  заголовки и прокси не будем говорить»; the dev-certificate expiry
  paragraph — «Не будем разбирать сертификаты детально»; the
  liveness/readiness paragraph and the orchestrator-in-shutdown paragraph —
  «Меньше говорим про прокси и оркестраторы». Shutdown talks about «тот, кто
  запустил процесс», not an orchestrator.
- **Frame 22 is the explicit pipeline in `Program.cs`**, asked for in review
  («Добавить листинг как это выглядит в Program.cs, явный пайплайн»). Its
  order matches the chain on frame 20 and the short-circuit on frame 33:
  ошибки → логирование → маршрутизация → аутентификация → авторизация →
  обработчик.
- **The three order examples (24)** are authentication before authorization,
  errors outside everything, routing before authorization. The proxy example
  went with the forwarded-headers block.
- **Frame 21 is the deck's one `\statement`** (the middleware definition).
  There is no `\shout`.
- **Numbers beside long listings (30, 45, 49) key into the table on the next
  frame (31, 46) or the aside** — the lines are too long for text labels.
- **Real output is never re-wrapped.** A line too long for the slide is cut
  with `...` or quoted in prose (8, 28, 47); listings may be reformatted,
  output may not.
- **Comparison order:** the side to follow is on the left in teal — «на
  старте» before «на первом запросе» (7), Kestrel before the socket server
  (16), `InvokeAsync` parameters before the constructor (28).
- Frames 14, 35, 42, 54 close four sections with `\keyline`; §2 ends on
  content (18).

## What is on the slides — how it was checked

Every C# listing on the slides was compiled together in one scratch
`dotnet new web -n HelloWorld` project on dotnet SDK **10.0.114** (Linux), with
`Quartz.Extensions.Hosting` **4.7.1**; the project is not in the repo. Terminal
output was captured from runs of that project and of a stock six-line one; the
project directory is shown as `/home/user/HelloWorld`.

| Frames | How it was checked |
|---|---|
| 4, 12 | Six-line app run with `dotnet run`; the log is the real start and Ctrl+C. |
| 6 | Compiled: framework modules, `AddScoped<IOrderService, OrderService>()`, `AddSingleton(TimeProvider.System)`. |
| 8 | A registration after `Build` throws `The service collection cannot be modified because it is read-only.` |
| 11 | `new HostOptions().ShutdownTimeout` is `00:00:30`. |
| 17 | `dotnet HelloWorld.dll` run from `bin/Debug/net10.0` with no settings, `ASPNETCORE_URLS`, both env and `--urls`, `ASPNETCORE_HTTP_PORTS`; lines are the real `Now listening on`. The Kestrel-section override was seen in a separate run (it logs `Overriding address(es)`). |
| 16 | `ListenOptions.Protocols` default is `Http1AndHttp2`; HTTP/3 was not tried. |
| 18 | `new KestrelServerLimits()` printed. 31 MB to a default `MapPost` → `413` on the first body read (`BadHttpRequestException: Request body too large`). |
| 22, 30, 32 | Explicit pipeline run; `curl` on `/orders/404` and `/orders/500` — output on 32 is real, log lines are the first three of the record. |
| 23, 34 | Inline middleware needs `using System.Diagnostics;`. A middleware that never calls `next` → empty `200` (the lambda needs explicit parameter types, or the call is ambiguous). |
| 24 | `UseAuthorization` before `UseRouting`: no build warning; a `RequireAuthorization()` endpoint fails on the first request with the message quoted. |
| 28 | Scoped service in a middleware constructor: Development fails at startup with the quoted message; Production gives every request the same instance. |
| 37 | `HttpContext.Items` is `IDictionary<object, object?>`. |
| 44 | `IHostedService.StartAsync` with a 5 s delay holds `Now listening on` for 5 s. A `BackgroundService` with `Thread.Sleep(5000)` before its first `await` does not delay startup on .NET 10. |
| 45, 47 | Run: `CleanupService` stops on SIGTERM in ~60 ms; `ReportService` gets a new `ReportBuilder` each iteration. A scoped dependency in its constructor fails in Development with the quoted message. |
| 46 | A `BackgroundService` throwing out of `ExecuteAsync` stops the host; the process exits with code 0. |
| 49 | Quartz 4.7.1: the 3.x form (`AddJob<T>(key)`, `Task Execute(context)`) does not compile; the listing is the 4.x form. Fired every two seconds with a test cron. |

Listings are ASCII only, including comments, same as every other lecture.
Labels and numbers beside listings (4, 6, 22, 23, 27, 30, 45, 49) are
positioned with `remember picture` marks inside the listing, so the deck needs
two pdflatex passes — `latexmk` does that. After editing such a listing, check
the labels still sit on their lines in the built PDF.

## Open items

The author's call. Do not quietly "fix" these:

See «Открытые вопросы» in `README.md` — the same list, in Russian:

1. Logging inside error handling never logs failed requests (verified).
2. `ErrorHandlingMiddleware` turns Kestrel's 413 into 500 (verified).
3. The Latin Modern fallback has not been checked; the deck builds with PT Sans.
