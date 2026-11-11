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

- Section timings: **5 / 15 / 22 / 18 / 20** = 80 minutes.
- **45 slides**, numbered 1–45 in order.
- Git is one section of four (§2): the model, the first commit, branching,
  the three ways to merge, nine everyday commands. It must not grow into a
  command reference. **Bisect was cut in review** — do not restore it.
- **History is drawn, not pasted.** Frames 15 and 19–22 are vertical diagrams
  (newest commit on top, main in teal, the task branch in brick); there is no
  terminal listing left in §2. Commands are set in `\texttt` at body size.
- **The hashes on the diagrams are real**, from git 2.43.0 on a throwaway
  `HelloWorld` repository, author `Student`: `cb5849b`, `408319b`, `983e9ae`
  on main; `3508968`, `c320998` on `feat/greet-by-name`; `845636a` the merge
  commit; `5e77d87` the squash commit; `68fa711`, `bffa5e2` the rebased
  pair. They belong to one history — re-run rather than edit.
- §3 is **«Организация репозитория и GitHub»**: the remote repository and
  the platform first (frames 26–31: remote, GitHub vs Git, Issue, Pull
  request, Actions, settings and protections), then what the repository
  holds (frames 32–35). §3 states what the platform can do; §4 says how it
  must be set up, and carries the workflow file.
- **«Pull request» and «issue» do not appear before §3** — not on a slide,
  not in a note. First-year students may not know the words; frames 28 and
  29 introduce them. Before that it is «задача в репозитории» and
  «изменение».
- One `\statement` pair (frames 5, 41), one `\shout` (frame 45), three
  `\keyline`s (frames 12, 35, 44).

## Open items

The author's call — see «Открытые вопросы» in
[`../../course-structure.md`](../../course-structure.md).
