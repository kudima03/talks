using System.Text;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class LoopConcatBench
{
    private const int N = 1_000;

    [Benchmark(Baseline = true)]
    public string Builder()
    {
        var sb = new StringBuilder();
        for (var i = 0; i < N; i++)
            sb.Append("item;");
        return sb.ToString();
    }

    [Benchmark]
    public string PlusEquals()
    {
        var s = "";
        for (var i = 0; i < N; i++)
            s += "item;";
        return s;
    }
}
