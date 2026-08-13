# A Program Is Nothing But `new`

### The Pure ecosystem — bringing Elegant Objects to .NET

**Speaker:** Dmitry Kurochkin
**Length:** 30 minutes
**Format:** spoken-delivery script

> Delivery marks used below:
> `[PAUSE]` — a real beat, about two seconds.
> `[LONG PAUSE]` — three or four seconds. Let it sit.
> **Bold** — vocal stress.
> `[SHOW: …]` — slide cue.

---

## 1. Opening — ≈ 1.5 min

Good afternoon, colleagues.

My name is Dmitry Kurochkin. I am a .NET developer, and I am the author and the main contributor of an ecosystem called Pure.

Before I say anything else, I want to be precise about what is mine and what is not. [PAUSE] The ideas I am going to show you today were not invented by me. They come from Elegant Objects — from Yegor Bugaenko. He is the one who did the thinking, the research, wrote the books and took the criticism for it.

What I did is smaller and more concrete. I took those ideas and I implemented them for .NET, in a form you can install from NuGet and use this afternoon. And in a few places, I read them differently than he does.

So the question I want to answer today is this. What does a .NET program actually look like if you take those ideas seriously — all the way down? Not down to your domain model. Not down to your service layer. [PAUSE] All the way down to primitives.

---

## 2. The premise — ≈ 3.5 min

Everything else in this talk rests on a single assumption, so let me put it on the table first.

**The purpose of a program is to transform data. And the result of a program is also data.**

Whatever your program is — a web service, a report generator, a trading system, or even a string concatenation function — you take data in one shape and you produce data in another shape. Everything else is machinery.

[PAUSE]

Now look at how we normally write that transformation. We write a sequence of instructions that *performs* it. We say: take this, do that, check this, assign that, return. The transformation exists only while the CPU is running. Before that, it is a recipe. After that, it is gone. What we have written down is not the transformation. It is a sequential set of data states for producing it.

So here is the alternative. [PAUSE] What if we wrote the transformation down **as a sequential set of object states**? Not the steps that compute the result — the result itself, in unevaluated form. A thing that already *is* the answer, and simply has not been asked yet.

Then a program stops being a sequence of instructions. A program becomes a **composition**. You build a graph of objects, each one standing for one small transformation, and at the very end — once — you read a field, and the whole thing collapses into a value.

[SHOW: the five principles]

Everything in Pure comes out of five rules. I will spend the rest of the talk showing you how each one is actually achieved in code.

**One.** A component has only fields. No methods.
**Two.** Execution logic — the transformation of data — is achieved only through composition of objects.
**Three.** Immutability. Not "mostly immutable." Not "immutable by convention." Iron immutability.
**Four.** Lazy evaluation. Nothing runs except `new`.
**Five.** Deterministic hash codes.

Let me expand the first one, because it is where I read Elegant Objects differently, and I would rather say it myself than have you notice it later.

Elegant Objects argues against exposing state. Pure is built almost entirely out of read-only fields. That looks like a contradiction, and it deserves a proper explanation.

In classical .NET, a field holds a value and an accessor hands it to you. That is the thing worth objecting to — the object becomes a bag you reach into. In Pure, a field is not an accessor bolted onto stored state. **The field is the state.** And that state is established in exactly one place: [PAUSE] the constructor, by composing other objects, and delegating from one constructor to the next.

That is the whole design. The rest of this talk is what falls out of it.

---

## 3. Why .NET stops you — ≈ 6 min

Let us try to obey rule number two with the tools .NET already gives us. Composition of objects. Fine. Let us compose two strings.

[SHOW: the BCL declarations]

```csharp
public sealed class String : IComparable, IEnumerable<char>, ...

public readonly struct Int32 : IComparable, IEquatable<int>, ...

public readonly struct Boolean : IComparable, IEquatable<bool>, ...
```

`System.String` is `sealed`. And every numeric primitive is a `struct`, which cannot be inherited either.

[PAUSE]

