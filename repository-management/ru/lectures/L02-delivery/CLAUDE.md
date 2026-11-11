# Л2. Лидер, задачи, поставка — house rules

Working notes for this lecture's directory. Rules shared by both lectures live
in [`../../../CLAUDE.md`](../../../CLAUDE.md). Read that first; this file
carries only what is specific to Л2.

## What is in here

| File | What it is |
|---|---|
| `L02-delivery.md` | The spoken script, with one `[СЛАЙД n — …]` cue per frame. **Source of truth.** |
| `README.md` | Theses — a compressed index of the script, used as a crib sheet at the lectern. Every thesis carries its frame number; `[—]` marks one that is spoken only. Written by hand, not generated. |
| `slides/L02-delivery-slides.tex` | The deck. One frame per `[СЛАЙД n — …]` cue. |
| `slides/img/` | QR codes for the contact slide (frame 37), copied from the .NET course. |

The three move together. **Adding, cutting or reordering a frame invalidates
the numbers** — renumber the cues in the script, then re-derive the tags in
`README.md`, in the same commit. There is no `lab/`: labs are not planned.

## Facts this lecture commits to

- **The script is written for the ear**, about 6,800 words. That is under an
  hour of speech; the rest is the author's own story about tests (frame 4,
  marked `[ИСТОРИЯ АВТОРА: …]` in the script and still to be written), the
  questions to the room and the discussion the lecture invites.
- **Where the script states the author's position rather than industry
  practice, it says so aloud**: the leader as main contributor (frame 11) and
  pay per artefact (frame 18). Keep those sentences.
- Section timings: **2 / 10 / 20 / 20 / 20 / 8** = 80 minutes.
- **38 slides**, numbered 1–38 in order.
- §1 «Тесты и review» (frames 3–5) came from Л1 §4. The phrase «код без
  теста — мнение, код с тестом — факт» is the claim of frame 4, not a
  `\statement`: the deck already has its two.
- §3 shows both sides explicitly — the repository owner with a budget and the
  developer — on frames 18 and 19.
- Frame 8 paraphrases section 11 of the OSS *Simple Sabotage Field Manual*
  (1944) in Russian. It has not been checked against the original.
- Frame 25's `Greeter` snippet is illustrative and has not been compiled.
- Two `\statement`s (frames 9, 36), one `\shout` (frame 38), one `\keyline`
  (frame 13).

## Open items

The author's call — see «Открытые вопросы» in
[`../../course-structure.md`](../../course-structure.md).
