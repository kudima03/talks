using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class SumBench
{
    private readonly int[] _data = Enumerable.Range(0, 1_000).ToArray();

    [Benchmark(Baseline = true)]
    public int Loop()
    {
        var sum = 0;
        foreach (var x in _data)
            if (x % 2 == 0)
                sum += x;
        return sum;
    }

    [Benchmark]
    public int Linq() => _data.Where(x => x % 2 == 0).Sum();
}
