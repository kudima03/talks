# A Program Is Nothing But `new`

**The Pure ecosystem — bringing Elegant Objects to .NET**
Dmitry Kurochkin (`kudima03`) · 30 min · [script](en/pure-ecosystem-talk.md) · [slides](en/slides/)

> Elegant Objects is **Yegor Bugaenko's** invention — his thinking, his research, his books. Pure is an implementation of those ideas for .NET, with divergences of my own.

## TL;DR

**Stop writing the instructions that produce a transformation. Write the transformation itself — as a graph of objects that has not run yet.**

Five rules get you there: only fields, no methods · logic is only composition of objects · iron immutability · nothing runs except `new` · **determined hash codes**.

## The theses

**1. The purpose of a program is to transform data from one shape into another.** Web service, report generator, trading system, string concatenation — same shape. Everything else is machinery.

**2. Today we achieve that transformation with a sequential set of instructions. It should be a recipe.** Instructions have to be *run* before the transformation exists — it lives only while the CPU does, and then it is gone. A recipe already **is** the transformation, standing there unperformed.

**3. The recipe is a written-down sequential set of object states.** Not the steps that compute the result — the result itself, unevaluated. *A thing that already **is** the answer, and simply has not been asked yet.*

**4. .NET removed that option before you sat down.** `String` sealed, every numeric a struct, the whole BCL written against concrete types. No seam anywhere.

**5. That — not preference — is why .NET developers write functional code.** The SDK leaves nothing else to hold on to.

**6. A library cannot fix it, because a library sits on top of the problem.** You re-found the primitives: nine interfaces, not one method among them.

**7. Native fields are the bridge — always computed, never stored.** `IDate` has none, because a date **is** three numbers.

**8. The transformation lives in the constructors.** One assigns fields; every other composes its inputs into objects and delegates. That delegation chain *is* the program.

**9. A component does not produce its interface — it is its interface.** `ConcatenatedString` **is** an `IString`.

**10. Nothing runs except `new`.** Nothing is memoised either — caching is one more object you place in the graph.

**11. Name a component for what it *is*, not what it does.** `Sum`, `Difference`, `CurrentTime` — a composition read aloud is a noun phrase.

**12. `GetHashCode` cannot be identity — it gives a different answer the next time you run the same program.** We use determined hash codes instead: SHA-256 over a snapshot of an object's state, with a per-type prefix so `0`, `false` and `""` stop colliding.

**13. Identity is a sequence of bytes, not an integer.** `IDeterminedHash : IEnumerable<byte>` — that is the entire file. Framework collections go with it, so the ecosystem ships its own.

**14. `if` is an object.** A choice that produces a string *is* a string. In `DateChoice` the branch is distributed across every field: ask only for the year and the day is never chosen.

**15. The switch never asks objects whether they are equal — it is told how to identify them.** Only the winning branch is ever read.

## What it costs, what it buys

**16. Adapters are the real price.** Everything that meets classic .NET needs a wrapper. Either write it, or implement as much of your domain as possible inside and evaluate at the very edge.

**17. Harder to design, easier to read.** More mental power at the moment of writing; a simpler, better design every time anybody reads it.

**18. Testing is the headline benefit.** Bounded, immutable, thread-safe. Build the object, read the field, compare.

**19. You do not adopt the ecosystem. You take the two packages you need.** ~75 NuGet packages, one repository each — deliberately not a monolithic SDK.

## The close

> **Your program is a graph of `new`. And everything that happens, happens once, at the end — when somebody finally reads a field.**

At the edges Pure disappears: the wire sees a string, the database sees a column, the client sees a primitive.
