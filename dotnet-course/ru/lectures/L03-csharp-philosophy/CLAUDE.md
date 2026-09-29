# Л3. C#: философия языка и система типов — house rules

Working notes for this lecture's directory. Course-wide rules — the palette,
the two-colour system, what may go on a slide, the build, the slide-cue
convention — live in [`../../../CLAUDE.md`](../../../CLAUDE.md). Read that
first; this file carries only what is specific to Л3.

## What is in here

| File | What it is |
|---|---|
| `L03-csharp-philosophy.md` | The spoken script. **Source of truth.** |
| `README.md` | Theses — a compressed index of the script, with the frame each thesis lives on. Used as a crib sheet at the lectern. |
| `slides/L03-csharp-philosophy-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |
| `lab/LAB03-task.md` | The lab, as handed to the student. |
| `lab/LAB03-review.md` | The same lab for whoever accepts it. |

The five move together. As in Л2, every thesis in `README.md` carries a frame
number and `[—]` marks a thesis with no frame. **Adding, cutting or reordering
a frame invalidates those numbers** — renumber the cues in the script, then
re-derive the tags in `README.md`, in the same commit. The lab asks for nothing
the lecture does not cover, and `LAB03-task.md` §7 depends on `Max<T>` staying
in the deck (frame 29).

## Code on the slides — how it was checked

Nothing here is a benchmark. §1 was **deliberately rebuilt without numbers**:
the old BenchmarkDotNet guessing game and the `benchmarks/` folder were removed,
and no timing, ratio or byte count may come back onto a slide or into the
script. The same goes for §5: boxing is argued by picture (one heap object per
element vs one array), not by megabytes.

What *is* checked is that the code is honest:

| Frames | How it was verified |
|---|---|
| 6–9 | Each imperative/declarative pair compiled on .NET 10 and run against the same random data; both sides return the same result. Re-run after any edit to either side. |
| 15 | Real terminal output of a stock `dotnet new console` named `HelloWorld` with the two-line `args` program, dotnet SDK **10.0.110**: `dotnet run -- Ann`, then `dotnet run; echo $?` showing exit code **0**. |
| 20 | «`==` не определён» for a plain struct is compiler error **CS0019**; class `Equals` → False, record `==` → True, record struct `==` → True — all checked. |
| lab extra task | The C-style fragment in `LAB03-task.md` was checked against `Where`/`Select`/`Distinct`/`Order` with `SequenceEqual`. |

Listings are ASCII only. The left side of frame 7 is set at 6.2pt, because 20
lines have to fit next to the right column. The room has already seen the same
code at full size on frame 6, which is why that is acceptable. Do not add lines
to it.

## Facts this lecture commits to

- Section timings: **35 / 5 / 20 / 8 / 12** = 80 minutes.
- **30 slides**, numbered 1–30 in order.
- The sample project is `HelloWorld`, as in Л1 and Л2.
- §1 comparisons are labelled **«императивно»** (alt, left) and
  **«декларативно»** (accent, right). Accent marks the thing to do, so it is
  on the right side here.
- Three pairs, not four: filter/sort/project, grouping, strings. A search pair
  (`Any`/`MaxBy`) was cut.
- The entities slide (frame 17) uses the **English keywords** for all seven
  entities; descriptions are Russian.
- The word is **boxing** (and **unboxing**) — never «коробка». The Russian verb
  «упаковывается» survives in a few places in the script, and that is fine.
- It is written **«Top-level statements»**, not «инструкции верхнего уровня».
- One `\statement` (frame 5), no `\shout`. Four `\keyline`s (frames 13, 21, 26,
  30); §2 closes on content.

## Cut in review — not to be restored without asking

- **The BenchmarkDotNet game** (three rounds, tables, «как читать таблицу»)
  and the whole `benchmarks/` folder.
- **«Рантайм оптимизирует за вас»** (inline, bounds-check elimination, PGO,
  stack allocation) with its foreach→while story.
- **The search pair** in §1.
- **«Многое взято из Java, свойства — из Delphi»**, from the slide, the note
  and the script alike.
- **Main vs top-level side by side, async Main, top-level file rules** (§2).
  §2 only recalls them.
- **The struct traps slide** (list indexer copy, array element, pass by value)
  and the `ref`/`in`/`readonly struct` paragraph with it.
- **The six-step ladder slide**, replaced by the entities table.
- **The equality listing and the hash-contract diagram**, replaced by one
  table: class / struct / record.
- **Generics mechanics**: no erasure vs specialisation, no code-size cost, no
  generic math. Also dropped in review: the `ArrayList` vs `List<int>` listing
  slide and the `SimpleList<T>` slide. §5 is the rationale opener, boxing and
  `Max<T>`, and nothing more.
- **The «по умолчанию» column** on «Кому видно» (frame 23). The defaults stay
  in the voice.

## Open items

The author's call. Do not quietly "fix" these:

1. **§1 is budgeted at 35 minutes but now has less material.** The runtime
   slide and a pair were cut, but the timing was left as it was. If §1
   finishes early in practice, the slack is there.
2. **Frame 11 «C# в трёх словах» is sparse** now that its aside is gone. It
   is left that way on purpose rather than padded.
3. **The Latin Modern fallback** has not been checked for this deck. The deck
   builds with PT Sans; see Л2's note on the fallback failing on the author's
   machine.
