# A Program Is Nothing But `new`

**The Pure ecosystem — bringing Elegant Objects to .NET.**
Dmitry Kurochkin (`kudima03`) · 30 min · [script](en/pure-ecosystem-talk.md) · [slides](en/slides/)

Elegant Objects is **Yegor Bugaenko's** invention. Pure is an implementation of those ideas for .NET, with divergences of my own. Credit goes first.

## The premise

> **The purpose of a program is to transform data. And the result of a program is also data.**

Classic code writes down *a sequential set of instructions* that **performs** the transformation — it exists only while the CPU runs. Pure writes down *a sequential set of object states* instead: the result itself, unevaluated. **A thing that already is the answer, and simply has not been asked yet.**

## Five rules

1. A component has only fields. **No methods.**
2. Execution logic is achieved only through **composition of objects**.
3. Iron immutability — not "immutable by convention."
4. Lazy evaluation. **Nothing runs except `new`.**
5. Deterministic hash codes.

## What follows

- **.NET welds the door shut.** `String` is sealed, numerics are structs, the whole BCL is written against concrete types. *"You reach for LINQ and lambdas because the alternative — real composition — was structurally removed before you sat down."* So you re-found the primitives: nine interfaces, not one method among them.
- **Native fields** (`BoolValue` / `NumberValue` / `TextValue`) bridge back to .NET — always computed, never stored. `IDate` has none: a date **is** three numbers.
- **The transformation lives in the constructors.** One constructor assigns; every other one composes and delegates. That delegation chain *is* the program. `ConcatenatedString` doesn't produce an `IString` — **it is an `IString`.**
- **Name what a component *is*, not what it does** — `Sum`, `Difference`, `CurrentTime`. A composition read aloud is a noun phrase.
- **Determined hashes, because `GetHashCode` gives a different answer the next time you run the same program.** Identity is 32 bytes of SHA-256 over a state snapshot, prefixed per type. The `int` isn't gone — it is **demoted from an identity to a bucket index**. Which is also why framework collections are out, and the ecosystem ships its own.
- **`if` is an object.** A choice that produces a string *is* a string, so it composes anywhere. In `DateChoice` the branch is **distributed across every field** — ask only for the year and the day is never chosen.
- **The switch never asks objects whether they are equal — it is told how to identify them.**

## Cost and payoff

Everything that meets classic .NET needs an **adapter** — the real price. Either write it, or push the edge further out and evaluate through a native field at the very border. It takes **more mental power to design**; the design is simpler and more satisfying to read.

Testing is the headline benefit: bounded, immutable, thread-safe. Build the object, read the field, compare.

~75 NuGet packages, one repository each — deliberately not a monolithic SDK. **You do not adopt the ecosystem. You take the two packages you need.**

## The close

> **Your program is a graph of `new`. And everything that happens, happens once, at the end — when somebody finally reads a field.**

At the edges Pure disappears completely: the wire sees a string, the database sees a column, the client sees a primitive.