You cannot subtype them. You cannot decorate them. You cannot write a class that *is* a string and that computes its characters when asked. The door is welded shut — and it is not just these three. It is every primitive in the framework. All sealed, all concrete.

Now, that alone would be survivable. The real damage is one layer up. **Every operation in the base class library is written against those concrete types.** `string.Concat` takes strings and returns a string. `Math.Abs` takes a double and returns a double. There is no seam anywhere. There is nothing to plug into.

And this is the thing I want you to take away even if you never install a single one of the ecosystem packages. [PAUSE] The reason .NET developers write functional-style code is not that they weighed functions against objects and chose functions. It is that **the SDK gives them concrete sealed types and static operations, and there is nothing else to hold on to.** The language pushes you there. You reach for LINQ and lambdas because the alternative — real composition — was structurally removed before you sat down.

So if you want composition, you cannot fix it with a library. A library sits on top of the problem. You have to go underneath it. You have to re-found the primitives themselves.

[SHOW: the abstractions]

Here is the entire foundation. Nine interfaces.

```csharp
public interface IBool
{
    public bool BoolValue { get; }
}

public interface INumber<out T>
    where T : System.Numerics.INumber<T>
{
    public T NumberValue { get; }
}

public interface IString : IEnumerable<IChar>
{
    public string TextValue { get; }
}
```

Nine interfaces, and **not one method among them.** Not one. Only fields.

Look at `INumber`. It is covariant, and it is constrained to the framework's own numeric interface, so it covers every numeric type there is — one abstraction, not fifteen. And `IString` is an `IEnumerable<IChar>`. A string is a sequence of characters, and in Pure the characters are objects too.

Now look closer, because one thing on that slide is not like the others. `TextValue` returns a `System.String` — a real, concrete, framework string. [PAUSE] That field is not there because I wanted it. It is there because it **has to be**. It is the bridge point: the single place where the ecosystem touches .NET and hands back something the framework can understand.

Every one of these interfaces has exactly one — `BoolValue`, `NumberValue`, `TextValue`. I call them native fields. And the rule about them is absolute: **a native field is always computed, never stored.** It is the point where the composition finally runs.

Which makes the date interesting, because it does not have one.

```csharp
public interface IDate
{
    public INumber<ushort> Day { get; }
    public INumber<ushort> Month { get; }
    public INumber<ushort> Year { get; }
}
```

[PAUSE]

`IDate` has **no value**. Nothing to unwrap, no bridge back to `DateTime`. A date is not a thing that has a value — a date **is** three numbers. And each of those numbers is itself an abstraction that may not have computed yet.

Here is what you build on top of that:

```csharp
public sealed record Millennium : IDate
{
    public Millennium()
    {
        Day = new UShort(1);
        Month = new UShort(1);
        Year = new UShort(2000);
    }

    public INumber<ushort> Day { get; }
    public INumber<ushort> Month { get; }
    public INumber<ushort> Year { get; }
}
```

And you use it like this:

```csharp
IDate birthday = new Millennium();
```

Look at what that gives you. Not a `DateTime` with a magic value in it, and not a constant buried in some static helper. [PAUSE] A **named** type. `Millennium`. It has a name, so it explains itself at the call site. It is reusable anywhere an `IDate` is accepted. It is testable entirely on its own. And its state is fixed at construction and can never become anything else.

That is what I mean by composition starting at the bottom. If your primitives are opaque, you can compose your domain objects all you like — the composition stops the moment you touch a `string`. Here it never stops.

---

## 4. The constructor is where the program is written — ≈ 5.5 min

Now the mechanic. This is the centre of the talk, so I want to be very exact about it.

Rule one says no methods. So where does the work go?

The naive answer is "into the fields." That is half true, and it is the half that misleads people. Here is the real rule.

**The transformation lives in the constructors.**

Every component has one primary constructor. That primary constructor takes the values of the fields — whatever custom types they happen to be — and it does exactly one thing. It assigns them. No work. No validation. No evaluation of results. Assignment.

**Every other constructor is a conversion.** It takes some other shape of input, wraps it in objects, and delegates — either straight to the primary constructor, or through another constructor on the way. And that delegation chain, that little cascade of `: this(new Something(...))`, **is** the transformation.

