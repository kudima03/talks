# Л2. Сборки, модели поставки и память — house rules

Working notes for this lecture's directory. Course-wide rules — the palette,
the two-colour system, what may go on a slide, the build, the slide-cue
convention — live in [`../../../CLAUDE.md`](../../../CLAUDE.md). Read that
first; this file carries only what is specific to Л2.

## What is in here

| File | What it is |
|---|---|
| `L02-assemblies-and-memory.md` | The spoken script. **Source of truth.** |
| `README.md` | Theses — a compressed index of the script, with the frame each thesis lives on. |
| `slides/L02-assemblies-and-memory-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |
| `lab/LAB02-task.md` | The lab, as handed to the student. |
| `lab/LAB02-review.md` | The same lab for whoever accepts it. |

The four move together. Л2's `README.md` is tied to the deck more tightly than
Л1's: every thesis carries a frame number, and `[—]` marks a thesis with no
frame. **Adding, cutting or reordering a frame invalidates those numbers** —
renumber the cues in the script, then re-derive the tags in `README.md`, in the
same commit.

## Captured output — how to reproduce it

Three listings in the deck are real output, not typed by hand. They were taken
on dotnet SDK **10.0.401** with ilspycmd **11.0.0**. Re-capture rather than
edit when a version moves:

| Frame | Command |
|---|---|
| 4 | the reflection snippet from the script, run as a console app; both the code and its six output lines are on the slide |
| 12 | `unzip -l` on `~/.nuget/packages/pure.primitives/3.6.5/pure.primitives.3.6.5.nupkg`, columns `Date`/`Time` dropped to fit |
| 8 | `ilspycmd bin/Release/net10.0/HelloWorld.dll` on a stock `dotnet new console`, attribute block cut at `...` |

Frame 12 is doing double duty: that package is version **3.6.5** and carries an
assembly version of **1.0.0.0**, which is exactly the claim the script makes
about the two numbers being different. **Do not swap the package for another
one** without checking that the replacement still shows a mismatch — and that
it still has both `lib/net10.0` and `lib/net8.0`, which the cue asks for.

Frame 8 likewise earns its place three times over: the three version attributes
from one csproj property (§1), the generated `internal class Program` with its
`private` Main (§1 and Л3), and decompilation as a technique. Shortening it
costs one of the three.

## Facts this lecture commits to

- Section timings: **20 / 15 / 20 / 20 / 5** = 80 minutes.
- **31 slides**, one per `[СЛАЙД n — …]` cue, numbered 1–31 in order.
- The sample project is `HelloWorld`, the same one as Л1 — frame 8 decompiles
  literally the artefact Л1 ended on.
- The deck's one caution line is «Метод не найден — при первом вызове, уже
  в проде» on frame 6. There is no `\shout` and no `\statement`; the three
  section thoughts are `\keyline` (frames 9, 16, 30), matching the three
  «мысль раздела» cues. §3 and §5 close on content, on purpose.

## Cut in review — not to be restored without asking

- **The container frame** (§3). The topic stays in the script and is delivered
  by voice; it is developed properly in Л12 and Л10.
- **The `dotnet-counters monitor` screenshot** (§5), cue and all.
- **The ILSpy GUI screenshot** (§1), replaced by console output of `ilspycmd`
  on our own `HelloWorld.dll`. The two paragraphs around that cue were rewritten
  to match, and the then-redundant «третий сценарий» paragraph folded into them.
- **A frame for the lock file and central package management** (§2) was drafted
  and dropped: the script has no cue there, so it is voice-only. It is the
  longest stretch of §2 without a slide — worth knowing before you stand up.

## Open items

Raised while building the deck, not resolved in the script — the author's call,
do not quietly "fix" them:

1. **«Манифест» now means three things across the course.** Л1's script uses it
   for the tool manifest, the course rules settle «манифест сборки» as `csproj`
   and nothing else, and Л2 uses it for the assembly manifest inside the dll —
   which is the standard term and hard to avoid here. The deck says «манифест»
   bare on frame 3 and never «манифест сборки», which is the most that can be
   done inside Л2 alone. A course-wide decision is still owed.
2. **Four of the five delivery steps have no frame of their own.** Only trimming
   does; the rest live on the §3 opener's ladder and in the comparison table on
   frame 20. Twenty minutes of §3 rest on two frames plus voice. If the section
   ever runs long, this is where it will show.
3. **The Latin Modern fallback does not build on the author's machine.** With
   `paratype` absent, `T2A/lmss` resolves to a bitmap font and microtype's font
   expansion aborts the run. **Л1 fails identically**, so this is a theme or
   distribution condition rather than anything in this deck — but the course
   rules ask for both paths to be looked at after an edit, and on this machine
   only one of them can be.
