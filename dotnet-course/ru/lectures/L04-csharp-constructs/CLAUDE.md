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
  mechanics. Do not thin them out to match Л3's density. The cuts the author
  did make (see «Cut in review») are his call, not a licence to cut more.
- **No `\note{}`.** The deck carries no speaker notes and no `\shownotes`
  block; the delivery lives in the script only. (`make notes` still exists
  because the Makefile is shared; it builds the same deck.)

## Code on the slides — how it was checked

| Frames | How it was verified |
|---|---|
| 4–9, 14, 15, 17, 20, 21, 22, 24, 28, 29, 31, 34, 36–39, 43, 46 | Compiled and run on dotnet SDK **10.0.112** in a stock `dotnet new console`, with the minimal types around them (`Order`, `User`, `Point`); outputs in comments (`// False`, `// 2..9`) are what it printed. |
| 9 | Both columns were also run side by side for scores 100, 95, 90, 80, 75, 60, 59, 0: the `if` ladder and the switch expression give the same grade every time. |
| 12, 13, 45 | Fragments: `Process`, `row`, `Invoice`, `file` are not defined anywhere. The constructs themselves (`var`, `new()` into a field and a parameter, catch order, `throw;`) were compiled separately. |
| 37 | `NameLength` gives `CS8602`, as the comment says. |
| 45 | `throw e;` gives `CA2200`; a general `catch` above a specific one is error `CS0160`. |
| 48 | Real output of `PORT=abc dotnet run` on a `HelloWorld` whose `Program.cs` is the frame-46 listing plus two lines on top. Captured in a scratch directory, path rewritten to `/home/user/HelloWorld`; line numbers 1, 8, 16 match that file. Re-capture rather than edit. |

Listings are ASCII only, including string literals: the script's listings use
the same English strings as the deck. Two frames break the course's Allman
style on purpose: the ladder (5) and early return (6) use `} else {` so that
two columns fit at a readable size. Frame 8 is set at 6.4pt, the smallest in
the deck; it shows method bodies without the method around them for the same
reason.

## Facts this lecture commits to

- Section timings: **15 / 20 / 15 / 12 / 18** = 80 minutes.
- **49 slides**, numbered 1–49 in order: §1 2–18, §2 19–25, §3 26–32,
  §4 33–41, §5 42–49.
- §1 is called **«Управляющие конструкции»**, without «и сахар», in the
  script, the deck, the README and the course documents. Its opener has no
  subtitle line.
- The order in §1 is the ladder **if → тернарный → switch → switch-выражение**,
  then values: **var → new() → коллекции → индексы и диапазоны**. The
  switch expression is called «аналог тернарного в семье switch» (frame 10).
- The grade ladder (`score >= 90 → "A"`) is one thread through frames 5, 9 and
  §2's relational pattern: keep the numbers in step if one of them changes.
  Frame 9 is built for beginners: one arm taken apart (pattern, `when`,
  result), then the `if` ladder and the switch expression line for line.
- `??`, `??=` and `?.` live in §4, not §1.
- The pattern ladder has **six steps**: константа, тип, свойство, отношение и
  логика, позиция, список.
- One `\statement` (frame 41, «каждый ! — это долг»), no `\shout`. Four
  `\keyline`s (frames 18, 25, 32, 49); §4 closes on the statement. §2's keyline
  is about patterns only — the switch-or-polymorphism half went with its frame
  and its text.
- One `\vote` frame: 11, «Что такое var?», asked for in review. The other
  questions to the room have no frame, as in Л3.

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
- **Four §2 frames and their text**: the list-pattern command parser, the
  pattern-ladder table and the «одиннадцать ветвей» summary, order and
  exhaustiveness with the «убрали `_`» question, and the whole
  switch-or-polymorphism block. Gone from the script, the README and the course
  documents (`course-structure.md`, `lecture-notes.md`) alike. The script now
  points `catch` order back at §1's switch, and the property-pattern paragraph
  forward to `?.` in §4.
- **Lab 4 was adjusted to match**: the task shows the `.. var name` tail itself,
  and the two questions that needed the cut text (switch or a virtual method;
  why `Command` is not exhaustive) are replaced by one on positional patterns.
- **«и сахар»** in §1's name and the opener's subtitle; **«if (count) не
  скомпилируется, в отличие от C»** on frame 4 and in the script.

## Open items

The author's call. Do not quietly "fix" these:

1. **The Latin Modern fallback** has not been checked for this deck; it builds
   with PT Sans.
2. **Timings were not re-budgeted** after the review added §1 material (if,
   ladder, early return, `when`, `var`) and cut parts of §2 and §3. §2 is
   still budgeted at 20 minutes with seven content frames.
3. **`ru/lecture-notes.md` still has the IQueryable link** for Л4 §3, cut in
   review here.
