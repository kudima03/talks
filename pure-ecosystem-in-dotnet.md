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

## 1. Opening — 0:00 to 2:30

Good afternoon, colleagues.

My name is Dmitry Kurochkin. I am a .NET developer, and I am the author and the main contributor of an ecosystem called Pure.

Before I say anything else, I want to be precise about what is mine and what is not. [PAUSE] The ideas I am going to show you today are not mine. They come from Elegant Objects — from Yegor Bugaenko. He is the one who did the thinking. He wrote the books, he made the argument, he took the criticism for it.

What I did is smaller and more concrete. I took those ideas and I implemented part of them, for .NET, in a form you can install from NuGet this afternoon. Part of them. Not all. And in a few places, I read them differently than he does.

Let me name the biggest difference right at the start, because otherwise someone in this room will spend the whole talk waiting for me to admit it. [PAUSE] Elegant Objects argues against exposing state. Pure is built almost entirely out of read-only fields. That looks like a contradiction, and it is worth thirty seconds.

In classical .NET, a field holds a value and an accessor hands it to you. That is the thing worth objecting to — the object becomes a bag you reach into. In Pure, a field is not an accessor bolted onto stored state. The field **is** the state. And very often that state is a computation that has not happened yet. Reading it is not "give me your data." Reading it is "**now**." [PAUSE] It is the moment you ask the object to exist.

So: no methods. Only fields. Some of them are stored. Most of them are computed.

And the question I want to answer in the next twenty-seven minutes is this. What does a .NET program actually look like if you take that seriously — all the way down? Not down to your domain model. Not down to your service layer. [PAUSE] All the way down to `int`.

---

## 2. The premise — 2:30 to 5:30

Here is the assumption everything else rests on. If you disagree with this one sentence, you will disagree with the rest of the talk, and that is a perfectly reasonable place to get off.

**The purpose of a program is to transform data. And the result of a program is also data.**

That is it. Whatever your program is — a web service, a report generator, a trading system — you take data in one shape and you produce data in another shape. Everything else is machinery.

[PAUSE]

Now look at how we normally write that transformation. We write a sequence of instructions that *performs* it. We say: take this, do that, check this, assign that, return. The transformation exists only while the CPU is running. Before that, it is a recipe. After that, it is gone. What we have written down is not the transformation. It is a set of orders for producing it.

So here is the alternative. [PAUSE] What if we wrote the transformation down **as an object**? Not the steps that compute the result — the result itself, in unevaluated form. A thing that already *is* the answer, and simply has not been asked yet.

Then a program stops being a sequence of instructions. A program becomes a **composition**. You build a graph of objects, each one standing for one small transformation, and at the very end — once — you read a field, and the whole thing collapses into a value.

[SHOW: the five principles]

Everything in Pure comes out of five rules. I will spend the rest of the talk showing you how each one is actually achieved in code.

**One.** A component has only fields. No methods.
**Two.** Execution logic — the manipulation of data — is achieved only through composition of objects.
**Three.** Immutability. Not "mostly immutable." Not "immutable by convention." Iron immutability.
**Four.** Lazy evaluation. Nothing runs except `new`.
**Five.** Deterministic hash codes.

That last one is going to look out of place next to the other four. [PAUSE] By minute twenty you will see that it is the one holding the rest of them up.

---

## 3. Why .NET stops you, and why this had to be an ecosystem — 5:30 to 9:30

Let us try to obey rule number two with the tools .NET already gives us. Composition of objects. Fine. Let us compose two strings.

[SHOW: `System.String` declaration]

`System.String` is `sealed`.

[PAUSE]

You cannot subtype it. You cannot decorate it. You cannot write a class that *is* a string and that computes its characters when asked. The door is welded shut. And it is not just `String` — it is `Int32`, it is `Boolean`, it is `DateTime`, it is every primitive in the framework. All sealed, all concrete.

Now, that alone would be survivable. The real damage is one layer up. **Every operation in the base class library is written against those concrete types.** `string.Concat` takes strings and returns a string. `Math.Abs` takes a double and returns a double. There is no seam anywhere. There is nothing to plug into.

