# Л1. Репозиторий — центр мироздания проекта — house rules

Working notes for this lecture's directory. Rules shared by both lectures live
in [`../../../CLAUDE.md`](../../../CLAUDE.md). Read that first; this file
carries only what is specific to Л1.

## What is in here

| File | What it is |
|---|---|
| `L01-repository.md` | The script: slide cues and the thesis each frame carries. **Source of truth.** Spoken prose is not written yet. |
| `README.md` | Theses — a compressed index of the script, with the frame each thesis lives on. |
| `slides/L01-repository-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |
| `slides/img/` | QR codes for the contact slide (frame 2), copied from the .NET course. |

The three move together. **Adding, cutting or reordering a frame invalidates
the numbers** — renumber the cues in the script, then re-derive the tags in
`README.md`, in the same commit. There is no `lab/`: labs are not planned.

## Facts this lecture commits to

- Section timings: **5 / 15 / 25 / 13 / 22** = 80 minutes.
- **39 slides**, numbered 1–39 in order.
- Git is one section of four (§2): the model, nine everyday commands, bisect.
  It must not grow into a command reference.
- The sample repository is `HelloWorld`. Every terminal listing (frames 17,
  19, 20, 23) is real output of git 2.43.0 on a throwaway repository, author
  `Student`; only the home directory is anonymised. Re-capture rather than
  edit: the hashes on frames 14, 15, 17, 19, 20 and 23 belong to one history.
- One `\statement` pair (frames 5, 35), one `\shout` (frame 39), three
  `\keyline`s (frames 12, 29, 38).

## Open items

The author's call — see «Открытые вопросы» in
[`../../course-structure.md`](../../course-structure.md).