```csharp
public Substring(IString source, INumber<ushort> length)
    : this(source, new Zero<ushort>(), length)
{ }

public Substring(IString source, INumber<ushort> startIndex, INumber<ushort> length)
{
    _source = source;
    _startIndex = startIndex;
    _length = length;
}
```

Two constructors. The bottom one is primary — three fields in, three assignments out. The top one is the convenience overload, and notice how it supplies its default. Not `0`. **`new Zero<ushort>()`.** Even the absent argument is an object.

Now the same pattern doing something more interesting.

```csharp
public sealed record WrappedString : IString
{
    private readonly IString _concatenated;

    public WrappedString(IString encloser, IString wrappedValue)
        : this(encloser, wrappedValue, encloser) { }

    public WrappedString(IString prefix, IString wrappedValue, IString suffix)
        : this(new ConcatenatedString(prefix, wrappedValue, suffix)) { }

    private WrappedString(IString concatenated)
    {
        _concatenated = concatenated;
    }

    public string TextValue => _concatenated.TextValue;
}
```

Three constructors, and the only one that touches a field is **private**. Every public entry point is a conversion that composes objects and hands them down. The two-argument version says "the same thing on both sides" by delegating with the encloser twice. The three-argument version does the actual work — and look at what "the actual work" is. [PAUSE] It builds a `ConcatenatedString`, which I will show you in a moment, and passes it along.

Nothing is wrapped. There is no wrapping code anywhere in that class. A wrapped string simply **holds a concatenation** and reports its text.

And that pattern is not confined to primitives. Here is a domain type built the same way.

```csharp
public sealed record TotalWithVat : INumber<decimal>
{
    private readonly INumber<decimal> _total;

    public TotalWithVat(INumber<decimal> net, INumber<decimal> rate)
        : this(new Sum<decimal>(net, new Product<decimal>(net, rate))) { }

    private TotalWithVat(INumber<decimal> total)
    {
        _total = total;
    }

    public decimal NumberValue => _total.NumberValue;
}
```

[PAUSE]

There is no arithmetic in that class. Not one operator. The rule "add the tax to the net amount" is written as a `Sum` of the net and a `Product` — a tree of objects, assembled in a constructor and standing still.

And notice what you got for free by naming it. `TotalWithVat` is a type. It goes anywhere an `INumber<decimal>` is accepted, it can be put inside another calculation, and you can test it by constructing it and reading one field.

[SHOW: `ConcatenatedString`]

```csharp
public sealed record ConcatenatedString : IString
{
    private readonly IEnumerable<IString> _parameters;

    public ConcatenatedString(params IEnumerable<IString> parameters)
    {
        _parameters = parameters;
    }

    public string TextValue => string.Concat(_parameters.Select(x => x.TextValue));
}
```

Now the point I want to land hard. [PAUSE] `ConcatenatedString` is not a helper. It is not a builder. It does not *produce* an `IString`.

**It is an `IString`.**

The component implements the interface and expresses the transformation at the same time — those are not two responsibilities, they are one. `new ConcatenatedString(a, b)` can be passed to anything that wants a string, stored in a field of type `IString`, put inside another `ConcatenatedString`, decorated, cached. It is a first-class citizen of the type system, and it is also an unperformed operation.

And now the consequence, which is rule four. [PAUSE] What did that constructor actually do? It assigned a field. That is all. **Nothing was concatenated.** No string exists. If you build a graph of ten thousand of these, you have allocated ten thousand small objects and performed exactly zero string operations.

Nothing runs except `new`.

So this is what a program actually looks like written this way.

```csharp
IString placeholder = new WrappedString(
    new LeftCurlyBracketString(),
    new RandomString(),
    new RightCurlyBracketString()
);

INumber<int> result = new Sum<int>(
    new Difference<int>(new Int(10), new Int(3)),
    new Product<int>(new Int(2), new Int(4))
);
```

