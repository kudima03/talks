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
| `lab/LAB08-task.md` | The lab, as handed to the student. |
| `lab/LAB08-review.md` | The same lab for whoever accepts it. |

The five move together. Every thesis in `README.md` carries a frame number
and `[—]` marks a thesis with no frame. **Adding, cutting or reordering a
frame invalidates those numbers** — renumber the cues in the script, then
re-derive the tags in `README.md`, in the same commit. The lab continues Л7's
`Shop.RawServer`: `OrderStore` and the same three routes move into a new
`dotnet new web` project, `Shop.Api`, which lives until the end of the
course. It asks for nothing the lecture does not cover — no DTOs, binding,
validation or OpenAPI (Л9), no options or lifetimes beyond frame 6 (Л10).
The `[ЛАБА 8 — …]` block that used to close the script now lives in the two
lab files; the spoken lab announcement stays.

## Shape specific to Л8

- **45 frames**: §1 2–13, §2 14–17, §3 18–28, §4 29–35, §5 36–45. Four
  `\vote` frames (3, 9, 31, 42) — one per `[ВОПРОС В ЗАЛ]`, the question
  alone. Section timings 20 + 10 + 25 + 10 + 15 = 80.
- **The size was cut twice on request** (98 → 55 → 45). Material without a
  frame stays in the script as voice and is `[—]` in `README.md`. Do not grow
  the deck back by splitting frames. Removed by name in review, do not bring
  back: configuration sources, «`Build` проверяет», «Где ломаться», default
  pipeline, three ways to write middleware, the `InvokeAsync` class, the
  captive dependency, the «граница процесса» vote (its `[ВОПРОС В ЗАЛ]`
  marker went too; the paragraph stays), the manual `curl` check,
  scope-per-iteration in the background, «фоновая работа невидима», the lab
  slide.
- **Frame 6 carries a custom registration** (`AddScoped<IOrderService,
  OrderService>()`, `AddSingleton(TimeProvider.System)`) next to the framework
  modules, with the three lifetimes — asked for in review.
- **§2 is titled «Kestrel»** (section, opener and footer). Frame 15 lists
  Kestrel's features only — the comparison with the lab's socket server and
  its closing line were removed. Frame 16 is a where/how/when table plus one
  real `Now listening on`.
- **Merged frames:** 7 is the two phases plus what `Build` does; 10 is the
  shutdown sequence plus the timeout and `Dispose`; 30 is `HttpContext` as a
  table of its properties; 37 is `IHostedService` plus the start trap; 39 is
  the four loop decisions plus «exception stops the host, exit code 0»;
  25 is the five error-handling decisions with «в лог» / «клиенту» columns;
  26 is the short circuit plus who answers by itself.
- **No lab mentions on slides** (review). The lab lives in `lab/`.
- **The deck is what the lecturer reads from.** Slides carry short marks on
  the important things (tables, asides, labels beside listings) rather than
  bare titles. Keep marks short; delivery lives in the script.
