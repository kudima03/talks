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

## Shape specific to Л5

- **Section 1 is deliberately two passes**, not one list of pairs like
  Л1–Л4's single-pass sections. Pass one (frames 3–12) is pure data
  structures, not a single .NET name on it. Pass two (frames 13–28) is the
  same five structures named in the library. The script's own `###`
  subheadings mark the boundary; the deck has no extra divider frame for it
  — the "разминка" question (frame 13's lead-in) carries the transition, as
  it already does the work of landing on `T[]`.
- **Stack and queue are a deliberate aside**, not a sixth and seventh data
  structure: frames 11–12 state and prove that they are an access
  discipline over any of the five, not a structure of their own. Frame 22
  (`Stack<T>`/`Queue<T>` on an array) closes the loop the microblock opened.
- **The immutable-style recommendation (frame 25) is the author's opinion**,
  stated as one, not a fact about the library like Frozen or Concurrent next
  to it. Keep that framing if the section is edited — it is doing real work
  distinguishing "I recommend" from "here is what exists."
- **One `\vote` frame** (17, array vs linked list for a million-element sum)
  — chosen because it has a clean binary show-of-hands shape and the
  diagram right after it pays off the guess, same pattern as Л4's single
  `var` vote.
- **Five `\keyline` frames**, one per section (29, 34, 39, 43, 46) — every
  section in this script ends on an explicit "Мысль раздела" sentence, so
  all five got a frame rather than the partial coverage Л1–Л3 used.

## Code on the slides — how it was checked

| Frames | How it was verified |
|---|---|
| 15 (`List<T>` capacity table) | Captured by running a loop that adds 20 ints to a fresh `List<int>` and prints `Count`/`Capacity` on dotnet SDK **10.0.112**; the deck shows a trimmed excerpt (1, 4, 5, 8, 9, 16, 17: capacity …) of the real output. |
| 31 (`foreach` desugared) | Both halves compiled and run together (`IEnumerable<int> items = [1, 2, 3];` through the `using`/`MoveNext`/`Current` loop) — output is `1 2 3` twice. |
| 32 (`Evens`/`yield return`) | Compiled and run as written; output order is exactly `got sequence`, `started`, `0 2 4 6 finished`, `started`, `0 2 4 6 finished` (English strings on the slide, Russian in the script's own version — same program, translated literals only). |
| 36 (orders → `top`) | Compiled and run with the four `Order` records from the script; printed `1: 150` and `3: 120`, matching the comment on the slide. |
| 38 (`query` composition) | Compiled and run with `onlyPaid = true`, `minAmount = 100`; printed `3: 120`. |
| 41 (`Where` signatures) | Not run — both are the real `System.Linq.Enumerable.Where` / `System.Linq.Queryable.Where` signatures, simplified to one generic parameter name for the slide, matching the script's own simplification. |
| 45 (`IsBlank` extension) | Compiled and run; `True`, `True`, `True`, including the call on a `null` receiver. |

All five snippets live only in the scratch directory used to check them, not
in the repo — the deck and the script carry the final text.

Listings are ASCII only, including comments: two frames (41, 45) have an
English-language code comment where the script's prose carries the Russian
gloss instead, same rule as every other lecture — `pdflatex` + T2A does not
set Cyrillic inside `lstlisting`.

## Lab — how it was checked

`LAB05-review.md`'s expected output, the double-`querying orders...` demo and
the four compiler diagnostics all come from a minimal reference
implementation (`Shop.Domain` + a `Program.cs` standing in for `Shell`),
built and run on dotnet SDK **10.0.112** in a scratch directory, not
committed to the repo. Three orders — two paid for Ann (150 + 150), one
shipped for Bob (120) — produce the exact `report`/`top 2` session quoted in
the review file; `top 2` genuinely returns one row on that data, because Bob
has no paid order, and that is the correct result, not a bug in the
reference solution.

The four diagnostics in the review file's table (`CS1105`, `CS0021`,
`CS0029`, `CS8604`) were each triggered against the reference solution and
copied verbatim, including the exact type names GroupBy/Select produce —
re-check them if `OrderExtensions`' signatures change.

## Facts this lecture commits to

- Section timings as labelled in the script: **30 / 15 / 22 / 10 / 8** = 85
  minutes, not 80. This is a known gap, not an oversight — see Open items.
- **46 slides**, numbered 1–46 in order: §1 2–29, §2 30–34, §3 35–39,
  §4 40–43, §5 44–46.
- §1's two subsections inside the script (`### Структуры данных без
  привязки к языку`, `### Те же структуры в .NET`) are not section breaks
  for numbering purposes — both live inside `## 1`, one `\section`, one
  entry in `README.md`.
- PriorityQueue was cut from the .NET pass in PR review (`#17`) and is not
  on any frame; do not re-add it without checking that review thread first.
- The hash-table diagram (frame 8) draws one collision box, not a two-deep
  chain — a second box pushed the frame into the footer at build time; keep
  it at one.

## Open items

The author's call. Do not quietly "fix" these:

1. **Timings do not sum to 80.** Section 1 grew to 30 minutes when it was
   split into two passes (PR review, same thread as above); sections 2–5
   were not re-budgeted to compensate. Either trim §1 or accept 85 as the
   real number.
2. **The Latin Modern fallback has not been checked** for this deck; it
   builds with PT Sans.
