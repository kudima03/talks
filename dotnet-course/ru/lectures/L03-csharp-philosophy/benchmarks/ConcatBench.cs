using System.Text;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class ConcatBench
{
    private string _a = "Hello";
    private string _b = ", ";
    private string _c = "World";

    [Benchmark(Baseline = true)]
    public string Concat() => _a + _b + _c;

    [Benchmark]
    public string Builder() => new StringBuilder().Append(_a).Append(_b).Append(_c).ToString();

    [Benchmark]
    public string Join() => string.Join("", _a, _b, _c);
}