And this is the thing I want you to take away even if you never install a single one of my packages. [PAUSE] The reason .NET developers write functional-style code is not that they chose functions over objects. It is that **the SDK gives them concrete sealed types and static operations, and there is nothing else to hold on to.** The language pushes you there. You reach for LINQ and lambdas because the alternative — real composition — was structurally removed before you sat down.

So if you want composition, you cannot fix it with a library. A library sits on top of the problem. You have to go underneath it. You have to re-found the primitives themselves.

That is why Pure is an ecosystem and not an SDK.

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

Look at `INumber`. It is covariant, and it is constrained to the framework's own numeric interface, so it covers every numeric type there is — one abstraction, not fifteen. And look at `IString` — it is an `IEnumerable<IChar>`. A string is a sequence of characters, and in Pure the characters are objects too. It goes all the way down.

But my favourite one is the date.

```csharp
public interface IDate
{
    public INumber<ushort> Day { get; }
    public INumber<ushort> Month { get; }
    public INumber<ushort> Year { get; }
}
```

[PAUSE]

`IDate` has **no value**. There is no `DateValue` field on it, nothing to unwrap. A date is not a thing that has a value — a date **is** three numbers. And each of those numbers is itself an abstraction that might not have computed yet.

That is what I mean by composition starting at the bottom. If your primitives are opaque, you can compose your domain objects all you like — the composition stops the moment you touch a `string`. Here it never stops.

---

## 4. The constructor is where the program is written — 9:30 to 15:00

Now the mechanic. This is the centre of the talk, so I want to be very exact about it.

Rule one says no methods. So where does the work go?

The naive answer is "into the fields." That is half true, and it is the half that misleads people. Let me give you the real rule, the one I actually follow when I write these classes.

[SHOW: the two-constructor pattern]

**The transformation lives in the constructors.**

Every component has one primary constructor. That primary constructor takes the values of the fields — whatever custom types they happen to be — and it does exactly one thing. It assigns them. No work. No validation. No allocation of results. Assignment.

**Every other constructor is a conversion.** It takes some other shape of input, wraps it in objects, and delegates — either straight to the primary constructor, or through another constructor on the way. And that delegation chain, that little cascade of `: this(new Something(...))`, **is** the transformation.

Here is the smallest example.

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
public Int(int value)
    : this(new Lazy<int>(() => value)) { }

public Int(INumber<ushort> value)
    : this(new Lazy<int>(() => value.NumberValue)) { }

public Int(INumber<short> value)
    : this(new Lazy<int>(() => value.NumberValue)) { }

private Int(Lazy<int> lazyValue)
{
    _lazyValue = lazyValue;
}
```

Four constructors, and the only one that touches a field is **private**. Every public entry point is a conversion that wraps its input and hands it down. Widening a `ushort` to an `int` is not a cast written in a method body. It is a constructor delegating.

And here is my favourite one, because it is a whole logical operator written without a single line of logic.

```csharp
public sealed record NotEmptyCondition<T> : IBool
{
    private readonly IEnumerable<T> _values;

    public NotEmptyCondition(IEnumerable<T> values)
    {
        _values = values;
    }

    public bool BoolValue => new Not(new EmptyCondition<T>(_values)).BoolValue;
}
```

[PAUSE]

"Not empty" is not implemented. It is **composed**. `Not` of `Empty`. That is the entire class. Boolean algebra, done by putting one object inside another object.

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

Now, the point I want to land hard. [PAUSE] `ConcatenatedString` is not a helper. It is not a builder. It does not *produce* an `IString`.

**It is an `IString`.**

The component implements the interface and expresses the transformation at the same time — those are not two responsibilities, they are one. `new ConcatenatedString(a, b)` can be passed to anything that wants a string, stored in a field of type `IString`, put inside another `ConcatenatedString`, decorated, cached. It is a first-class citizen of the type system, and it is also an unperformed operation.

And now the consequence, which is rule four. [PAUSE] What did that constructor actually do? It assigned a field. That is all. **Nothing was concatenated.** No string exists. If you build a graph of ten thousand of these, you have allocated ten thousand small objects and performed exactly zero string operations.

Nothing runs except `new`.

Let me prove how far that goes, because this is the part that surprises people.

```csharp
public Date(INumber<ushort> day, INumber<ushort> month, INumber<ushort> year)
{
    Day = day;
    Month = month;
    Year = year;
    _validState = new DateValidState(day, month, year);
}

