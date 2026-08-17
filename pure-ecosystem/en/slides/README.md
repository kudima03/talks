# Slides — *A Program Is Nothing But `new`*

Beamer deck for the English script in [`../pure-ecosystem-talk.md`](../pure-ecosystem-talk.md).
The script is the source of truth: the slides carry code, diagrams and short
statements, and every delivery beat lives in `\note{}` next to its frame.

## Build

```sh
make          # pure-ecosystem-slides.pdf
make notes    # same deck + speaker notes on a second screen (right half)
make watch    # rebuild on save
make clean
```

Engine: `pdflatex` (also builds with `lualatex` / `xelatex`). No shell-escape,
no external images, no downloads — everything is drawn with TikZ and typeset
with `listings`.

### Packages

`beamer`, `listings`, `tikz`, `xcolor`, `amssymb`, `lmodern` — all in
`texlive-latex-recommended` + `texlive-pictures`, and all present on Overleaf.

Plus [`handstroke`](https://ctan.org/pkg/handstroke) — Yegor Bugaenko's package
for an underline that looks drawn by hand, used on the deck's key phrases. It is
a fitting choice for a talk that credits him, and it is genuinely new (v0.4.0,
2026), so the theme guards it:

```latex
\newif\ifhavehandstroke
\IfFileExists{handstroke.sty}{\havehandstroketrue}{\havehandstrokefalse}
\ifhavehandstroke
  \RequirePackage[colored,color=accent]{handstroke}
\else
  \newcommand{\handstroke}[2][]{\underline{#2}}
\fi
```

If the package is missing, the deck still builds and those phrases get a plain
underline instead — which is why both paths are worth checking after an edit.
(The branches cannot go directly inside `\IfFileExists`: they are macro
arguments there, so a `#2` in them is an illegal parameter number.)

Debian's TeX Live 2023 does not carry it, and apt-installed TeX Live has no
`tlmgr`. To get the real strokes, extract the `.sty` from CTAN once:

```sh
curl -sfLO https://mirrors.ctan.org/macros/latex/contrib/handstroke.zip
unzip -q handstroke.zip && cd handstroke && tex handstroke.ins   # -> handstroke.sty
```

Then drop `handstroke.sty` next to the `.tex` (it is gitignored), or into
`~/texmf/tex/latex/handstroke/`. On a full TeX Live: `tlmgr install handstroke`.
Its own dependency, `pgfopts`, is already in `texlive-latex-recommended`.

## Structure

55 slides: 18 code frames, 2 diagrams, 3 big-figure slides, 6 dark chapter
transitions, and text slides that hold one thought each.

| # | Chapter | Transition that opens it | Budget |
|---|---|---|---|
| 1 | Opening — credit, and the divergence | *(title)* | ≈ 1.5 min |
| 2 | The premise — instructions vs. object states | *What is a program, actually?* | ≈ 3.5 min |
| 3 | Why .NET stops you — sealed primitives, 9 interfaces | *So why not just write it that way today?* | ≈ 6 min |
| 4 | The constructor is where the program is written | *No methods. So where does the work go?* | ≈ 5.5 min |
| 5 | Determined hashes | *But what is identity?* | ≈ 5.5 min |
| 6 | Control flow is an object too | *And what about `if`?* | ≈ 5 min |
| 7 | What it costs, and what it buys | *So what is it like to live with?* | ≈ 4 min |

The budgets live in this table and in `\note{}`, never on a slide — the audience
should not be reading the speaker's clock.

Two frames are TikZ diagrams rather than code: the composition tree for
`new Sum<int>(new Difference<int>(…), new Product<int>(…))`, and the
branch-selection machine inside `StringSwitch` — which review asked to be a
diagram, never internal LINQ. Three slides are a single figure: **9** interfaces,
**256** bits of identity, **75** packages.

## The theme

`beamerthemepure.sty` (`\usetheme{pure}`) carries the palette, the code style
and the slide furniture, so a second-language deck can reuse it unchanged:

| Command | For |
|---|---|
| `\hi{…}` | inline accent on the **one word** that matters — the workhorse |
| `\handstroke{…}` | the hand-drawn underline, for the one phrase per slide that lands |
| `\banner{…}` | short bold heading on a titleless frame |
| `\bigline{…}` | a statement that owns its slide |
| `\bignum{75}{caption}` | a figure that owns its slide |
| `\thoughtbody{…}` | the question on a chapter transition |
| `\aside{…}` | the quiet supporting line |

Dark slides are a group around the frame, not a wrapper macro — beamer frames
inside macros are fragile:

```latex
{\darkslide
\begin{frame}[plain]
  \thoughtbody{And what about \hi{\texttt{if}}?}
\end{frame}}
```

Slides carry no overlays. One PDF page is one slide, so the footer's number
matches what the audience sees and the deck stays reviewable in a file diff.

## House rules

Same as the script (see [`../../CLAUDE.md`](../../CLAUDE.md)): snippets are
quoted from the source repos under `kudima03`, trimmed only of the
`GetHashCode` / `ToString` / `GetEnumerator` members every shipped record
carries. `Millennium` and `TotalWithVat` are illustrative types; `DateChoice` is
shown in its intended constructor-delegation form.

When editing:

- **One thought per slide.** If a slide needs a paragraph to make sense, the
  paragraph belongs in the script and the slide is wrong. Sentences get cut to
  phrases; explanation goes to `\note{}`.
- Keep `\handstroke` for short phrases only — it typesets its argument in a
  TikZ node, so it cannot break across lines. Roughly one per slide, or it
  stops meaning anything.
- Any frame containing `lstlisting` must be `[fragile]`.
- Never wrap a `frame` in a macro. Dark slides use a group around the frame.
- A trailing smaller line inside `\bigline` / `\thoughtbody` needs its own
  `\par` **inside** the size group, or it inherits the big line's baseline
  skip and the leading looks wrong.
- After editing, look at the pages, not just the log:
  `pdftoppm -r 40 -png … && montage p-*.png -tile 5x6 …` catches bad wraps and
  collisions that produce no warning.
