# Л14. EF Core: модель, миграции, запросы — house rules

Working notes for this lecture's directory. Course-wide rules — the palette,
the two-colour system, what may go on a slide, the build, the slide-cue
convention — live in [`../../../CLAUDE.md`](../../../CLAUDE.md). Read that
first; this file carries only what is specific to Л14.

| File | What it is |
|---|---|
| `L14-ef-core.md` | The spoken script. **Source of truth.** |
| `README.md` | Theses — a crib sheet with the frame each thesis lives on. |
| `slides/L14-ef-core-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |
| `lab/LAB14-task.md` | The lab, as handed to the student. |
| `lab/LAB14-review.md` | The same lab for whoever accepts it. |

The five move together: adding, cutting or reordering a frame means
renumbering the cues in the script and the `**[n]**` tags in `README.md`.

## Shape specific to Л14

- **59 frames**: title 1, §1 2–6, §2 7–21, §3 22–28, §4 29–39, §5 40–50,
  §6 51–59. Section timings 5 + 20 + 10 + 15 + 15 + 15 = 80.
- **Six `\vote` frames**: 3, 19, 25, 35, 42, 48. The script has seven
  `[ВОПРОС В ЗАЛ]`; the seventh is frame 32, the bad migration under the title
  «Что здесь не так?» — the listing is the question, so it is not a `\vote`.
- **Keylines close §2–§6** (21, 28, 39, 50, 59). §1 ends on content; its
  «мысль раздела» stays in the script as voice. Frame 56 is the deck's one
  `\statement` (the Bugayenko line, with the source under it). No `\shout`.
- **Frame order follows the script, not the topic list**: «не является» (4)
  comes before «берёт / требует» (5); «атрибуты или класс» (9) before the two
  listings (10, 11); shadow properties and the query filter (13) before types,
  names and keys. Do not regroup them.
- **The script's one entity + configuration listing is two frames** (10, 11);
  long lines are broken so the text labels clear the code.
- **Frame 27's tree is ASCII** (indentation, no box-drawing characters); the
  script keeps the drawn tree and its Russian comments, the slide carries them
  as labels.
- **Frame 53 says «условие „где версия та же“»**, not a count of lines: the
  script says «три строки конфигурации», the listing shows one. Leave both.
- **No speaker notes, no lab mentions on slides** — same as Л9.
- **The script's `[ЛАБА 14 — …]` block now lives in the two lab files**; the
  spoken lab announcement stays. Nothing else in the script was changed
  except the cues.

## What is on the slides — how it was checked

Every C# listing was compiled in a scratch `dotnet new console -n HelloWorld`
(not in the repo): dotnet SDK **10.0.114**, EF Core and `dotnet-ef`
**10.0.15**, Npgsql provider **10.0.3**, `EFCore.NamingConventions`
**10.0.1**, PostgreSQL **17** in Docker. Output is real; `...` marks omitted
lines, and one long line each on 33 and 37 is wrapped by `breaklines`.

| Frames | How it was checked |
|---|---|
| 15, 20, 46 | `ToQueryString()`, with and without the snake-case convention; 15 and 46 omit the leading `-- @email=…` line that 20 shows. |
| 31, 37 | `dotnet ef migrations add / list / script Initial --idempotent`; 37 stops after the first `DO` block. |
| 32, 33 | The script's migration applied for real: `25001: CREATE INDEX CONCURRENTLY cannot run inside a transaction block`. |
| 43 | 100 orders, a command-counting interceptor: 101 commands with lazy-loading proxies, 1 with `Include`. |
| 53, 54 | SQL from the `Database.Command` log, indentation as logged; the stale update raised `DbUpdateConcurrencyException`. The scratch entity adds `CreatedAt` and `Cancel()`, which the script's entity listing lacks. |
| voice | `FindAsync` after `FirstAsync` issues no command; `EndsWith` on `Email.Value` does not translate. |

## Lab

About twenty minutes: three steps (`Shop.Data` with two configuration classes
and the context; a migration through local `dotnet-ef`, applied by a separate
command; `EfOrderStore` swapped in by one registration and its SQL read next
to `PgOrderStore`'s), one question per step, one «усложнённый», one «сломайте
нарочно» (drop `Include` → `"total":0`). The task is ~145 lines because the
boilerplate is given as code; do not grow it. Carried forward to Л15:
`Shop.Data`, `ShopDbContext`, `Migrations/`, `EfOrderStore` (scoped, in
`AddOrders()`), `PgOrderStore` kept but unregistered.

Do not reverse without re-running the reference:

- Order number and item key are **shadow properties**, `Customer` is an
  **owned type**, `Order.Id` (the domain `Guid`) is ignored — the domain gains
  only a private constructor and the tables keep Л13's column names.
- `Microsoft.EntityFrameworkCore.Relational` is referenced explicitly and
  `Design`/`dotnet-ef` are pinned to `10.*`: without the first `Shop.Api`
  fails with `CS1705`; without the pin the .NET 11 packages are picked up.
- `EfOrderStore` lives in `Shop.Api` beside `PgOrderStore`, so the lab works
  wherever Л10 put `IOrderStore`. `dotnet ef` goes through an
  `IDesignTimeDbContextFactory` in `Shop.Data` and never builds `Shop.Api`.

`LAB14-review.md` quotes a reference solution (stub `Shop.Domain`,
`Shop.Data`, `Shop.Api`, port 5090, PostgreSQL in Docker). **Not reproduced:**
the AppHost, JWT and HybridCache of labs 11–12 — open item 3 in `README.md`.