public INumber<ushort> Day =>
    _validState.BoolValue
        ? field
        : throw new ArgumentException($"{nameof(Day)} field is not valid");
```

Watch the constructor. It does not validate the date. It **composes a validity check** — `new DateValidState(...)` — and stores it, unperformed, like everything else.

Which means this: [PAUSE] constructing the thirty-second of January **does not throw.** The object is created, happily. Reading `Day` throws.

Even validation is deferred. Even correctness is lazy.

[LONG PAUSE]

So where does it ever end? If nothing computes, when does anything happen?

There is exactly one exit door in the whole ecosystem, and it is called materialization.

```csharp
public sealed record MaterializedString
{
    private readonly IString _value;

    public MaterializedString(IString value)
    {
        _value = value;
    }

    public string Value => _value.TextValue;
}
```

Notice what this class does **not** do. It does not implement `IString`. It implements nothing. It is not part of the composition — it is the wall at the end of it.

And notice the name of the field. `Value`. In an ecosystem where every field is called `BoolValue` or `TextValue` or `NumberValue`, this is the **only** place a field is simply called `Value`. That is not an accident of naming. That is the border.

Your entire program is a graph of constructors. Then, once, at the very end, someone reads a field — and the graph runs.

---

## 5. Control flow is an object too — 15:00 to 20:00

Right. I have shown you data as objects. Now the harder question, and the one I get asked in every hallway conversation: **what about `if`?**

You cannot compose a keyword. `if` is a statement. It executes, it branches, it does not exist as a value, and you cannot pass it to a constructor. So under rule one, `if` is illegal. And a language without branching is not a language.

Here is the answer.

[SHOW: `BoolChoice`]

```csharp
public sealed record BoolChoice : IBool
{
    public BoolChoice(IBool condition, IBool valueOnTrue, IBool valueOnFalse)
    {
        _condition = condition;
        _valueOnTrue = valueOnTrue;
        _valueOnFalse = valueOnFalse;
    }

    public bool BoolValue =>
        _condition.BoolValue ? _valueOnTrue.BoolValue : _valueOnFalse.BoolValue;
}
```

The condition is an object. Both branches are objects. And the choice itself — [PAUSE] look at the declaration — **the choice implements the target interface.**

That is the whole trick, and it is worth stating slowly. An `if` that produces a boolean **is** a boolean. An `if` that produces a string **is** a string. It is not a control structure that yields a value. It is a value that happens to have a decision inside it.

Which means it composes. You can put a choice inside a concatenation, inside another choice, inside a cache, and nothing downstream needs to know that a decision is in there.

Now let me show you what happens when the target interface has more than one field. Remember `IDate` — three numbers, no value.

```csharp
public sealed record DateChoice : IDate
{
    public INumber<ushort> Day =>
        _condition.BoolValue ? _valueOnTrue.Day : _valueOnFalse.Day;

    public INumber<ushort> Month =>
        _condition.BoolValue ? _valueOnTrue.Month : _valueOnFalse.Month;

    public INumber<ushort> Year =>
        _condition.BoolValue ? _valueOnTrue.Year : _valueOnFalse.Year;
}
```

[PAUSE]

There is no single branch point. The `if` is **smeared across every field of the interface**, and each one is decided independently, at the moment it is read. If you only ever ask for the year, the day is never chosen. The branch for the day never happens at all.

That is not something a statement can do. A statement executes once, in one place, at one time. This is a decision that exists in three places and may partially never occur.

[LONG PAUSE]

Now `switch`. Same idea — a switch that returns a string implements `IString`, and it holds a parameter and a set of branches. But when I sat down to write it, I hit a wall, and that wall is the last third of this talk.

A switch has to answer one question: **is this the same as that?**

So let me show you what a Pure object looks like at the bottom. Every single concrete type in the ecosystem ends with these six lines:

```csharp
public override int GetHashCode()
{
    throw new NotSupportedException();
}