- **No speaker notes in the deck** — no `\note{}`, on request.
- **Cut in review (PR #20), do not bring back:** the whole forwarded-headers
  block (`X-Forwarded-*`, `UseForwardedHeaders`, `KnownProxies`, the HTTPS
  redirect/HSTS paragraph) — «Про заголовки и прокси не будем говорить»; the
  dev-certificate expiry paragraph — «Не будем разбирать сертификаты
  детально»; the liveness/readiness and orchestrator-in-shutdown paragraphs —
  «Меньше говорим про прокси и оркестраторы».
- **Frame 21 is the explicit pipeline in `Program.cs`** (review). Its order
  matches the chain on frame 19 and the short-circuit on frame 26: ошибки →
  логирование → маршрутизация → аутентификация → авторизация → обработчик.
- **The three order examples (23)** are authentication before authorization,
  errors outside everything, routing before authorization.
- **Frame 20 is the deck's one `\statement`** (the middleware definition).
  There is no `\shout`.
- **Numbers beside long listings (24, 38, 41) key into the table on the next
  frame (25, 39) or the aside** — the lines are too long for text labels.
- **Real output is never re-wrapped.** A line too long for the slide is cut
  with `...` or quoted in prose (7); listings may be reformatted, output may
  not.
- Frames 13, 28, 35, 45 close four sections with `\keyline`; §2 ends on
  content (17).

## What is on the slides — how it was checked

Every C# listing on the slides was compiled together in one scratch
`dotnet new web -n HelloWorld` project on dotnet SDK **10.0.114** (Linux), with
`Quartz.Extensions.Hosting` **4.7.1**; the project is not in the repo. Terminal
output was captured from runs of that project and of a stock six-line one; the
project directory is shown as `/home/user/HelloWorld`.

| Frames | How it was checked |
|---|---|
| 4, 11 | Six-line app run with `dotnet run`; the log is the real start and Ctrl+C. |
| 6 | Compiled: framework modules, `AddScoped<IOrderService, OrderService>()`, `AddSingleton(TimeProvider.System)`. |
| 7 | A registration after `Build` throws `The service collection cannot be modified because it is read-only.` |
| 10 | `new HostOptions().ShutdownTimeout` is `00:00:30`. |
| 15 | `ListenOptions.Protocols` default is `Http1AndHttp2`; HTTP/3 was not tried. |
| 16 | `dotnet HelloWorld.dll` run from `bin/Debug/net10.0` with both `ASPNETCORE_URLS` and `--urls`; the line is the real `Now listening on`. The Kestrel-section override was seen in a separate run (it logs `Overriding address(es)`). |
| 17 | `new KestrelServerLimits()` printed. 31 MB to a default `MapPost` → `413` on the first body read. |
| 21, 24 | Explicit pipeline with `ErrorHandlingMiddleware` run; `/orders/404` → 404 JSON, `/orders/500` → «Internal error» + `traceId`, the exception message only in the log. |
| 22, 27 | Inline middleware needs `using System.Diagnostics;`. A middleware that never calls `next` → empty `200` (the lambda needs explicit parameter types, or the call is ambiguous). |
| 23 | `UseAuthorization` before `UseRouting`: no build warning; a `RequireAuthorization()` endpoint fails on the first request with the message quoted. |
| 30 | `HttpContext.Items` is `IDictionary<object, object?>`. |
| 37 | `IHostedService.StartAsync` with a 5 s delay holds `Now listening on` for 5 s. A `BackgroundService` with `Thread.Sleep(5000)` before its first `await` does not delay startup on .NET 10. |
| 38, 39 | `CleanupService` stops on SIGTERM in ~60 ms. A `BackgroundService` throwing out of `ExecuteAsync` stops the host; the process exits with code 0. |
| 41 | Quartz 4.7.1: the 3.x form (`AddJob<T>(key)`, `Task Execute(context)`) does not compile; the listing is the 4.x form. Fired every two seconds with a test cron. |

Listings are ASCII only, including comments, same as every other lecture.
Labels and numbers beside listings (4, 6, 21, 22, 24, 38, 41) are
positioned with `remember picture` marks inside the listing, so the deck needs
two pdflatex passes — `latexmk` does that. After editing such a listing, check
the labels still sit on their lines in the built PDF.

## Lab — how it was checked

Everything quoted in `LAB08-review.md` comes from a minimal reference
solution built and run on dotnet SDK **10.0.114** (Linux) in a scratch
directory, not committed: a `Shop.Domain` with `DomainException` and
`NotFoundException`, a `Shop.RawServer` on `TcpListener` (port 8088) and
`Shop.Api` from `dotnet new web`. Two seeded orders — Ann Smith, Paid,
300.00; Bob Brown, Created, 120.00 — the same as Л7's lab. Paths are shown as
`/home/user/Shop.Api`; `Date` headers and `traceId`s are as captured.

| Claim in the review | How it was checked |
|---|---|
| Two-server comparison | Both servers run side by side, `curl -i` on each. A string-returning handler answers `chunked`, `Results.Text` answers with `Content-Length`; `/nope` → empty `404`; `POST /orders` → empty `405` with `Allow: GET`; `/orders/abc` → `404` (route constraint). |
| `NotFoundException` without middleware | `dotnet run` (Development) → plain-text trace in the body; `dotnet run -- --environment Production` → empty `500`, trace only in the Kestrel log. |
| Reference session, steps 4–5 | Run as `dotnet Shop.Api.dll --urls …` (Production). `/fail/slow` with `curl --max-time 1` → curl exit 28, one `Information` line; `/fail/late` → curl exit 18. The `traceId` from a response is **not** in the console log. |
| The missing log line | Without the filter, `/orders/7` and `/fail/boom` have no `RequestLoggingMiddleware` line. With logging before errors the line says `404`; with `try/finally` it says `200` — both run. |
| Step 6 | `next` without `await` → `CS4014`, `/fail/slow` answers an empty `200` at once; no `next` at all → `CS9113`, empty `200` everywhere; `InvokeAsync` renamed → clean build, `No public 'Invoke' or 'InvokeAsync' method found…` at start. |
| Step 7 | Stop with `SIGTERM` (a background job of a non-interactive shell ignores `SIGINT`) took 0.03 s, exit 0. `Thread.Sleep(10 s)` loop: no warning, stop took 5.04 s, one more stats line after `Application is shutting down...`. Exception out of `ExecuteAsync` → host stops, exit 0. `IOrderService` in the background service → `Cannot consume scoped service…` on `Build` in Development, silent start in Production. |
| 413 → 500 | A temporary `MapPost` copying the body to `Stream.Null`, 31 000 000 bytes → `500` `Internal error`, `BadHttpRequestException: Request body too large…` in the log. |
| Extra task | `dotnet run -- --urls http://localhost:5001` listened on 5001 and logged stats (`--urls` beats the launch profile); two instances side by side were not run. Quartz.Extensions.Hosting 4.7.1, the 4.x listing from frame 41 with `0/5 * * * * ?` and a 7 s job: runs do not overlap. `Program.cs` needs `using Quartz;`. |

**The lab's answer to open item 1 is the exception filter**
(`catch (Exception e) when (LogFailed(...))` returning `false`): it keeps the
lecture's order and gives one line per request, at the price of writing the
exception type, not the final status, for a failed request. The `finally`
variant is called out in the review as wrong (it logs `200`), the
logging-first variant as acceptable with its price named. If the lecture's
order changes, steps 4–5 and the table in the review change with it.

## Open items

The author's call. Do not quietly "fix" these:

See «Открытые вопросы» in `README.md` — the same list, in Russian:

1. Logging inside error handling never logs failed requests (verified). The
   lab works around it with an exception filter, see above; the lecture is
   unchanged.
2. `ErrorHandlingMiddleware` turns Kestrel's 413 into 500 (verified). In the
   lab it is an «усложнённый» question only.
3. The Latin Modern fallback has not been checked; the deck builds with PT Sans.