Two statements, and both of them are declarations. No string was built. No arithmetic was performed. Not even the random string was generated — `RandomString` holds a `Lazy<string>` that has never been asked. [PAUSE] What you are looking at is the shape of the answer, not the answer.

And while these are on the screen, let me say something about the names, because it is a rule and not a habit.

**We name a component for what it is, not for what it does.** `ConcatenatedString`. `Sum`. `Difference`. `CurrentTime`. Not `StringConcatenator`, not `Calculate`, not `TimeProvider`. [PAUSE] A component is not an agent that performs an action — it is a value that stands for one. So it gets the name of the value: a sum, a difference, a wrapped string, the current time. Read either of those declarations out loud and you are reading a noun phrase, not a procedure. That is deliberate.

[LONG PAUSE]

So where does it ever end? If nothing computes, when does anything happen?

It happens at the native field. That is the whole answer.

The moment somebody reads `.TextValue` — or `.BoolValue`, or `.NumberValue` — the graph is walked, every deferred operation runs in order, and a concrete .NET value comes out the other side. Before that call, your entire program is inert. It is a description of an answer, holding still, having done nothing.

Your program is a graph of constructors. Then, once, at the very end, someone reads a native field — and the graph runs.

---

## 5. Determined hashes — ≈ 5.5 min

Principle five. The one that looked out of place next to the other four.

Here is my position, stated plainly. [PAUSE] **We do not use the built-in `GetHashCode` and `Equals`. At all. We use determined hashes only.**

And the reason is not taste. It is this:

**`GetHashCode` gives you a different answer the next time you run the same program.**

[LONG PAUSE]

Hash the same string in two processes and you get two different numbers. That is not a bug — it is a deliberate security measure, and it has been that way since .NET Core. But think about what it means for the word "identity." It means the framework's notion of sameness has a **lifetime shorter than your application's**. It cannot be written to a file. It cannot be sent over a network. It cannot be compared to yesterday. It survives exactly as long as one process, and then it is meaningless.

What I want instead is the hash of a **snapshot of an object's state** — a value that says "this is precisely what this object *was*," and that says the same thing tomorrow, on a different machine, in a different runtime.

There is a second problem, and it is worse. In .NET, `0.GetHashCode()` is zero. `false.GetHashCode()` is zero. The default char hashes to zero. [PAUSE] Three different values, from three different types, sharing one identity.

So here is what we use instead.

```csharp
public interface IDeterminedHash : IEnumerable<byte>;
```

That is the entire file. An identity is not an integer — it is **a sequence of bytes**. And it is lazy, like everything else: enumerate it and it computes; leave it alone and it costs you nothing.

Underneath, everything bottoms out in one place:

```csharp
public IEnumerator<byte> GetEnumerator()
{
    return SHA256.HashData([.. _bytes]).AsEnumerable().GetEnumerator();
}
```

SHA-256. Thirty-two bytes. Deterministic across processes, machines and runtimes — which is the entire point.

And every type gets a domain separator:

```csharp
private static readonly byte[] TypePrefix =
[
    0, 69, 151, 1, 4, 52, 46, 126, 159, 32, 211, 174, 149, 230, 168, 150,
];
```

Sixteen random bytes, unique per type, prepended before hashing. So the number zero, the boolean false, and the empty string produce three completely different digests. The collision the framework hands you for free is structurally impossible here.

There is a direct consequence, and it is not a small one. [PAUSE] If we do not use `GetHashCode`, **we cannot use the framework's collections.** A `Dictionary` or a `HashSet` asks its keys for a hash code, and our objects do not answer that question.

So the ecosystem ships its own collection wrappers, and they work through determined hashes instead. Two keys are the same when their byte sequences are the same — equality is decided by comparing two hundred and fifty-six bits, exactly. The integer underneath has not disappeared — but it has been **demoted from an identity to a bucket index**. It no longer answers "are these the same." It only answers "roughly where should I look," and collisions there are harmless, because the real comparison is exact.

And notice how such a collection learns what identity is — it is handed a `Func<T, IDeterminedHash>` in its constructor. The dictionary never asks its keys who they are. **The caller decides what identity means, and hands it in.**

