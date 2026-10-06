# Slide theme — .NET course

One beamer theme, shared by all fifteen lectures. A lecture's `slides/`
folder holds its `.tex` and its own `Makefile`, so lecture 12 looks like
lecture 1 without anyone remembering to make it so.

```
dotnet-course/
  theme/
    beamerthemedotnet.sty     ← palette, furniture, code styles, language
  ru/lectures/L01-platform/
    slides/
      L01-platform-slides.tex
      Makefile
```

## Starting a new lecture

Copy the previous lecture's `Makefile` and change `DECK`:

```sh
mkdir -p ru/lectures/L02-runtime/slides
sed 's/L01-platform/L02-runtime/' \
  ru/lectures/L01-platform/slides/Makefile \
  > ru/lectures/L02-runtime/slides/Makefile
```

Then in the `.tex`:

```latex
\documentclass[aspectratio=169,10pt,t]{beamer}
\usetheme{dotnet}
\usetikzlibrary{arrows.meta,positioning,calc}
\dotnetlecture{Л2}          % the tag in the footer
```

`\usetheme{dotnet}` brings the language setup with it — `inputenc`,
`fontenc[T1,T2A]`, `babel[russian]`, the fonts. A deck never loads those
itself; that is the whole point of the file.

## Build

```sh
make          # the deck
make notes    # deck + speaker notes on a second screen (right half)
make watch    # rebuild on save
make clean
```

`make` treats the theme as a prerequisite, so editing `beamerthemedotnet.sty`
rebuilds the PDF. If you add another shared input, add it to `$(STY)` — a
stale PDF that silently ignores your change costs more time than it saves.

## Requirements

```sh
sudo apt install texlive-lang-cyrillic texlive-fonts-extra
```

Plus `texlive-latex-recommended` and `texlive-pictures` for beamer, listings,
tikz and microtype. No shell-escape, no external images, no downloads at build
time — everything is drawn with TikZ and typeset with `listings`.

`texlive-lang-cyrillic` is **not optional**: without `t2aenc.def` and
`babel-russian` nothing Russian compiles. `texlive-fonts-extra` is: the theme
guards PT Sans and falls back to Latin Modern when it is missing, so a machine
without it gets the deck in the wrong face rather than an error — which is why
**both paths are worth a look after an edit**:

```latex
\IfFileExists{paratype.sty}{\haveparatypetrue}{\haveparatypefalse}
```

### Why pdflatex and not lualatex

The obvious route was LuaLaTeX + `fontspec` + Fira Sans OTF, which would have
made these decks typographic twins of the Pure deck — Fira covers Cyrillic and
is already installed in `~/texmf`. It was rejected because the machine has no
`luaotfload`, so `fontspec` does not run at all, and the fix is a second system
package with a font cache to keep warm. pdflatex + T2A + PT Sans needs one
package, is what Overleaf handles without configuration, and PT Sans was
commissioned for exactly this job. The two decks are siblings, not twins, and
that is the intended outcome.

## The register

Deliberately plain, and copied in spirit from `beamerthemepure` — including
the reason that theme exists. An earlier version of the Pure deck was styled
like a product keynote: full-bleed dark slides carrying a rhetorical question,
54pt figures, highlighter underlines. It read as an advertisement. The rules
that replaced it apply here unchanged:

- **Every content slide has a title.** If it cannot be titled, it is not a
  slide — it is a sentence from the script that escaped.
- **A slide states a claim and shows the evidence for it**, on the same slide.
- **One thought per slide.** Strict is not dense.
- **No slogans.** A single phrase earns a page only when the phrase *is* the
  point.
- **The accent lands once per slide**, inline, on the word the claim turns on.
- Nothing is bigger than it has to be.

Plain is not bare. What the theme spends its detail on — each of which had to
pass *would anyone notice it if they were not looking for it?*:

- **PT Sans and PT Mono**, with `microtype` doing protrusion and expansion
  behind them, and real letterspacing on the `\eyebrow` labels.
- **A ground under every listing** (`codebg`, one step off the paper), so a
  snippet reads as one object rather than text scattered on the slide.
- **The slide number set against the total** (`16 / 37`, the total lighter),
  and a 0.5pt hairline along the bottom edge that fills as the lecture runs.
- **The lecture tag in the footer** (`Л1`), so a photographed slide can still
  be placed in the course.

## Commands

| Command | For |
|---|---|
| `\claim{…}` | the claim the slide makes, directly under its title |
| `\hi{…}` `\lo{…}` | inline accent, primary (teal) and second (brick) |
| `\aside{…}` | the quiet supporting line: evidence, caveat, consequence |
| `\eyebrow{…}` | small uppercase label above a column or a block |
| `\colhead{colour}{…}` | coloured, letterspaced column head |
| `cbul` env | bullet list with coloured markers: `\begin{cbul}{accent}` |
| `\versus{h}{b}{h}{b}` | two-column comparison, left teal / right brick |
| `\vote{…}` | a question put to the room, alone on the slide (`[ВОПРОС В ЗАЛ]`) |
| `\statement{…}` | a phrase that *is* the content, set large |
| `\shout{…}` | one word, centred, 44pt — once per deck |
| `\keyline{…}` | the one-line thought a section closes on |
| `\sectionopener{n}{title}{line}` | section divider, on a `[plain]` frame |
| `\titlepagednc{kicker}{title}{sub}{by}` | the lecture title slide |
| `\hrulethin{0.4}` | a thin rule, as a fraction of `\linewidth` |
| `\slidetop` `\slidegap` `\rulegap` | the deck's only three vertical gaps |
| `\dotnetlecture{Л1}` | the lecture tag in the footer (preamble) |

Listing styles: `csharp`, `xml`, `json`, `bash`, `plain` (IL and file trees),
`term` (terminal output, one size smaller).

```latex
\begin{lstlisting}[style=term]
$ dotnet --info
\end{lstlisting}
```

## Palette

Tuned for a projector in a lit hall, not for a laptop screen: every value is
darker than its Pure counterpart, because Pure's `muted` in particular
disappeared on a wall.

`ink` `#14171A` on `paper` `#FBFAF7`, `muted` `#3F464D`, `hairline` `#B3AFA6`,
`codebg` `#EFEDE6`.

**Two colours carry meaning, and only two.** `accent` `#0E5A72`, a deep teal:
the word a claim turns on, the first side of a comparison, the thing you
should do. `alt` `#9C3D14`, a brick: the second side of a comparison, and the
one caution line per deck (`bin` and `obj` in Л1). `warn` is an alias of
`alt`. The pairing is held across the whole course, so the room learns it
instead of decoding it on each slide.

Use them often — a slide with no colour on it reads as flat from the back of
the room. What is not allowed is a third colour, or colour as decoration.
Nothing inside a snippet is singled out: the slide's own sentence says which
line matters.
