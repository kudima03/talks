# Pure ecosystem talk — working notes

Guidance for anyone (human or agent) editing the talks in this folder.

## Layout

```
pure-ecosystem/
├── CLAUDE.md       ← this file
├── en/             ← English
│   ├── pure-ecosystem-talk.md
│   └── slides/     ← Beamer deck (see its README)
└── ru/             ← Russian
```

One folder per language. Scripts are **spoken-delivery text**, not slide bullets — continuous prose meant to be read aloud.

**The script is the source of truth.** Slides carry code, diagrams and short statements; the spoken beats live in `\note{}` beside each frame. Change the script first, then the deck — and keep the frame's `\note{}` matching the paragraph it delivers.

## Speaker

Dmitry Kurochkin (`kudima03`) — .NET developer, author and main contributor of the Pure ecosystem (~75 NuGet packages, one repository per package). Source repos live under `/Users/dmitry/RiderProjects/` as `Pure.*` and `PureQL*`, and are public on GitHub under `kudima03` — verify snippets from there when the local checkout is not available (`gh api repos/kudima03/<repo>/git/trees/HEAD?recursive=1`, then raw.githubusercontent.com).

## Attribution — non-negotiable

Elegant Objects is **Yegor Bugaenko's** invention. He did the thinking, the research, wrote the books, and took the criticism. Pure is an implementation of those ideas for .NET with some divergences.

Credit him in the first minute. Never open with a provocative "your code is wrong" hook — it reads as loud and as borrowed thunder. Open plainly and give credit first.

## Vocabulary

| Use | Never use |
|---|---|
| read-only field, computed field | getter, accessor, property-with-logic |
| **native field** — `BoolValue` / `NumberValue` / `TextValue` | "the value property" |
| transformation of data | manipulation of data |
| "a sequential set of object states" | "an object" (when describing a written-down transformation) |

**Native fields** are the bridge point between .NET and the ecosystem. They are forced to exist, and they are *always computed, never stored*. Full evaluation happens when one is read.

## Doctrine to state correctly

- **The transformation lives in constructors.** Exactly one constructor assigns fields, and as soon as a component has a second constructor that assigning one is **private**. Every other constructor is a conversion that composes its inputs into objects and delegates. That delegation chain *is* the transformation. (State it that way — single-constructor components like `ConcatenatedString` and `Sum<T>` do assign publicly, so "the assigning ctor is always private" contradicts the very first snippet.)
- **Attribution wording.** "The concept and the inspiration were taken from Elegant Objects"; the core ideas are taken **from the Elegant Objects concept**, but this is *not* Elegant Objects ported to .NET — in a lot of places the implementation differs from how the author sees it.
- **Classic code is "a sequential set of instructions"** — that is what is written down. "A sequential set of *object* states" is the Pure alternative. Data states are what happen at runtime; don't call the written artefact that.
- **Elegant Objects argues against exposing *fields*** (not "state").
- **Object state is initialised only via composition in constructor delegation.** Don't describe fields as "some stored, some computed" — that framing was rejected.
- **Determined hashes, stated positively.** Say "we use determined hashes instead," never "we deprecate / destroy / reject `GetHashCode`." The reason to lead with: `GetHashCode` differs between program runs, so it cannot be identity. What we want is the hash of an **object state snapshot**.
- **Order matters:** determined hashes must be explained *before* the switch that uses them.
- **Collections follow from hashes:** because built-in `GetHashCode` is unused, framework collections cannot be used — the ecosystem ships collection wrappers keyed on determined hashes.
- **Allocator = one short thought, no hedging paragraph.** "Determined hashes open doors for allocator implementations: objects are immutable and their state can be determined, so the allocator can check whether an object with the same state is already allocated and return the existing reference instead." Not built yet — but state it briefly and move on.
- **Mental power:** it takes more mental power to *design*, but the design is much simpler, better and more satisfying to *read* than the classic alternative. Do not frame this as a cost the audience must accept.
- **Adapters** are the real cost. Keep it general — "everything that meets classic .NET needs a wrapper." The main thought: write the adapter, or implement as large a domain as possible inside the ecosystem, evaluate via a native field at the edge, and go further. Do not enumerate individual adapter packages.
- **Testing is a headline benefit,** not an afterthought: everything is bounded, immutable, thread-safe.
- **Naming: what a component *is*, not what it does.** `ConcatenatedString`, `Sum`, `Difference`, `CurrentTime`, `Millennium`. Never `StringConcatenator`, `Calculate`, `TimeProvider`. A composition read aloud should be a noun phrase.
- **Evaluation is not memoised.** A composition keeps no result — read `.TextValue` twice and the graph is walked twice. `CachedString` (a `Lazy<string>` with `ExecutionAndPublication`) is the opt-in, and it is just another `IString` in the graph. Note the exception: `Lazy`-backed leaves such as `Int` and `RandomString` do hold their value once read.
- **Identity is not framed as "a constructor parameter."** That beat was cut in review — say the switch is *told how to identify* its keys via a determined state hash, and stop there.

