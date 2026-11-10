# Л6. Ресурсы, файлы и асинхронность — house rules

Working notes for this lecture's directory. Course-wide rules — the palette,
the two-colour system, what may go on a slide, the build, the slide-cue
convention — live in [`../../../CLAUDE.md`](../../../CLAUDE.md). Read that
first; this file carries only what is specific to Л6.

## What is in here

| File | What it is |
|---|---|
| `L06-resources-and-async.md` | The spoken script. **Source of truth.** |
| `README.md` | Theses — a summary and a crib sheet for the lectern, with the frame each thesis lives on. |
| `slides/L06-resources-and-async-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |

The three move together. Every thesis in `README.md` carries a frame number
and `[—]` marks a thesis with no frame. **Adding, cutting or reordering a
frame invalidates those numbers** — renumber the cues in the script, then
re-derive the tags in `README.md`, in the same commit. There is no `lab/`
directory yet: the lab is described only by the `[ЛАБА 6 — …]` block at the
end of the script.

## Shape specific to Л6

- **55 frames**, numbered 1–55: §1 2–23, §2 24–55. Five `\vote` frames (4,
  25, 30, 41, 48) — one per `[ВОПРОС В ЗАЛ]`, the question alone.
- **The `IDisposable` block comes first**, right after the managed/unmanaged
  problem (frames 6–13), and the three file levels (14–16) come after it. This
  was a review decision: `using` is demonstrated before `FileStream`, so the
  file levels can lean on it. Do not move it back below the levels.
- **The three file levels get one frame each**, a principle in one line — not
  a single ladder frame.
- **No `IAsyncDisposable` section, no `Close`, no leak-hunting paragraph, no
  history of `Begin/End`, no synchronization-context paragraph, no fourth
  "grabli" paragraph.** All were cut on request in PR review (`#18`).
  `await using` is explained in one sentence next to frame 52 instead. With
  the synchronization context gone, `.Result` is explained by pool starvation
  alone, not by the UI deadlock — keep it that way.
- **Listings follow the review's style rules:** explicit types, never `var`;
  braces on every block; no target-typed `new`; no `static` on the example
  methods; no pattern matching (`while (line != null)`, not `is { } line`).
- **Frame 35 is the deck's one `\statement`**; frame 45's «Не блокируй
  поток!» is a plain large line, not a `\shout`.
- **Comparison order:** the side to follow is on the left in teal — `Task.Delay`
  before `Thread.Sleep` (34), absolute before relative path (19), «создали
  сами» before «получили снаружи» (13).
- **Frame 46 is four numbered blocks**, each a bold step plus one muted line, on three lanes (поток 3, база, поток 7) — the earlier version's inline labels were unclear in review.
- **Frame 50 is three chained methods passing `ct`**, not the earlier diagram of the source→endpoint→…→driver chain.
- **Frame 54 is titled «Три опасности»**, not «Три грабли».
- **Frame 47** closes the «how await works» block; frames 23 and 55 close the
  two sections. Everything else ends on content.

## Code on the slides — how it was checked

All listings were compiled and run together in one scratch console project on
dotnet SDK **10.0.112** (Linux); the project is not in the repo.

| Frames | How it was verified |
|---|---|
| 7, 8 (`FindHello`) | Compiled and run on a four-line file; both forms return the line with `Hello!`. |
| 10 | Compiled and run; `writer` and `stream` disposed in reverse order, the file is written in full. |
| 11 (forgotten `using`) | `File.OpenWrite(path)` left open, then `File.ReadAllText(path)`: the read throws `System.IO.IOException: The process cannot access the file '…' because it is being used by another process.` The text on the slide is that output with the temp directory replaced by `/home/user`. **Two `new FileStream(path, FileMode.Create)` in a row did not throw on Linux** (default `FileShare.Read`), and `StreamWriter` + `File.ReadAllText` does not throw either (it returns an empty string, the buffer was never flushed) — so the demo uses `File.OpenWrite`, which opens with `FileShare.None` and fails the same way on every OS. |
| 12 (`LogWriter`) | Compiled and run: second `Dispose` is a no-op, `Write` after `Dispose` throws `ObjectDisposedException`. |
| 17, 18 | `Path.Combine`/`Join` print `data/logs/app.txt` on Linux; the five signatures are the real ones, read by reflection. The Windows column of frame 17 is the documented behaviour, not run here. |
| 21 | Compiled; the finalizer is never invoked by the slide, it only shows syntax. |
| 32, 33 | Compiled and run (`Task.Run(() => 6 * 7)` → 42; `ReadAllTextAsync` → file length). `Compute()` on the slide is a placeholder. |
| 36 | Compiled and run against stub `LoadUserAsync`/…/`NotifyAsync` returning completed tasks. |
| 38 | Compiled and run: the `catch` prints `boom`. |
| 49 | Compiled and run with two reads of the same file. |
| 50 | Compiled and run: three chained methods return `orders: 3`; with an already-cancelled token the call throws `OperationCanceledException`. |
| 51 | Compiled and run on a three-element array: returns 6. |
| 52, 53 | Compiled and run: empty lines are dropped; the two `ERROR` lines come out of the `await foreach`. `token` on frame 53 is any `CancellationToken`. |

Listings are ASCII only, including comments, same as every other lecture.
Frames 12, 38, 50 and 51 set the listing at 6.6–6.8pt to fit above the footer —
if a line is added there, check the built PDF, not just the `.tex`.

## Open items

The author's call. Do not quietly "fix" these:

1. **`yield` is referenced in Л6 but not taught in Л5.** The script says
   «помните yield из прошлой лекции» before the state-machine steps (frame
   39) and before `IAsyncEnumerable` (frame 53), and frame 53 contains `yield
   return`. Л5 deliberately cut `yield return` (see its `CLAUDE.md`). Either
   restore it there or teach it here from scratch.
2. **Section timings are 30 + 45 = 75**, not 80; the header says 80.
3. **The Latin Modern fallback has not been checked** for this deck; it builds
   with PT Sans.
