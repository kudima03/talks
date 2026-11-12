# Л5. Коллекции, IEnumerable и LINQ — house rules

Working notes for this lecture's directory. Course-wide rules — the palette,
the two-colour system, what may go on a slide, the build, the slide-cue
convention — live in [`../../../CLAUDE.md`](../../../CLAUDE.md). Read that
first; this file carries only what is specific to Л5.

## What is in here

| File | What it is |
|---|---|
| `L05-collections-and-linq.md` | The spoken script. **Source of truth.** |
| `README.md` | Theses — a summary and a crib sheet for the lectern, with the frame each thesis lives on. |
| `slides/L05-collections-and-linq-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |
| `lab/LAB05-task.md` | The lab, as handed to the student. |
| `lab/LAB05-review.md` | The same lab for whoever accepts it. |

The five move together. Every thesis in `README.md` carries a frame number
and `[—]` marks a thesis with no frame. **Adding, cutting or reordering a
frame invalidates those numbers** — renumber the cues in the script, then
re-derive the tags in `README.md`, in the same commit. The lab continues
`Shop.Domain`/`HelloWorld`/`Shell` from Л3–Л4's labs and asks for nothing the
lecture does not cover.

## `yield return` is not taught in this lecture

Deliberate cut, not an oversight. The script never uses the words `yield
return`, and no frame shows a custom iterator. Deferred execution is
demonstrated instead with a plain LINQ `Where` whose predicate increments a
counter (frame 27, "Построили запрос — ничего не выполнили") — the same
lesson (build vs. traverse, re-traversal without materialization), carried
entirely by a stock operator. This also reaches into the lab: `LAB05-task.md`
step 5 and the optional paging task were rewritten to use a counter captured
in a one-element array through `Select`, not a hand-written iterator — do not
reintroduce `yield` into either the script or the lab without deciding to
teach it first.

## Shape specific to Л5

- **Section 1 is two passes**, not one list of pairs like Л1–Л4's
  single-pass sections. Pass one (frames 3–11) is pure data structures, no
  .NET name on it. Pass two (frames 12–25) is the same five structures named
  in the library. The script's own `###` subheadings mark the boundary; the
  deck has no extra divider frame for it.
- **Frame 16, "По строкам или по столбцам?", is a question frame with its
  own diagram**, not the bare `\vote` macro: the 2D-array grid with both
  traversal arrows is the evidence the room looks at while guessing, and the
  answer (DRAM row buffer, not just cache lines) is given verbally, never
  printed on the slide. Keep the slide silent on which arrow wins — both
  directions are coloured in the claim, neither is singled out. It sits
  *after* the List/LinkedList million-element pair (frames 14–15), not in
  the `T[]` part, because its spoken answer leans on that comparison.
- **Stack and queue are a deliberate aside**, not a sixth and seventh data
  structure: frames 10–11 state and prove that they are an access discipline
  over any of the five, not a structure of their own. Frame 20
  (`Stack<T>`/`Queue<T>` on an array) closes the loop the microblock opened.
- **The immutable-style recommendation (frame 23) is the author's opinion**,
  stated as one, not a fact about the library like Frozen next to it. Keep
  that framing if the section is edited.
- **Section 3 gives every named LINQ method its own frame** (32–63, one
  method each: `Where` through `ToHashSet`), each with one explicitly-typed
  variable — never `var` — holding the call. There is no five-families
  table: it duplicated the section opener (frame 30) and the per-method
  frames and was cut. `OfType` was cut on request and is not mentioned
  anywhere in the lecture. Only methods named by identifier in the family-rundown prose got
  a frame; the "три мелочи" aside methods (`Contains`, etc.) did not, and
  neither did `Chunk`/the indexed overloads, which the prose only describes.
  Adding a method to the rundown prose means adding its frame here too, in
  family order.
- **One `\vote` frame proper** (14, array vs linked list for a
  million-element sum) — the other "question frame" (16) is not a `\vote`
  because it carries a diagram.
- **Four `\keyline` frames** (29, 65, 69, 72), for §2–§5. Every section in
  the script ends on an explicit "Мысль раздела" sentence; §1's is spoken
  only — its keyline frame was cut on request.

## Code on the slides — how it was checked

