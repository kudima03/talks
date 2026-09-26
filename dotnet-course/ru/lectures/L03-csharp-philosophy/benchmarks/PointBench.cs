using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class PointBench
{
    private const int N = 1_000_000;

    [Benchmark(Baseline = true)]
    public long Struct()
    {
        var points = new PointS[N];
        for (var i = 0; i < N; i++)
            points[i] = new PointS { X = i, Y = i };

        long sum = 0;
        foreach (var p in points)
            sum += p.X + p.Y;
        return sum;
    }

    [Benchmark]
    public long Class()
    {
        var points = new PointC[N];
        for (var i = 0; i < N; i++)
            points[i] = new PointC { X = i, Y = i };

        long sum = 0;
        foreach (var p in points)
            sum += p.X + p.Y;
        return sum;
    }
}

public struct PointS
{
    public int X;
    public int Y;
}

public class PointC
{
    public int X;
    public int Y;
}
