using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

// dotnet run -c Release -- --filter '*'
// The config only trims two columns the slides do not use; the benchmark
// classes stay exactly as the lecture shows them.
BenchmarkSwitcher
    .FromAssembly(typeof(Program).Assembly)
    .Run(args, DefaultConfig.Instance.HideColumns(Column.RatioSD, Column.AllocRatio));