| Frames | How it was verified |
|---|---|
| 12 (`List<T>` capacity table) | Captured by running a loop that adds 20 ints to a fresh `List<int>` and prints `Count`/`Capacity` on dotnet SDK **10.0.112**; the deck shows a trimmed excerpt of the real output. |
| 27 (`Where` with a counter) | Compiled and run as written (`Enumerable.Range(0, 7).Where(...)` incrementing `calls`); printed exactly `calls so far: 0`, `0 2 4 6`, `calls after first pass: 7`, `0 2 4 6`, `calls after second pass: 14`. |
| 31 (orders → `top`) | Compiled and run with the four `Order` records from the script; printed `1: 150` and `3: 120`, matching the comment on the slide. |
| 32–63 (one frame per LINQ method) | All 32 snippets compiled and run together in one `Program.cs` against a shared `orders`/`customers` fixture (`List<Order>`, `List<Customer>`); every call returned the expected value and none threw (in particular `ToDictionary(o => o.Amount)` did not hit a duplicate-key exception, because the fixture's amounts are distinct on purpose — keep them distinct if the fixture changes). |
| 64 (`query` composition) | Compiled and run with `onlyPaid = true`, `minAmount = 100`; printed `3: 120`. |
| 67 (`Where` signatures) | Not run — both are the real `System.Linq.Enumerable.Where` / `System.Linq.Queryable.Where` signatures, simplified to one generic parameter name for the slide, matching the script's own simplification. |
| 71 (`IsBlank` extension) | Compiled and run; `True`, `True`, `True`, including the call on a `null` receiver. |

All snippets live only in the scratch directory used to check them, not in
the repo — the deck and the script carry the final text.

Listings are ASCII only, including comments: frames 67 and 71 have an
English-language code comment where the script's prose carries the Russian
gloss instead, same rule as every other lecture — `pdflatex` + T2A does not
set Cyrillic inside `lstlisting`. The 32 per-method frames in §3 keep their
single code line under roughly 75 characters at `\small`; five of them
(`FirstOrDefault`, `GroupBy`, `ToLookup`, `Zip`, `ThenBy`) needed
`\fontsize{8.2}{9.6}` instead because the line ran past the listing's right
edge at `\small` — check new one-liners against the built PDF, not just the
`.tex`, before assuming `\small` fits.

## Lab — how it was checked

`LAB05-review.md`'s expected `report`/`top 2` session, the `accessCount`
numbers and the four compiler diagnostics all come from a minimal reference
implementation (`Shop.Domain` + a `Program.cs` standing in for `Shell`),
built and run on dotnet SDK **10.0.112** in a scratch directory, not
committed to the repo. Three orders — two paid for Ann (150 + 150), one
shipped for Bob (120) — produce the exact `report`/`top 2` session quoted in
the review file; `top 2` genuinely returns one row on that data, because Bob
has no paid order, and that is the correct result, not a bug in the
reference solution. The same three orders feed the deferred-execution demo:
`accessCount` is 6 before materializing (two passes × three orders) and 3
after — re-derive both numbers from the actual order count if the fixture
changes, they are not independent of it.

The four diagnostics in the review file's table (`CS1105`, `CS0021`,
`CS0029`, `CS8604`) were each triggered against the reference solution and
copied verbatim, including the exact type names GroupBy/Select produce —
re-check them if `OrderExtensions`' signatures change.

## Facts this lecture commits to

- Section timings as labelled in the script: **30 / 15 / 22 / 10 / 8** = 85
  minutes, not 80. This is a known gap, not an oversight — see Open items.
- **72 slides**, numbered 1–72 in order: §1 2–25, §2 26–29, §3 30–65,
  §4 66–69, §5 70–72.
- §1's two subsections inside the script (`### Структуры данных без
  привязки к языку`, `### Те же структуры в .NET`) are not section breaks
  for numbering purposes — both live inside `## 1`, one `\section`, one
  entry in `README.md`.
- PriorityQueue was cut from the .NET pass in PR review (`#17`) and is not
  on any frame; do not re-add it without checking that review thread first.
- Frame "Массив → T[]" (the `newarr`/`ldlen`/`ldelem`/`stelem` table) and
  frame "ImmutableList<T> vs ImmutableArray<T>" were both cut on request —
  the script's prose about IL instructions and about the two immutable
  collections still exists and is still spoken, it simply has no visual
  support now. Don't "restore" a slide for them without asking first.
- Also cut on request, with the prose kept and spoken: "Массив vs
  динамический массив" (duplicated the five-structures table), §1's
  keyline, and "`foreach`: вы пишете / компилятор делает" (the desugaring
  is now described by voice only). "Пять семейств операторов" and the
  `OfType` frame were cut together with their prose.
- The hash-table diagrams (frames 7 and 17) draw one collision box, not a
  two-deep chain — a second box pushed the pure-CS one into the footer at
  build time; keep both at one box.

## Open items

The author's call. Do not quietly "fix" these:

1. **Timings do not sum to 80.** Section 1 grew to 30 minutes when it was
   split into two passes; sections 2–5 were not re-budgeted to compensate.
   Section 3 in particular now has 36 frames for its stated 22 minutes —
   averaging under 35 seconds a frame across many of them, since the 32
   one-method slides are meant to flash by quickly as a reference run rather
   than each get full discussion time. Either trim §1/§3 or accept that the
   real total is well past 80.
2. **The Latin Modern fallback has not been checked** for this deck; it
   builds with PT Sans.