public override string ToString()
{
    throw new NotSupportedException();
}
```

[PAUSE]

Every type. Hundreds of them. And because these are records, overriding `GetHashCode` like that takes structural equality down with it. It is deliberate. I destroyed equality on purpose, in every class I wrote.

So the switch cannot use `==`. It cannot use `Equals`. It cannot use `GetHashCode`. It has nothing.

Here is how it matches:

```csharp
public StringSwitch(
    TSelector parameter,
    IEnumerable<KeyValuePair<TSelector, IString>> branches,
    Func<TSelector, IDeterminedHash> hashFactory
)
```

```csharp
IEnumerable<byte> parameterHash = _hashFactory(_parameter);

IEnumerable<IString> filteredBranches = _branches
    .Where(x => parameterHash.SequenceEqual(_hashFactory(x.Key)))
    .Select(x => x.Value);
```

The switch does not ask the objects whether they are equal. It is **told how to identify them** — `Func<TSelector, IDeterminedHash>`, handed in through the constructor — and it compares byte sequences.

[PAUSE] **Identity is a constructor parameter.**

---

## 6. Deterministic hashes — 20:00 to 25:30

So let me defend the thing I just did, because destroying `Equals` and `GetHashCode` across an entire ecosystem needs more justification than "it felt right."

Here is my position, stated plainly. [PAUSE] **We do not use the built-in `GetHashCode` and `Equals`. At all. We use determined hashes only.**

And the reason is not taste. It is this:

**`GetHashCode` gives you a different answer the next time you run the same program.**

[LONG PAUSE]

Hash the same string in two processes and you get two different numbers. That is not a bug — it is a deliberate security measure, and it has been that way since .NET Core. But think about what it means for the word "identity." It means the framework's notion of sameness has a **lifetime shorter than your application's**. It cannot be written to a file. It cannot be sent over a network. It cannot be compared to yesterday. It survives exactly as long as one process, and then it is meaningless.

I do not want that. What I want is the hash of a **snapshot of an object's state** — a value that says "this is what this object *was*, precisely," and that says the same thing tomorrow, on a different machine, in a different runtime.

There is a second problem, and it is worse. In .NET, `0.GetHashCode()` is zero. `false.GetHashCode()` is zero. The default char hashes to zero. [PAUSE] Three different values from three different types, one identity between them.

So here is the replacement.

```csharp
public interface IDeterminedHash : IEnumerable<byte>;
```

That is the entire file. Three lines, one of them blank. An identity is not an integer — it is **a sequence of bytes**. And it is lazy, like everything else: enumerate it and it computes; leave it alone and it costs you nothing.

That interface is its own NuGet package. One three-line interface, published on its own. I will come back to why in a moment.

Underneath, everything bottoms out in one place:

```csharp
public IEnumerator<byte> GetEnumerator()
{
    return SHA256.HashData([.. _bytes]).AsEnumerable().GetEnumerator();
}
```

SHA-256. Thirty-two bytes. Deterministic across processes, machines and runtimes — which is the whole point.

And every type gets a domain separator:

```csharp
private static readonly byte[] TypePrefix =
[
    0, 69, 151, 1, 4, 52, 46, 126, 159, 32, 211, 174, 149, 230, 168, 150,
];
```

Sixteen random bytes, unique per type, prepended before hashing. So the number zero, the boolean false, and the empty string produce three completely different digests. The collision the framework hands you for free is structurally impossible here.

Then there is composition — because a hash of a compound object is a hash of its parts. And here the design makes a choice worth pointing out. When you aggregate child hashes, they are **sorted by their bytes** before being folded together. Which means an aggregate hash is a **set** hash, not a sequence hash. Two objects holding the same children in different order have the same identity. That is usually exactly what structural identity should mean.

Now, the honest part. [PAUSE] How do you put these things in a dictionary? Because `FrozenDictionary` wants an `int`, and I have thirty-two bytes and a type that throws when you ask it for a hash code.

```csharp
public bool Equals(T? x, T? y)
{
    return _determinedHashFactory(x!).SequenceEqual(_determinedHashFactory(y!));
}

public int GetHashCode(T obj)
{
    HashCode hash = new();
    hash.AddBytes(_determinedHashFactory(obj).ToArray());
    return hash.ToHashCode();
}

