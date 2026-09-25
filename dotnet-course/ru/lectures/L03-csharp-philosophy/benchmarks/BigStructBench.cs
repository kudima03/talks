using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

// Three nested calls that the JIT is not allowed to inline, the way a value
// travels through a real call chain. Each level still reads the value after
// passing it on, so the JIT cannot hand the callee its own copy: with a last
// use instead, .NET 10 skips the copy and the struct wins.
[MemoryDiagnoser]
public class BigStructBench
{
    private readonly BigS _s = new() { A = 1, J = 10 };
    private readonly BigC _c = new() { A = 1, J = 10 };

    [Benchmark(Baseline = true)]
    public long Class() => C1(_c);

    [Benchmark]
    public long Struct() => S1(_s);

    [Benchmark]
    public long Boxed() => B1(_s);

    [MethodImpl(MethodImplOptions.NoInlining)]
    static long C1(BigC c) => C2(c) + c.B;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static long C2(BigC c) => C3(c) + c.C;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static long C3(BigC c) => c.A + c.J;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static long S1(BigS s) => S2(s) + s.B;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static long S2(BigS s) => S3(s) + s.C;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static long S3(BigS s) => s.A + s.J;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static long B1(object o) => B2(o) + ((BigS)o).B;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static long B2(object o) => B3(o) + ((BigS)o).C;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static long B3(object o) => ((BigS)o).A + ((BigS)o).J;
}

public struct BigS
{
    public long A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J;
}

public sealed class BigC
{
    public long A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J;
}
