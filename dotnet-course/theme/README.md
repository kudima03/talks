# Slide theme — .NET course

One beamer theme and one set of make rules, shared by all fifteen lectures.
A lecture's `slides/` folder holds two files: its `.tex` and a three-line
`Makefile`. Everything else lives here, so lecture 12 looks like lecture 1
without anyone remembering to make it so.

```
dotnet-course/
  theme/
    beamerthemedotnet.sty     ← palette, furniture, code styles, language
    dotnet-slides.mk          ← build rules (all / notes / watch / proof / clean)
  ru/lectures/L01-platform/
    slides/
      L01-platform-slides.tex
      Makefile                ← DECK, THEME, include
```

## Starting a new lecture

```sh
mkdir -p ru/lectures/L02-runtime/slides
cat > ru/lectures/L02-runtime/slides/Makefile <<'EOF'
DECK  := L02-runtime-slides
THEME := ../../../../theme

include $(THEME)/dotnet-slides.mk
EOF
```

Then in the `.tex`:

```latex
\documentclass[aspectratio=169,10pt,t]{beamer}
\usetheme{dotnet}
\usetikzlibrary{arrows.meta,positioning,fit}
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
make proof    # render pages to .proof/, plus a contact sheet if imagemagick is present
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
- **The slide number set against the total** (`16 / 42`, the total lighter),
  and a 0.5pt hairline along the bottom edge that fills as the lecture runs.
- **The lecture tag in the footer** (`Л1`), so a photographed slide can still
  be placed in the course.

## Commands

| Command | For |
|---|---|
| `\claim{…}` | the claim the slide makes, directly under its title |
| `\hi{…}` | inline accent on the **one word** the claim turns on |
| `\aside{…}` | the quiet supporting line: evidence, caveat, consequence |
| `\eyebrow{…}` | small uppercase label above a column or a block |
| `\versus{h}{b}{h}{b}` | two-column comparison — SDK/Runtime, Debug/Release |
| `\vote{…}` | a question put to the room, alone on the slide (`[ВОПРОС В ЗАЛ]`) |
| `\statement{…}` | a phrase that *is* the content, set large |
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

`ink` `#1F2328` on `paper` `#FBFAF7`, `muted` `#5F666E`, `hairline` `#D2CFC8`,
`codebg` `#F4F2ED` — all identical to the Pure deck. One accent,
`#1F4E5F`, a deep teal that sits next to the ink rather than jumping off the
wall, and that nobody will mistake for the Pure brick halfway through a
semester. One more, `warn` `#7C3016`, used for the single "never do this" line
per deck (`bin` and `obj` in Л1) and nowhere else.

Nothing inside a snippet is singled out: the slide's own sentence says which
line matters.
