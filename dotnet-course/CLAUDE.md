# .NET course — house rules

Applies to everything under `dotnet-course/`. Lecture-specific notes live in
each lecture's own `CLAUDE.md`; the theme and build rules are in `theme/`.

Each lecture is three files that move together — the script (`Lnn-*.md`, the
source of truth), the theses (`README.md`) and the deck (`slides/*.tex`). A
change to a claim, a number or a section's shape lands in all three in the
same commit.

## Presentation preferences

Learned from review of Л1. These are settled decisions — follow them rather
than re-deriving, and don't quietly reverse one because a slide looks tight.

### Colour and contrast

- **The palette targets a projector in a lit hall, not a laptop.** The Pure
  deck's values were a step too light; `muted` in particular disappeared on a
  wall. Never lighten `ink`, `muted` or `hairline` back toward Pure's.
- **Exactly two colours carry meaning.** `accent` (teal) is the primary: the
  word a claim turns on, the first side of a comparison, the thing you should
  do. `alt` (brick) is the second side of a comparison and the one caution
  line per deck. The pairing is held across the whole course so the room
  learns it instead of decoding it each time.
- **Use them often.** A slide with no colour on it reads as flat from the back
  of the room. `\hi{}` and `\lo{}` are cheap; what is not allowed is a third
  colour or decorative colour.

### Structure and alignment

- **A comparison is two colour-coded bullet lists**, not two blocks of prose.
  `\versus{}{}{}{}` with `cbul` inside, headings via `\colhead`. Items short
  and parallel, so the two columns line up line for line.
- **`\eyebrow` and `\colhead` uppercase their argument**, so pass words with
  spaces: `Package Reference`, never `PackageReference` — it renders as
  `PACKAGEREFERENCE`.
- **Claims are short.** «Расшифруем.» beats «Расшифровываю один раз, дальше
  пользуюсь без расшифровки.» If a claim needs two clauses, one of them
  belongs in the script or in `\aside{}`.
- **Not every section needs a closing `\keyline`.** Л1 keeps them only where
  the thought is genuinely load-bearing (§2, §4); §1, §3 and §5 end on content.
- **Nothing may touch the footer.** Long asides under a tall diagram are the
  usual cause; tighten the diagram rather than shortening the thought.

### What goes on a slide

- **Terminal output is real**, captured from the machine, never invented or
  touched up. Run the command, paste the result. Anonymise only the home
  directory — `/home/user`, never a real username.
- **Contact slide carries links only** — QR codes and an address. Achievements
  and credentials are said out loud and live in `\note{}`.
- **No forward references in asides.** «Файл, который мы увидим в конце
  лекции» rots the moment a slide is cut. Say what the thing does now.
- **Terminology must not collide across a deck.** «Манифест сборки» means
  `csproj` and nothing else.
- **A phrase the room photographs is centred and 44pt** (`\shout`), not 64 —
  large enough to carry, small enough not to shout twice.

### Conventions

- Sample project in every lecture is **`HelloWorld`**.
- Target is **.NET 10 (LTS)**; "now" is **September 2026**, so .NET 11 (STS)
  is «выйдет в ноябре этого года».
- Settled wording: «манифест сборки» (not «программа сборки»), «реестр
  пакетов» (not «репозиторий»), «патчи» (not «заплатки»).
- **Benchmarks are not a Л1 topic.** BenchmarkDotNet and «угадай, что
  быстрее» belong to Л3 and Л15.

## Slide cues in the script

Every `[СЛАЙД n — …]` in a script is numbered and corresponds to exactly one
frame, in order. After adding or cutting a frame, renumber the cues and check
the count matches the PDF:

```sh
grep -o '\[СЛАЙД [0-9]*' Lnn-*.md | grep -o '[0-9]*' | sort -n | tr '\n' ' '
pdfinfo slides/Lnn-*-slides.pdf | awk '/^Pages/{print $2}'
```

Other markers: `[ВОПРОС В ЗАЛ]` is a show of hands (`\vote{}` on a slide, the
question alone); `[ПАУЗА]` is a beat in delivery and **not** a slide;
Labs are not markers in the script: each lecture keeps its lab in `lab/`, as
a task for the student and a companion file for whoever accepts it.

## Building

```sh
cd Lnn-*/slides && make        # also: make notes / watch / clean
```

Engine is pdflatex. `texlive-lang-cyrillic` is required — without it nothing
Russian compiles at all. `texlive-fonts-extra` (PT Sans) is not: without it
the deck still builds but falls back to Latin Modern, so both paths are worth
a look after an edit. A newly installed font also needs `updmap-user`: pdfTeX
reads the user map in `~/.texlive*/`, which shadows the system one.

- Any frame with `lstlisting` must be `[fragile]`.
- **Listings are ASCII only** — pdflatex + T2A will not set Cyrillic inside
  one. Russian belongs in the prose around the listing.
- Vertical space comes from `\slidetop`, `\slidegap`, `\rulegap`; a raw
  `\vspace` is for tuning a code frame and nothing else.
- **Look at the pages, not just the log.** A listing that is too wide
  overflows its grey ground silently, and a slide can run into the footer,
  with a clean log in both cases. `pdftoppm -r 100 -f N -l N -png deck.pdf p`
  renders one page to look at.