public override int GetHashCode()
{
    throw new NotSupportedException();
}
```

Two `GetHashCode`s in one class, doing opposite things. [PAUSE] Look at them for a second.

The one from the comparer interface is implemented. The one inherited from `Object` throws.

So no — I have not made the integer hash disappear. It is still there. But it has been **demoted from an identity to a bucket index**. It no longer answers "are these the same." It only answers "roughly where should I look." Equality is decided by comparing two hundred and fifty-six bits, exactly, and collisions in the bucket are harmless because the real comparison is exact.

And notice, once again, the shape of it: `Func<T, IDeterminedHash>`, passed into the constructor. The dictionary does not ask its keys who they are. **The caller decides what identity means, and hands it in.**

That is the reframe I would like you to leave with, more than any particular class I have shown you. The problem was never that `int` is too small. The problem is that **identity was inherited instead of chosen.** Every object in .NET is born already knowing how to answer "am I you," and it learned that answer from a base class that has never seen your domain.

[PAUSE]

One last thought on this, and I want to be careful to mark it as speculation rather than as something I have built. When an object is deeply immutable, and its identity is a deterministic function of its content, then its identity does not depend on where it lives. Two identical objects are not two objects. And an address derived from content never needs to move, because the content never changes. [PAUSE] That combination — content addressing plus iron immutability — makes a different kind of allocation mechanism **possible to build**. I have not built it. But the door is open, and it is only open because of the two rules.

---

## 7. What it costs, and what it buys — 25:30 to 30:00

I would rather tell you the price myself than have someone find it in the Q&A.

**Everything at the border needs a wrapper.**

Inside the process, this all works. Objects compose, nothing evaluates, identity is deterministic. But the moment you touch anything that was not written this way — and that is the entire rest of the .NET world — you need an adapter. Something that unwraps an interface into a concrete type on the way out, and wraps a concrete type back into an interface on the way in.

Every framework that expects `string` where you have `IString` needs one. Every framework that reconstructs objects for you needs one, because a model made of interfaces gives it nothing to construct. Every framework that inspects your types to describe them needs one, or it will describe your beautiful abstraction as an empty object.

These adapters are boring. They are numerous. They are usually nine lines each and there are dozens of them. And writing them is the recurring, unglamorous tax on the whole idea. [PAUSE] I do not have a clever answer for that. It is simply what it costs.

The second cost is harder to put in a repository. **This approach demands more mental power.** Reading a deeply composed constructor chain is genuinely harder than reading five imperative statements, the first few times. I am not going to pretend otherwise, and I am not apologising for it either. It is a price, and you get to decide whether the thing it buys is worth it.

[PAUSE]

So let me tell you what I think it buys, and how it is packaged.

Pure is about seventy-five NuGet packages. One repository per package. One pipeline per package. Not a monolithic SDK — [PAUSE] and that is deliberate, not an accident of growth.

A monolithic SDK forces a decision on you: all of it or none of it. And with an idea this opinionated, "all of it" is not a reasonable thing to ask of anyone. So instead: you want lazy boolean algebra and nothing else? Take one package. You want deterministic hashing in an otherwise completely conventional codebase? Take one package — it is three lines of interface and it does not drag a philosophy in behind it.

**You do not adopt the ecosystem. You take the two packages you need.**

That granularity is only survivable because every repository is generated from the same template — same build, same publish on a version tag, same gates. Ninety-eight per cent mutation score on every pull request. That is what makes seventy-five packages maintainable by one person instead of a full-time job in release management.

[LONG PAUSE]

Let me finish where I started.

A program transforms data, and its result is data. If you believe that, then a program should not be a list of instructions that produce a transformation. **A program should be the transformation** — written down, composed, and standing there unperformed until someone needs it.

That is what all of this is for. Not the interfaces, not the hashes, not the seventy-five packages. Those are consequences. The idea is that you should be able to write your entire program as a composition of `new`, hand it to someone, and have it be **completely inert** — a description of an answer, holding still, having done nothing.

And here is the part I find genuinely satisfying. At the edges, Pure disappears completely. The wire sees a string. The database sees a column. The client sees a primitive. None of them can tell. All of this exists only inside the process, between the first constructor and the moment of materialization.

**Your program is a graph of `new`. And everything that happens, happens once, at the end — when somebody finally reads a field.**

[PAUSE]

Thank you. The ecosystem is on NuGet and on GitHub under `kudima03`, and I would very much like to be argued with.

---

*Elegant Objects is the work of Yegor Bugaenko. Pure is a partial implementation of those ideas for .NET, with some divergences of my own.*