[PAUSE]

One more thing, because determined hashes open a door I have not walked through yet. If objects are immutable, and their state can be determined exactly, then an allocator can ask a different question: has an object with this state already been allocated? If yes — there is no need to allocate again. Just return the existing reference.

---

## 6. Control flow is an object too — ≈ 5 min

Right. Data as objects, identity as bytes. Now the harder question, and the one I get asked in every hallway conversation: **what about `if`?**

You cannot compose a keyword. `if` is a statement. It executes, it branches, it does not exist as a value, and you cannot pass it to a constructor. So under rule one, `if` is illegal. And a language without branching is not a language.

Here is the answer.

```csharp
public sealed record StringChoice : IString
{
    private readonly IBool _condition;
    private readonly IString _valueOnTrue;
    private readonly IString _valueOnFalse;

    public StringChoice(IBool condition, IString valueOnTrue, IString valueOnFalse)
    {
        _condition = condition;
        _valueOnTrue = valueOnTrue;
        _valueOnFalse = valueOnFalse;
    }

    public string TextValue =>
        _condition.BoolValue ? _valueOnTrue.TextValue : _valueOnFalse.TextValue;
}
```

The condition is an object. Both branches are objects. And the choice itself — [PAUSE] look at the declaration — **the choice implements the target interface.**

That is the whole trick, and it is worth stating slowly. An `if` that produces a string **is** a string. It is not a control structure that yields a value. It is a value that happens to have a decision inside it.

Which means it composes.

```csharp
INumber<int> count = new Int(3);

IString label = new WrappedString(
    new LeftCurlyBracketString(),
    new StringChoice(
        new GreaterThanCondition<int>(count, new Zero<int>()),
        new String(count),
        new EmptyString()
    ),
    new RightCurlyBracketString()
);
```

The `WrappedString` has no idea there is a branch inside it. It was handed an `IString`, and that is all it will ever know. [PAUSE] You can put a choice inside a concatenation, inside another choice, inside a cache — and nothing downstream needs to know a decision is in there.

Now let me show you what happens when the target interface has more than one field. Remember `IDate` — three numbers, no native field.

```csharp
public sealed record DateChoice : IDate
{
    public DateChoice(IBool condition, IDate valueOnTrue, IDate valueOnFalse)
        : this(
            new NumberChoice<ushort>(condition, valueOnTrue.Day, valueOnFalse.Day),
            new NumberChoice<ushort>(condition, valueOnTrue.Month, valueOnFalse.Month),
            new NumberChoice<ushort>(condition, valueOnTrue.Year, valueOnFalse.Year)
        )
    { }

    private DateChoice(
        INumber<ushort> day,
        INumber<ushort> month,
        INumber<ushort> year
    )
    {
        Day = day;
        Month = month;
        Year = year;
    }

    public INumber<ushort> Day { get; }

    public INumber<ushort> Month { get; }

    public INumber<ushort> Year { get; }
}
```

[PAUSE]

Look at the structure, because it is the pattern from section four applied to branching. The public constructor performs no branching at all — it **composes three smaller choices**, one per field, and delegates. The private constructor assigns. And that is a rule I hold everywhere in the ecosystem: **only one constructor may assign fields, and it is private.** Every other constructor has to earn its way there by composing objects.

The result is that there is no single branch point. The `if` is **distributed across every field of the interface**, and each one is decided independently, at the moment it is read. If you only ever ask for the year, the day is never chosen. That branch never happens at all.

A statement cannot do that. A statement executes once, in one place, at one time. This is a decision that exists in three places and may partially never occur.

[LONG PAUSE]

Now `switch`. Same idea — a switch that returns a string implements `IString`, and it holds a parameter and a set of branches. But it has to answer one question that the choice never had to: **is this the same as that?**

And we already know how Pure answers that. Determined hashes.

```csharp
public StringSwitch(
    TSelector parameter,
    IEnumerable<KeyValuePair<TSelector, IString>> branches,
    Func<TSelector, IDeterminedHash> hashFactory
)
```

