# Slides — *A Program Is Nothing But `new`*

Beamer deck for the English script in [`../pure-ecosystem-talk.md`](../pure-ecosystem-talk.md).
The script is the source of truth: the slides carry code, diagrams and short
claims, and every delivery beat lives in `\note{}` next to its frame.

## Build

```sh
make          # pure-ecosystem-slides.pdf
make notes    # same deck + speaker notes on a second screen (right half)
make watch    # rebuild on save
make clean
```

Engine: `pdflatex`; `lualatex` builds it too (both verified). No shell-escape,
no external images, no downloads, no packages outside a standard TeX Live —
everything is drawn with TikZ and typeset with `listings`.

### Packages

`beamer`, `listings`, `tikz`, `xcolor`, `amssymb`, `lmodern` — all in
`texlive-latex-recommended` + `texlive-pictures`, and all present on Overleaf.
Nothing else. If a build needs a package that has to be fetched from CTAN by
hand, that is a reason to drop the effect, not to add the package.

## The register

The deck is deliberately plain. An earlier version of it was styled like a
product keynote — full-bleed dark slides carrying only a rhetorical question,
54pt figures, hand-drawn highlighter underlines, and one-line slides that said
how good the idea was rather than what it was. It read as an advertisement.

The rules that replaced it:

- **Every content slide has a title.** If a slide cannot be titled, it is not a
  slide — it is a sentence from the script that escaped.
- **A slide states a claim and shows the evidence for it**, on the same slide:
  the claim in `\claim{}`, the evidence as prose, a snippet or a diagram, and
  the qualification in `\aside{}`.
- **One thought per slide** still holds. Strict does not mean dense: a slide
  that needs a paragraph belongs in the script.
- **No slogans.** A slide with a single phrase on it is fine when the phrase is
  the point ("A program should *be* the transformation"). It is not fine when
  the phrase is only an invitation to be impressed.
- **The accent lands once per slide**, inline, on the word the claim turns on.
  It is not a highlighter.
- **Numbers go inside the sentence that gives them meaning** — "all 256 bits
  decide equality; the `int` is a bucket index" — never alone on a page.
- Nothing is bigger than it has to be. The talk title is the largest text in
  the deck, then the chapter titles, and nothing else competes with them.

## Structure

53 slides: 1 title, 7 chapter dividers, 16 code frames, 2 diagrams, 25 text
slides, a closing statement and a thank-you.

| # | Chapter | Divider line | Budget |
|---|---|---|---|
| 1 | Where this comes from | Elegant Objects, its author, and the places where Pure does not follow it | ≈ 1.5 min |
| 2 | What a program is | Instructions that have to run, against object states that already exist | ≈ 3.5 min |
| 3 | Why .NET stops you | Sealed primitives, no seam in the BCL, and the nine interfaces that replace them | ≈ 6 min |
| 4 | Where the work goes | No methods, so the transformation is a chain of constructor delegation | ≈ 5.5 min |
| 5 | Identity | Why `GetHashCode` cannot be identity, what replaces it, and what that costs | ≈ 5.5 min |
| 6 | Control flow | `if` and `switch` as objects that implement the interface they return | ≈ 5 min |
| 7 | What it costs, and what it buys | Testing, the adapters at the border, and how the packages are shipped | ≈ 4 min |

The budgets live in this table and in `\note{}`, never on a slide — the
audience should not be reading the speaker's clock. Chapter dividers are plain
slides: number, title, rule, and one factual line about what is coming. The
running footer carries the chapter name and the slide number; the speaker's
handle appears on the title and closing slides only.

Two frames are TikZ diagrams rather than code: the composition tree for
`new Sum<int>(new Difference<int>(…), new Product<int>(…))`, and the
branch-selection machine inside `StringSwitch` — which review asked to be a
diagram, never internal LINQ.

## The theme

`beamerthemepure.sty` (`\usetheme{pure}`) carries the palette, the code style
and the slide furniture, so a second-language deck can reuse it unchanged:

| Command | For |
|---|---|
| `\claim{…}` | the claim the slide makes, directly under its title |
| `\hi{…}` | inline accent on the **one word** the claim turns on |
| `\aside{…}` | the quiet supporting line: evidence, caveat, consequence |
| `\eyebrow{…}` | small uppercase label above a column or a block |
| `\chapterpage{n}{title}{line}` | the chapter divider, on a `[plain]` frame |
| `\hrulethin{0.4}` | a thin rule, as a fraction of `\linewidth` |
| `\slidetop` `\slidegap` `\rulegap` | the deck's only three vertical gaps |

The palette is low-chroma on purpose: ink `#1F2328` on paper `#FBFAF7`, one
accent (`#7C3016`, a dark brick that sits next to the ink rather than jumping
off the wall), and desaturated keyword/string colours in the code style.

Slides carry no overlays. One PDF page is one slide, so the footer's number
matches what the audience sees and the deck stays reviewable in a file diff.

## House rules

Same as the script (see [`../../CLAUDE.md`](../../CLAUDE.md)): snippets are
quoted from the source repos under `kudima03`, trimmed only of the
`GetHashCode` / `ToString` / `GetEnumerator` members every shipped record
carries. `Millennium` and `TotalWithVat` are illustrative types; `DateChoice`
is shown in its intended constructor-delegation form.

When editing:

- The class option is `t`: frames are top-aligned, so every slide starts its
  content at the same height. Empty space at the bottom of a short slide is
  the design, not a gap to fill.
- Vertical space comes from `\slidetop`, `\slidegap` and `\rulegap`, never
  from a literal `\vspace`. The exception is a code frame, where the gaps
  around the listing are tuned to make it fit — that is the only place a raw
  `\vspace` belongs.
- Any frame containing `lstlisting` must be `[fragile]`.
- Never wrap a `frame` in a macro — beamer frames inside macros are fragile.
- A code frame that overflows gets its `basicstyle` dropped a size
  (`\scriptsize`, then `\tiny`) or its `\aside{}` shortened. Do not reflow the
  snippet: it is quoted.
- After editing, look at the pages, not just the log:
  `pdftoppm -r 45 -png … && montage p-*.png -tile 5x …` catches bad wraps and
  collisions that produce no warning. `Overfull \vbox` in the log means a slide
  is running into the footer.
