# A Program Is Nothing But `new`

**The Pure ecosystem — bringing Elegant Objects to .NET**
Dmitry Kurochkin (`kudima03`) · 30 min · [script](en/pure-ecosystem-talk.md) · [slides](en/slides/) · [the ecosystem](https://github.com/kudima03/Pure)

> [Elegant Objects](https://www.elegantobjects.org) is **[Yegor Bugaenko's](https://www.yegor256.com/)** invention — his thinking, his research, his books. Pure is an implementation of those ideas for .NET, with divergences of my own.

## TL;DR

**Stop writing the instructions that produce a transformation. Write the transformation itself — as a graph of objects that has not run yet.**

Five rules get you there: only fields, no methods · logic is only composition of objects · iron immutability · nothing runs except `new` · **determined hash codes**.

## The theses

**1. The purpose of a program is to transform data from one shape into another.** Web service, report generator, trading system, string concatenation — same shape. Everything else is machinery.

**2. Today we achieve that transformation with a sequential set of instructions. It should be a recipe.** Instructions have to be *run* before the transformation exists — it lives only while the CPU does, and then it is gone. A recipe already **is** the transformation, standing there unperformed.

**3. The recipe is a written-down sequential set of object states.** Not the steps that compute the result — the result itself, unevaluated. *A thing that already **is** the answer, and simply has not been asked yet.*

**4. .NET removed that option before you sat down.** `String` sealed, every numeric a struct, the whole BCL written against concrete types. No seam anywhere.

**5. That — not preference — is why .NET developers write functional code.** The SDK leaves nothing else to hold on to.

**6. A library cannot fix it, because a library sits on top of the problem — so we redefined the primitives themselves.** `IBool`, `INumber<T>`, `IString`: nine interfaces, not one method among them.

**7. Primitives are the bridge back to .NET, and their native field is always computed, never stored.** It is the single place the ecosystem hands the framework something it understands. `IDate` has none, because a date **is** three numbers.

**8. The transformation lives in the constructors.** One assigns fields; every other composes its inputs into objects and delegates. That delegation chain *is* the program.

**9. A component does not produce its interface — it is its interface.** `ConcatenatedString` **is** an `IString`.

**10. Nothing runs except `new`.** Nothing is memoised either — caching is one more object you place in the graph.

**11. Name a component for what it *is*, not what it does.** `Sum`, `Difference`, `CurrentTime` — a composition read aloud is a noun phrase.

**12. `GetHashCode` cannot be identity — it gives a different answer the next time you run the same program.** Its notion of sameness outlives nothing: not a file, not a network hop, not yesterday.

**13. Identity is a snapshot of an object's state.** That is what a determined hash code is: SHA-256 over the snapshot, with a per-type prefix so `0`, `false` and `""` stop colliding — and it says the same thing tomorrow, on another machine, in another runtime. Built-in hashing goes unused, so framework collections go with it and the ecosystem ships its own.

**14. `if` is an object.** A choice that produces a string *is* a string, so it composes anywhere one is accepted — and nothing downstream needs to know a decision is in there.

**15. `switch` is an object too — and it never asks whether two objects are equal, it is told how to identify them.** Determined hash codes decide which branch matches, and only the winning one is ever read.

## What it costs, what it buys

**16. Adapters are the real price.** Everything that meets classic .NET needs a wrapper. Either write it, or implement as much of your domain as possible inside and evaluate at the very edge.

**17. Harder to design, easier to read.** More mental power at the moment of writing; a simpler, better design every time anybody reads it.

**18. Testing is the headline benefit.** Bounded, immutable, thread-safe. Build the object, read the field, compare.

**19. You do not adopt the whole ecosystem. You take the packages you need.** ~75 NuGet packages, one repository each — deliberately not a monolithic SDK.

## The close

> **Your program is a graph of `new`. And everything that happens, happens once, at the end — when somebody finally reads a field.**

At the edges Pure disappears: the wire sees a string, the database sees a column, the client sees a primitive.

---

The ecosystem lives at [github.com/kudima03/Pure](https://github.com/kudima03/Pure) — the home repository indexes every package.