And this is the whole machine. Nothing else is in there.

```
new StringSwitch<IDayOfWeek>(today, branches, day => new DeterminedHash(day))

  parameter
    today ─────────────▶ hashFactory ─▶ 3f a1 c7 …  (32 bytes)
                                             │
                                             │  SequenceEqual
                                             ▼
  branches
    ├── new Monday()   ─▶ hashFactory ─▶ 9c 2e 44 …    ✗
    ├── new Saturday() ─▶ hashFactory ─▶ 3f a1 c7 …    ✓ ─▶ new String("Weekend")
    └── new Sunday()   ─▶ hashFactory ─▶ 71 0b 9d …    ✗
                                                            └── the only branch read
```

Hash the parameter. Hash each key. Keep the branch whose bytes match — and read only that one. The two that lost are still sitting there, unevaluated, and they will stay that way.

[PAUSE] Notice what is **not** in that picture. No `==`. No `Equals`. No virtual dispatch to ask an object about itself. The switch does not ask the objects whether they are equal. It is **told how to identify them** — `Func<TSelector, IDeterminedHash>`, handed in through the constructor — and it compares byte sequences.

[PAUSE] **Identity is a constructor parameter.**

---

## 7. What it costs, and what it buys — ≈ 4 min

Let me talk about what this is like to live with.

The first thing you notice is testing. [PAUSE] Everything is bounded — a component holds exactly the objects it was given. Everything is immutable, so there is no order of operations to reconstruct and no setup to unwind. Everything is thread-safe by construction. A test becomes what a test should be: build the object, read the field, compare. There is nothing else in the way.

The cost is at the border.

**Everything that meets classic .NET needs a wrapper.** The moment you touch a framework that was not written this way — and that is the entire rest of the .NET world — you need an adapter that unwraps an interface into a concrete type on the way out, and wraps it back on the way in.

The main thought is simple: [PAUSE] either you write the adapter, or you implement as much of your domain as possible inside the ecosystem, evaluate through a native field at the very edge, and carry on from there. The further out you push that edge, the fewer adapters you write.

The second thing is often stated as an objection, and I think it is stated wrongly. Yes — **it takes more mental power to design** this way. That part is true. But reading the design afterwards is simpler, better, and frankly more satisfying than reading the classic alternative. The difficulty is at the moment of writing, and it buys you clarity every time anybody reads it afterwards.

[PAUSE]

A word on how this is packaged, because it matters.

Pure is about seventy-five NuGet packages. One repository per package. One pipeline per package. Not a monolithic SDK — [PAUSE] and that is deliberate, not an accident of growth.

A monolithic SDK forces a decision on you: all of it or none of it. And with an idea this opinionated, "all of it" is not a reasonable thing to ask of anyone. So instead: you want lazy boolean algebra and nothing else? Take one package. You want deterministic hashing in an otherwise completely conventional codebase? Take one package — it is three lines of interface, and it does not drag a philosophy in behind it.

**You do not adopt the ecosystem. You take the two packages you need.**

[LONG PAUSE]

Let me finish where I started.

A program transforms data, and its result is data. If you believe that, then a program should not be a list of instructions that produce a transformation. **A program should be the transformation** — written down, composed, and standing there unperformed until someone needs it.

That is what all of this is for. Not the interfaces, not the hashes. Those are consequences. The idea is that you should be able to write your entire program as a composition of `new`, hand it to someone, and have it be **completely inert**.

And at the edges, Pure disappears completely. The wire sees a string. The database sees a column. The client sees a primitive. None of them can tell. All of this exists only inside the process, between the first constructor and the moment a native field is read.

**Your program is a graph of `new`. And everything that happens, happens once, at the end — when somebody finally reads a field.**

[PAUSE]

Thank you. The ecosystem is on NuGet and on GitHub under `kudima03`, and I would be glad to discuss any of it with you.

---

*Elegant Objects is the work of Yegor Bugaenko. Pure is an implementation of those ideas for .NET, with some divergences of my own.*
