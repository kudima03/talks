# Л1. Платформа .NET и dotnet CLI — house rules

Working notes for this lecture's directory. Course-wide rules — the palette,
the two-colour system, slide structure, what may go on a slide, the build —
live in [`../../../CLAUDE.md`](../../../CLAUDE.md). Read that first; this file
carries only what is specific to Л1.

## What is in here

| File | What it is |
|---|---|
| `L01-platform.md` | The spoken script. **Source of truth.** |
| `README.md` | Theses — a compressed index of the script, section by section. |
| `slides/L01-platform-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |

The three move together. A change to the script that alters a claim, a number
or a section's shape must be reflected in the other two in the same commit —
a README that describes a paragraph no longer in the script is worse than no
README.

## The script is the source of truth

Slides carry code, diagrams and short claims. Delivery lives in `\note{}`.
Nothing goes on a slide that the script does not say, and no slide invents a
claim of its own — if a slide needs a new fact, put the fact in the script
first.

Markers in the script and what they mean:

- `[СЛАЙД n — …]` — one frame in the deck. The cue is the spec for that frame.
- `[ВОПРОС В ЗАЛ]` — a show of hands. On a slide it is `\vote{}`: the
  question alone, so nobody reads ahead to the answer.
- `[ПАУЗА]` — a beat in delivery. **Not** a slide.
- `[ЛАБА 1 — …]` — instructions for whoever writes the lab. Not spoken, not
  a slide.

## Building the deck

```sh
cd slides
make          # L01-platform-slides.pdf
make notes    # same deck + speaker notes on a second screen (right half)
make watch    # rebuild on save
make clean
```

Engine is **pdflatex**. The theme is shared across all fifteen lectures and
lives in `../../../../theme/` — see `theme/README.md` there before changing
anything in it, because a change there lands on every lecture.

System requirements (Debian):

```sh
sudo apt install texlive-lang-cyrillic texlive-fonts-extra
```

Without `texlive-lang-cyrillic` nothing Russian compiles at all. Without
`texlive-fonts-extra` the deck still builds but falls back from PT Sans to
Latin Modern — so **both paths are worth a look after an edit**.

## Slide rules

Inherited from `beamerthemepure` and deliberately identical in spirit:

- **Every content slide has a title.** If it cannot be titled, it is not a
  slide — it is a sentence from the script that escaped.
- **A slide states a claim and shows the evidence for it**, on the same
  slide: `\claim{}`, then the evidence as prose, a snippet or a diagram, then
  the qualification in `\aside{}`.
- **One thought per slide.** Strict is not the same as dense: a slide that
  needs a paragraph belongs in the script.
- **The accent lands once per slide**, inline, on the word the claim turns
  on. `\hi{}` is not a highlighter.
- **No slogans.** `\statement{}` is for a phrase that *is* the content —
  «Пять файлов, два этапа компиляции, ноль магии». Two or three per deck,
  never as an invitation to be impressed.
- Nothing is bigger than it has to be. The lecture title is the largest text
  in the deck, then the section titles, and nothing else competes.
- Slides carry no overlays. One PDF page is one slide, so the footer's number
  matches what the audience sees and the deck stays reviewable in a diff.

## Editing the `.tex`

- The class option is `t`: frames are top-aligned. Empty space at the bottom
  of a short slide is the design, not a gap to fill.
- Vertical space comes from `\slidetop`, `\slidegap`, `\rulegap` — never a
  literal `\vspace`. The exception is a code frame, where the gaps around the
  listing are tuned to make it fit.
- Any frame containing `lstlisting` must be `[fragile]`.
- Never wrap a `frame` in a macro — beamer frames inside macros are fragile.
- **Listings are ASCII only.** pdflatex + T2A will not set Cyrillic inside a
  listing without a `literate` table nobody wants to maintain. Every snippet
  in this course is a command, a csproj, a json file, IL or C# — ASCII
  already. Russian belongs in the prose around the listing.
- A code frame that overflows gets its style dropped a size
  (`style=term` is already `\scriptsize`) or its `\aside{}` shortened. Do not
  reflow a snippet that is quoted output.
- After editing, look at the pages, not just the log. `Overfull \vbox` in the
  log means a slide is running into the footer, but **a listing that is too
  wide overflows its own grey ground silently** — no warning, clean log,
  broken slide. The only way to catch it is to look at the render.

## Facts this lecture commits to

- Section timings: **9 / 13 / 20 / 15 / 18 / 5** = 80 minutes.
- 37 slides, one per `[СЛАЙД n — …]` cue in the script, numbered 1–37 in order.
- Cut in review and **not to be restored without asking**: the benchmark
  thesis and its slide (§2), the dedicated `runtimeconfig.json` walkthrough
  and JSON listing (§5), the §1 and §3 closing keylines, the «три строки»
  opener, `add package` on the lifecycle slide.
- `runtimeconfig.json` survives as one row of the bin table and one sentence
  in §5 — it is the fifth file, and the count is what «пять файлов, два этапа
  компиляции, ноль магии» rests on.

## Open items

Raised in review, not yet resolved in the script — do not quietly "fix" these
without asking, they are the author's call:

1. **Timing.** Still long: roughly 63 min of pure talking at 125 wpm, before
   6 shows of hands and the interruptions the lecture explicitly invites.
   Candidates for the next cut: CLS (the script itself says it almost never
   comes up in practice) and the three-participants recap in §2.
2. **Three claims flagged as overstated**, which .NET developers in the room
   are most likely to challenge — the deck marks the first one with a
   "здесь я упрощаю" note:
   - «JIT знает про эту машину всё» — RyuJIT uses ISA detection, it does not
     auto-tune; C++ has `-march=native`.
   - «Одна dll работает везде» — true via `dotnet app.dll`; the apphost next
     to it is native, which the script itself says 350 lines later.
   - «Версия здесь обязательна» — `PackageReference` without `Version` is
     legal, and is the norm under Central Package Management.
3. **`dotnet publish` is not the only command defaulting to Release** —
   `dotnet pack` does too, since .NET 8. The script still says "единственная".
