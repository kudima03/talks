# Л13. Данные: семейства баз и ADO.NET — house rules

Course-wide rules live in [`../../../CLAUDE.md`](../../../CLAUDE.md); read
that first. This file carries only what is specific to Л13.

Files: `L13-databases-and-ado.md` (script, **source of truth**), `README.md`
(theses with frame tags), `slides/L13-databases-and-ado-slides.tex` (one frame
per `[СЛАЙД n — …]` cue), `lab/LAB13-task.md`, `lab/LAB13-review.md`. They move
together: a frame added or cut means renumbering the cues and the `**[n]**`
tags in the same commit. The `[ЛАБА 13 — …]` block that closed the script now
lives in the lab files; the spoken lab announcement stays.

## Shape

- **60 frames**: §1 2–16, §2 17–20, §3 21–50, §4 51–60. 60 is the course cap —
  a new frame has to replace one.
- **Eight `\vote` frames**: 3, 12, 23, 36, 43, 48, 52, 57. The script has nine
  `[ВОПРОС В ЗАЛ]`; «экранировать кавычки» (between 44 and 45) is voice only.
  The vote on 43 is worded without code: a `\texttt` SQL string breaks badly
  at `\LARGE`.
- **Keylines** close §1, §3, §4 (16, 50, 60); §2 ends on content (20). Frame 46
  is the one `\statement`. No `\shout`, no speaker notes, no lab on slides.
- **Script listings are cut for the slides, not rewritten**: 34 shows only the
  reader part of `ReadForCustomerAsync`; the transaction is split over 39–40;
  long SQL literals are broken with `+`. Labels beside listings are text (`\mk`
  marks + overlay), as in Л9.
- **Frames 44–45 use `count(*)` and a `::text` cast**, not the script's
  `select *` — see «Открытые вопросы» in `README.md`. Do not align them with
  the script without re-running.
- A `\colhead` containing «Й» sits lower than its neighbour in `\versus`;
  frame 13's heads («Магазин» / «Лента игры») avoid it.

## Slides — how they were checked

All C# was compiled in a scratch `dotnet new console -n HelloWorld` (SDK
**10.0.114**, **Npgsql 10.0.3**) and run against **PostgreSQL 18** in a
container; not in the repo. The program takes a mode, so slide commands are
`dotnet run -- <mode>` and the output is what that mode printed: 27 `leak`
(`Maximum Pool Size=3;Timeout=3`; the one-line exception is wrapped on the
slide), 28–29 `count`, 31 `nulls`, 34–35 `read`, 37 `break`, 39–40 `tx` (the
exception message is English on the slide, Russian in the script), 44
`inject`, 45 `param`. SQL on 18, 33 and 55 is text, not run; 55's SQL is the
script's, not captured from EF Core. A local `DateTime` written to
`timestamptz` through `AddWithValue` did **not** throw (README, open item 3).

## Lab

About twenty minutes: three steps (PostgreSQL in the AppHost + `schema.sql`;
`PgOrderStore` reads with a parameter and a Reader; `AddAsync` in a
transaction) plus a commit, one question per step, one «сломайте нарочно»
(injection through the product name). Do not grow it back. Moved to the review
file: forgotten `CommitAsync`, the pool leak, the cast, one reader per
connection. Dropped: a count method, `IAsyncEnumerable` reads (the chain's
`IOrderStore` returns lists), the stock scenario (no `products` table).

Names follow the lab-chain contract, not the script: tables `orders(number …)`
and `order_items`, `PgOrderStore`, resource `postgres`, database `shopdb`.
**LAB14 depends on these** — same two tables, `PgOrderStore` kept for the SQL
comparison, `schema.sql` retired by the first migration.

`LAB13-review.md` quotes a minimal reference (stub `Shop.Domain`, `Shop.Api`
with `IOrderStore` and three `/orders` routes, `Shop.AppHost` on Aspire
**13.6.4** with PostgreSQL only, port 5090). It had **no JWT and no cache** —
LAB10–LAB12 were written in parallel. Not checked: `$AUTH` on the requests,
interplay with `HybridCache`, spans on the dashboard, the extra task's code.
