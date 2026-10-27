using BenchmarkDotNet.Running;
using REDox.Serialization.DataContractJson.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(DateTimeFormatBenchmarks).Assembly).Run(args);