## Style rules

- No absolute clock references in spoken text ("by minute twenty", "the last third of this talk"). Section headers carry a duration (`≈ 5 min`) only.
- Prefer showing a code snippet over a bare `[SHOW: …]` cue. If a beat needs a slide, write the snippet.
- Show *usage* as well as declaration — a composition the audience can read as one expression (`new WrappedString(new LeftCurlyBracketString(), new RandomString(), new RightCurlyBracketString())`).
- Where a mechanism would otherwise be shown as internal LINQ, draw it instead — the switch's branch selection is a tree diagram, not a `.Where(…)`.
- Delivery marks: `[PAUSE]`, `[LONG PAUSE]`, `**bold**` for vocal stress, `[SHOW: …]` for slide cues. Use sparingly so they keep meaning.
- Pacing: ~128–140 spoken words per minute. A 30-minute slot is ≈ 3,900–4,200 spoken words (excluding code blocks).
- Avoid over-aggressive framing. Confident and direct is right; combative is not.
- Every named example must be a **concrete, named, reusable, testable** type with immutable state — e.g. `Millennium : IDate` rather than an anonymous literal.

## Code snippets

Quote **verbatim from the source repos** and re-check before committing. Do not quote package READMEs — several disagree with their own code (`Materialized`, `Cached`, `Choices`, `Switches`, `Linq.Conditions`).

Deliberate exceptions in the current English script, all requested in review:

1. **`DateChoice`** is shown in idiomatic constructor-delegation form (public ctor composes three `NumberChoice<ushort>` and delegates; private ctor assigns). The shipped type instead evaluates `_condition.BoolValue` inside each field. The script teaches the intended pattern.
2. **`Millennium`** is an illustrative type, not a shipped one.
3. **`TotalWithVat`** is an illustrative type, not a shipped one — a domain-level example of the same delegation pattern, composed only from shipped `Sum<T>` / `Product<T>`.

Snippets are trimmed for the slide: the `GetHashCode` / `ToString` / `GetEnumerator` members that every shipped record carries are cut. Nothing else is altered.

Prefer non-primitive samples. `Int`'s four constructors were rejected in review as a primitive example; `WrappedString` carries the same lesson (only the private ctor assigns) and is one layer up.

## One job per beat

Review has twice flagged **semantic duplication** in section 4 — constructor delegation re-explained under every snippet, "nothing ran" restated three times, and the "named / reusable / testable" point repeated from `Millennium`. Each snippet now carries exactly one idea, and nothing earns a second explanation:

| Snippet | Its one job |
|---|---|
| `ConcatenatedString` | the component *is* the interface — and its ctor only assigned, so nothing runs except `new` |
| `WrappedString` | the delegation chain; only the private ctor assigns; even defaults are objects |
| `TotalWithVat` | the same shape at domain level — a business rule as a tree of objects |
| usage samples | what a program looks like; the compositions are cheap values to move around |
| `CachedString` | evaluation happens per read, and caching is one more object in the graph |

Before adding a paragraph, check it is not the previous point in new words.

## Out of scope for this talk

- Relational schema and PureQL — a separate talk.
- `Pure.Primitives.Materialized` — **obsolete**, do not present it. Evaluation is explained via native fields instead.
- Do not mention `Pure.Serialization.Json` or `Pure.RelationalSchema.Conditions` (no `.cs` files shipped), or the three placeholder `FakeTests` projects.

## Accuracy guardrails

Verified during the scan; keep the script honest about these:

- There is **no custom allocator**. The collections are `FrozenDictionary`/`FrozenSet` + `Lazy<T>` + immutability. Allocation is an opportunity, never a claim.
- The `int` hash is **not eliminated** — 256-bit identity folds back to an int for bucket selection in `EqualityComparerByDeterminedHash`. Frame it as "demoted from an identity to a bucket index."

## Workflow

Deliver via PR. Address review comments in the **same** PR rather than opening a new one.
