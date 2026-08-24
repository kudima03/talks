# Repository management talk — working notes

Guidance for anyone (human or agent) editing the talk in this folder.

## Layout

```
repository-management/
├── CLAUDE.md       ← this file
└── ru/             ← Russian
    └── repository-management-talk.md
```

One folder per language, following the same convention as `pure-ecosystem/`. When an English version is written, it goes in `en/`, mirroring the Russian script — not a translation-by-machine, but the same talk delivered in English.

The current file is a concept note (the pitch), not yet a spoken-delivery script. Once the script exists, it becomes the source of truth and should follow `pure-ecosystem`'s split: script first, then slides, with `\note{}` blocks kept in sync with the paragraph they deliver.

## Core thesis — non-negotiable

This is **not** a course about how to use Git. It is a course about **managing the lifecycle of a digital product through its repository**.

The central claim: a programmer's professionalism is measured less by how much code they can write, and more by **how much chaos they can organize and control**. Frame every example around this — a repo is not a code dump, it's the self-contained record of what a product is, how to build it, test it, change it and ship it: tasks, docs, config, lockfiles, tests, CI, contribution rules, CODEOWNERS, releases, changelog, and the full trace of changes.

## The five skills

Structure the course around this progression — do not reorder or rename them:

**Mastering mindset → Mastering repository organization → Mastering task management → Mastering code delivery → Mastering defense.**

The end state: a student can take a chaotic project and turn it into a system where changes are controlled, reproducible, reviewed, and can evolve safely.

## Doctrine to state correctly

- **main is always working and protected.** Never present this as a nice-to-have; it's the baseline the rest of the course assumes.
- **A PR without a linked task and a test is a suspicious PR** — not automatically wrong, but it should raise a reviewer's guard.
- **A branch must be up to date with main before merge.** Frame staleness as a source of silent integration bugs, not just a merge-conflict inconvenience.
- **One commit, one logical thought.** Atomic commits are a communication tool for the next person doing `git bisect`, not a style preference.
- **Code review is a defense mechanism, not a formality.** Don't present it as bureaucracy to get through — it's the thing that makes the process independent of trusting any one contributor (human or AI).
- **Static analysis, security scanning, Dependabot and supply-chain security are part of development**, not an add-on bolted on before release.
- **Builds must be reproducible.** "Works on my machine" is explicitly retired as an argument.
- **Backward compatibility, deprecation policy and versioning are their own engineering discipline** — not something that falls out naturally from good code.
- **Repository history must stay legible and useful** — squash/atomic commits, semantic versioning, meaningful changelogs.
- **After merge, code stops being "so-and-so's code."** It belongs to the repository, and the team is accountable for it.

## Framing AI correctly

- Writing code without AI today is like driving in an unfamiliar city without navigation — but when the navigator is wrong, the human still has final control.
- The senior/tech lead's role shifts to being **the coordinator of people and AI agents, and the last point of quality control.**
- AI can write code, invent an API, a dependency, or a test — and can get any of those wrong (hallucinated dependency, broken backward compatibility). The repository and the processes around it are what must catch this before it reaches the product — **not trust in the AI or in the person who prompted it.**
- Never frame AI as replacing the process described above; frame it as the reason the process is now non-optional.

## Practices to cover

Trunk-based development vs GitFlow, squash vs merge commits, atomic commits, `git bisect`, semantic versioning, releases, feature flags, canary/rollback, SBOM and supply-chain security. Each should be tied back to one of the five skills, not presented as a standalone tools tour.

## Style rules

- Keep the framing product-lifecycle-first. If an example could just as easily be retitled "Git tips," rewrite it so the repository's role in *controlling chaos* is explicit.
- Avoid treating any practice (review, CI, versioning) as a formality to check off — everything ties back to "how do we stay in control without relying on trust in one person."

## Workflow

Deliver via PR. Address review comments in the **same** PR rather than opening a new one.
