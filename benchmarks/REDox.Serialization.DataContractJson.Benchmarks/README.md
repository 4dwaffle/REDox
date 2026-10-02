# DataContract JSON date format benchmarks

These benchmarks call REDox's DataContract JSON adapter with cached serializer settings and arrays of 1 or 64 `DateTime` values. They measure serialization to a string and deserialization from a string, including allocated bytes per operation. Settings construction, payload generation, and framework comparisons happen outside measurement.

The cases exercise the paths changed by the date converter:

- `Default`: no `DateTimeFormat`; a control for the existing Microsoft JSON date path.
- `InvariantTimestamp`: `yyyy-MM-ddTHH:mm:ssK` with an invariant provider and `AdjustToUniversal | AssumeUniversal` parsing styles. The UTC payload gives both revisions identical successful results.
- `JapaneseLongDate`: `D` with the Japanese provider. This exercises provider-dependent formatting and `ParseExact` deserialization.

The ambient culture is explicitly `ja-JP` in both runs. The baseline ignores the configured provider, so a different ambient culture would produce different output or throw when reading Japanese long dates. Matching the ambient culture makes this a performance comparison of equivalent work; it does not demonstrate the correctness fix. The regression tests cover a Japanese provider with a different ambient culture and an unspecified date requiring parsing styles.

Each setup compares serialization, deserialized values, and `DateTime.Kind` against the framework `DataContractJsonSerializer` and fails if they differ.

Run in Release mode:

```powershell
dotnet run -c Release --project benchmarks/REDox.Serialization.DataContractJson.Benchmarks -- --filter '*DateTimeFormatBenchmarks*' --job short --warmupCount 3 --iterationCount 8 --iterationTime 250 --artifacts BenchmarkDotNet.Artifacts/date-format
```

For a before/after comparison, copy this benchmark project unchanged into a checkout of the target branch and run the same command there. Run the revisions sequentially on the same machine. Compare rows with the same method, format, and count; timings and allocations are per array operation, not per date.

The checked-in results use target commit `e64ee501c18356a7812d68eab0a83b915444d2fd` as the baseline and converter commit `025cfe2` as the candidate. Raw BenchmarkDotNet summaries retain timing error estimates and environment information. This is a short local run, not a throughput or concurrency benchmark.


## Before and after results

| Operation | Format | Dates | Before mean | After mean | After / before | Before allocation | After allocation |
|---|---|---:|---:|---:|---:|---:|---:|
| Serialize | Default | 1 | 56.31 ns | 50.56 ns | 0.90x | 80 B | 80 B |
| Deserialize | Default | 1 | 84.56 ns | 81.56 ns | 0.96x | 128 B | 128 B |
| Serialize | Default | 64 | 1,214.94 ns | 1,210.68 ns | 1.00x | 3352 B | 3352 B |
| Deserialize | Default | 64 | 3,823.04 ns | 3,865.82 ns | 1.01x | 6680 B | 6680 B |
| Serialize | InvariantTimestamp | 1 | 105.74 ns | 99.22 ns | 0.94x | 136 B | 136 B |
| Deserialize | InvariantTimestamp | 1 | 48.08 ns | 182.64 ns | 3.80x | 32 B | 96 B |
| Serialize | InvariantTimestamp | 64 | 3,977.20 ns | 3,445.20 ns | 0.87x | 7064 B | 7064 B |
| Deserialize | InvariantTimestamp | 64 | 1,154.43 ns | 8,823.07 ns | 7.64x | 536 B | 4632 B |
| Serialize | JapaneseLongDate | 1 | 128.15 ns | 98.18 ns | 0.77x | 104 B | 104 B |
| Deserialize | JapaneseLongDate | 1 | 304.10 ns | 287.20 ns | 0.94x | 32 B | 80 B |
| Serialize | JapaneseLongDate | 64 | 6,083.37 ns | 4,154.85 ns | 0.68x | 5104 B | 5104 B |
| Deserialize | JapaneseLongDate | 64 | 18,290.53 ns | 17,516.37 ns | 0.96x | 536 B | 3608 B |

Both runs: Windows 11, Ryzen 9 9950X, .NET 10.0.12, BenchmarkDotNet 0.15.8, Release, one launch, three warmups and eight 250 ms measurement iterations. Same fixture and inputs; settings and framework correctness checks are outside measurement. Times and allocations are per array operation. Baseline: target main at e64ee50; candidate: converter at 025cfe2.

The Japanese provider is measured with Japanese ambient culture in both revisions so the baseline performs equivalent successful work. Regression tests separately verify differing ambient/provider cultures. Invariant timestamps use UTC values with AdjustToUniversal/AssumeUniversal styles.

Invariant timestamp deserialization is 3.80x slower for one date and 7.64x slower for 64 dates, adding 64 bytes per date. Japanese long-date deserialization adds 48 bytes per date. Serialization allocations are unchanged. The default path has no source or allocation change; small timing differences in these separate short local runs should not be treated as an improvement. These measurements do not include settings creation or concurrent workloads.

Raw summaries: [before](results/before.md), [after](results/after.md). CSV exports are alongside them.
