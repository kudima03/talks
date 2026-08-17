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
2026), so the preamble guards it:

```latex
\IfFileExists{handstroke.sty}{\usepackage[colored,color=accent]{handstroke}}
                             {\newcommand{\handstroke}[2][]{\underline{#2}}}
```

If the package is missing, the deck still builds and those phrases get a plain
underline instead — which is why both paths are worth checking after an edit.

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

55 slides — 48 content frames plus 7 section dividers, one per chapter of the
script, each divider showing that chapter's budget.

| # | Chapter | Budget |
|---|---|---|
| 1 | Opening — credit, and the divergence | ≈ 1.5 min |
| 2 | The premise — instructions vs. object states, the five rules | ≈ 3.5 min |
| 3 | Why .NET stops you — sealed primitives, the nine interfaces | ≈ 6 min |
| 4 | The constructor is where the program is written | ≈ 5.5 min |
| 5 | Determined hashes | ≈ 5.5 min |
| 6 | Control flow is an object too | ≈ 5 min |
| 7 | What it costs, and what it buys | ≈ 4 min |

Two frames are TikZ diagrams rather than code: the composition tree for
`new Sum<int>(new Difference<int>(…), new Product<int>(…))`, and the
branch-selection machine inside `StringSwitch` — which review asked to be a
diagram, never internal LINQ.

## House rules

Same as the script (see [`../../CLAUDE.md`](../../CLAUDE.md)): snippets are
quoted from the source repos under `kudima03`, trimmed only of the
`GetHashCode` / `ToString` / `GetEnumerator` members every shipped record
carries. `Millennium` and `TotalWithVat` are illustrative types; `DateChoice` is
shown in its intended constructor-delegation form.

When editing:

- Keep `\handstroke` for short phrases only — it typesets its argument in a
  TikZ node, so it cannot break across lines.
- Any frame containing `lstlisting` must be `[fragile]`.
- Statement slides use `\bigline{…}` inside a `[plain]` frame. Don't wrap a
  frame in a macro.
