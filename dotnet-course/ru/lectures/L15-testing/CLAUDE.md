# Л15. Тестирование и качество — house rules

Course-wide rules live in [`../../../CLAUDE.md`](../../../CLAUDE.md); read that first. This file
carries only what is specific to Л15, the last lecture of the course.

| File | What it is |
|---|---|
| `L15-testing.md` | The spoken script. **Source of truth.** |
| `README.md` | Theses with the frame each one lives on; `[—]` = voice only. |
| `slides/L15-testing-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |
| `lab/LAB15-task.md`, `lab/LAB15-review.md` | The lab for the student and for whoever accepts it. |

Adding, cutting or reordering a frame means renumbering the cues and the README tags in one commit.

## Shape

- **60 frames** (the brief's ceiling — do not add without cutting): §1 2–10, §2 11–13, §3 14–28,
  §4 29–41, §5 42–45, §6 46–51, §7 52–55, §8 56–60. Timings 10/5/20/20/5/10/5/5 = 80.
- **Eight `\vote` frames**: 3, 8, 18, 20, 31, 39, 44, 47 — every `[ВОПРОС В ЗАЛ]` in the script.
- **Keylines** close §1 (10), §3 (28), §4 (41), §8 (58). §2, §5, §7 end on content; their «мысль
  раздела» paragraphs stay as voice. **Statements**: 25 («Не управлять порядком тестов. Никогда.»)
  and 51 («В третьей лекции вы угадывали. Теперь вы мерите.», which closes §6 instead of a keyline).
- **Ending follows the script's farewell**: keyline 58, then 59 «Пятнадцать лекций» (course recap), then
  60 — the deck's one `\shout`, «Продолжайте вскрывать». No contact frame, no speaker notes, no lab
  mentions on slides (as in Л9). The lab announcement is the script's last paragraph, voice only.
- **Script edits besides cues**: the `[ЛАБА 15 — …]` block removed; the §8 «мысль раздела» paragraph
  moved before the farewell (so 58 precedes 59–60); the stopwatch vote paragraph split in two after
  the question so that frame 48 shows under the answer. No wording changed.
- Frame 16 (argument order) precedes 17 (`Theory`) because the script says it in that order.
- Frame 50 is a `tabular` of real BenchmarkDotNet numbers, not a `term` listing: the raw table is
  135 columns wide. Columns kept: Mean, Error, Ratio, Gen0, Allocated; StdDev, Median, RatioSD and
  Alloc Ratio are dropped, no number is altered.
- Frame 54's error line uses `breaklines=true` — the terminal wraps it too.

## How it was checked (dotnet SDK 10.0.114, Linux, scratch projects, not committed)

- 15, 17, 22: `HelloWorld` + `HelloWorld.Tests` (xUnit 2.9.3, NSubstitute, FakeTimeProvider) compile
  and pass; output on 17 is `dotnet test --logger "console;verbosity=normal"`, cut with `...`.
- 16: same test with `Assert.Equal(order.Status, OrderStatus.New)` — output as shown.
- 34, 37: not compiled verbatim (`IRatesClient`, `ErrorResponse` are the script's names); the same
  shape runs green in the lab reference — `WebApplicationFactory<Program>`, Testcontainers 4.16.0,
  `UseSetting`, `ConfigureTestServices`. Frame 34 uses `new PostgreSqlBuilder("postgres:17-alpine")`:
  the parameterless constructor in the script's listing is obsolete (CS0618).
- 49, 50: `Benchmarks` console project, BenchmarkDotNet 0.15.8, .NET 10.0.14, i7-13700; second of two
  runs (the first was noisier). Debug run refuses with «non-optimized».
- 54: `dotnet build | grep -m1 CA1849` in `HelloWorld` with the three properties of frame 53.
- Lab: reduced LAB14-state reference (Domain, Data with one migration, Api with JWT, policy, HybridCache
  over Redis, EF store; no AppHost, no `RatesClient`) — 12 tests green, every claim in the review file
  observed on it. The lab against a real LAB10–LAB14 chain has **not** been run: those labs were
  written in parallel.

## Lab

Carries forward the LAB14 solution; adds `Shop.Tests`, `Directory.Build.props`, `.editorconfig`.
About twenty minutes: three steps and a commit, one question per step, one deliberate break (CA1849).
The fixture and the test auth handler are given as code — do not turn them into an exercise. It starts
Redis as well as PostgreSQL because LAB12's cache sits on `GET /orders/{number}`. Benchmark is the
extra task; CI, FsCheck and the all-routes test are review-file prompts only.

## Open items (the author's call — see «Открытые вопросы» in `README.md`)

1. The script says `public partial class Program;` is required; on .NET 10 it is not (verified).
2. The script's listing still has `new PostgreSqlBuilder()`.
3. The lab uses `latest-recommended` plus one raised rule; frame 53 shows `latest-all`.
4. The script's lab announcement promises a benchmark and a green pipeline; the lab makes them optional.
5. Latin Modern fallback not checked.
