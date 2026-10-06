# Л4. C#: конструкции, паттерны, делегаты и ошибки — house rules

Working notes for this lecture's directory. Course-wide rules — the palette,
the two-colour system, what may go on a slide, the build, the slide-cue
convention — live in [`../../../CLAUDE.md`](../../../CLAUDE.md). Read that
first; this file carries only what is specific to Л4.

## What is in here

| File | What it is |
|---|---|
| `L04-csharp-constructs.md` | The spoken script. **Source of truth.** |
| `README.md` | Theses — a summary and a crib sheet for the lectern, with the frame each thesis lives on. |
| `slides/L04-csharp-constructs-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |
| `lab/LAB04-task.md` | The lab, as handed to the student. |
| `lab/LAB04-review.md` | The same lab for whoever accepts it. |

The five move together. Every thesis in `README.md` carries a frame number and
`[—]` marks a thesis with no frame. **Adding, cutting or reordering a frame
invalidates those numbers** — renumber the cues in the script, then re-derive
the tags in `README.md`, in the same commit. The lab asks for nothing the
lecture does not cover.

## The deck is the lecturer's support

The author does not learn the script: he lectures by expanding the slides.
That decides two things for this deck:

- **Every important moment has a frame.** The review of the script asked for
  slides on plain `if`, the ladder, early return, `when`, `var`, target-typed
  `new`, collection expressions, ranges, tuples, lambdas, events, `int?`,
  enabling annotations, the null operators, `TryParse` and try/catch
  mechanics. Do not thin them out to match Л3's density.
- **No `\note{}`.** The deck carries no speaker notes and no `\shownotes`
  block; the delivery lives in the script only. (`make notes` still exists
  because the Makefile is shared; it builds the same deck.)

## Code on the slides — how it was checked

| Frames | How it was verified |
|---|---|
| 4–9, 13, 14, 16, 19–21, 24, 26, 31, 32, 34, 37, 39–42, 46, 49 | Compiled and run on dotnet SDK **10.0.112** in a stock `dotnet new console`, with the minimal types around them (`Order`, `User`, `Point`); outputs in comments (`// False`, `// 2..9`) are what it printed. |
| 11, 12, 48 | Fragments: `Process`, `row`, `Invoice`, `file` are not defined anywhere. The constructs themselves (`var`, `new()` into a field and a parameter, catch order, `throw;`) were compiled separately. |
| 23 | `CS8510`, `CS8509` and `CS8524` are real diagnostics from the same SDK: general arm above a specific one, no `_` on `object`, an enum without `_`. |
| 40 | `NameLength` gives `CS8602`, as the comment says. |
| 48 | `throw e;` gives `CA2200`; a general `catch` above a specific one is error `CS0160`. |
| 51 | Real output of `PORT=abc dotnet run` on a `HelloWorld` whose `Program.cs` is the frame-49 listing plus two lines on top. Captured in a scratch directory, path rewritten to `/home/user/HelloWorld`; line numbers 1, 8, 16 match that file. Re-capture rather than edit. |

Listings are ASCII only, including string literals: the script's listings use
the same English strings as the deck. Two frames break the course's Allman
style on purpose: the ladder (5) and early return (6) use `} else {` so that
two columns fit at a readable size. Frame 8 is set at 6.4pt, the smallest in
the deck; it shows method bodies without the method around them for the same
reason.

## Facts this lecture commits to

- Section timings: **15 / 20 / 15 / 12 / 18** = 80 minutes.
- **52 slides**, numbered 1–52 in order: §1 2–17, §2 18–28, §3 29–35,
  §4 36–44, §5 45–52.
- The order in §1 is the ladder **if → тернарный → switch → switch-выражение**,
  then values: **var → new() → коллекции → индексы и диапазоны**. The
  switch expression is called «аналог тернарного в семье switch» (frame 10).
- The grade ladder (`score >= 90 → "A"`) is one thread through frames 5, 9 and
  §2's relational pattern: keep the numbers in step if one of them changes.
- `??`, `??=` and `?.` live in §4, not §1.
- The pattern ladder has **six steps**: константа, тип, свойство, отношение и
  логика, позиция, список.
- One `\statement` (frame 44, «каждый ! — это долг»), no `\shout`. Four
  `\keyline`s (frames 17, 28, 35, 52); §4 closes on the statement.
- Questions to the room (`[ВОПРОС В ЗАЛ]`) have no `\vote` frames, as in Л3.

## Cut in review — not to be restored without asking

- **The lab announcement** at the end of the script.
- **The async aside** in §5 (exceptions inside tasks, cancellation).
- **The IQueryable / expression-tree bridge** in §3 and the paragraph on telling
  `Func` from `Expression` apart. `ru/lecture-notes.md` still lists it as a
  §3 link to Л5.
- **Method-group caching**, the decompiler paragraph and the «where this leaks
  in services» paragraph in §3.
- **Tuples in `switch`** and the order state machine example.
- **The two remarks on enabling annotations** (warnings in old projects,
  unannotated packages).
- **«Почему я на этом останавливаюсь»** at the end of §1.

## Open items

The author's call. Do not quietly "fix" these:

1. **The Latin Modern fallback** has not been checked for this deck; it builds
   with PT Sans.
2. **Timings were not re-budgeted** after the review added §1 material (if,
   ladder, early return, `when`, `var`) and cut parts of §3.
3. **`ru/lecture-notes.md` still has the IQueryable link** for Л4 §3, cut in
   review here.
