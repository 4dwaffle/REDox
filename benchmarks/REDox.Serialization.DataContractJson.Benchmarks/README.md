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

## Implementation and limits

The converter caches whether the format/provider can use the fast parser. The shortcut accepts only the exact invariant format `yyyy-MM-ddTHH:mm:ssK` with the immutable invariant provider, a 19-character timestamp or a 20-character timestamp ending in `Z`, and `RoundtripKind` or `AdjustToUniversal | AssumeUniversal` styles. Style values are checked when reading so later style changes still use the correct path. Numeric offsets, other providers/calendars, fractions, other styles, and other formats use span-based `DateTime.ParseExact`. Invalid or mismatched input is not accepted by a more permissive ISO fallback.

Formatting uses `TryFormat` and `WriteString(ReadOnlySpan<char>)` with a 128-character stack buffer. Longer output retains the string fallback. General parsing decodes UTF-8 into a stack buffer up to 256 characters and rents a buffer for longer values, returning it in `finally`.

## Before and after results

| Operation | Format | Dates | Before mean | After mean | After / before | Before allocation | After allocation |
|---|---|---:|---:|---:|---:|---:|---:|
| Serialize | Default | 1 | 53.18 ns | 50.37 ns | 0.95x | 80 B | 80 B |
| Deserialize | Default | 1 | 86.66 ns | 80.60 ns | 0.93x | 128 B | 128 B |
| Serialize | Default | 64 | 1,237.72 ns | 1,184.46 ns | 0.96x | 3352 B | 3352 B |
| Deserialize | Default | 64 | 3,782.96 ns | 3,895.27 ns | 1.03x | 6680 B | 6680 B |
| Serialize | InvariantTimestamp | 1 | 102.35 ns | 88.95 ns | 0.87x | 136 B | 72 B |
| Deserialize | InvariantTimestamp | 1 | 47.10 ns | 54.74 ns | 1.16x | 32 B | 32 B |
| Serialize | InvariantTimestamp | 64 | 3,957.01 ns | 3,234.53 ns | 0.82x | 7064 B | 2968 B |
| Deserialize | InvariantTimestamp | 64 | 1,159.93 ns | 1,432.78 ns | 1.24x | 536 B | 536 B |
| Serialize | JapaneseLongDate | 1 | 125.87 ns | 91.73 ns | 0.73x | 104 B | 56 B |
| Deserialize | JapaneseLongDate | 1 | 282.01 ns | 311.13 ns | 1.10x | 32 B | 32 B |
| Serialize | JapaneseLongDate | 64 | 6,058.85 ns | 4,029.28 ns | 0.67x | 5104 B | 2032 B |
| Deserialize | JapaneseLongDate | 64 | 17,554.29 ns | 17,416.46 ns | 0.99x | 536 B | 536 B |

Both runs: Windows 11, Ryzen 9 9950X, .NET 10.0.12, BenchmarkDotNet 0.15.8, Release, one launch, three warmups and eight 250 ms measurement iterations. Same fixture and inputs; settings and framework correctness checks are outside measurement. Times and allocations are per array operation. Refreshed baseline: target main at e64ee50; optimized converter source: d2a35f8.

The Japanese provider is measured with Japanese ambient culture in both revisions so the baseline performs equivalent successful work. Regression tests separately verify differing ambient/provider cultures. Invariant timestamps use UTC values with AdjustToUniversal/AssumeUniversal styles.

The optimized timestamp parse remains about 16% slower for one date and 24% slower for 64 dates than the original path. Deserialization allocations now match the baseline in both configured formats. Serialization saves 64 bytes per invariant timestamp and 48 bytes per Japanese date. The default path has no source or allocation change; small timing differences across separate short runs should not be treated as an improvement. General format/provider/style combinations still require framework parsing; these measurements do not establish their speed. Settings creation and concurrent workloads are not measured.

## Why the shortcut matters

| Implementation | Timestamp deserialize, 64 dates | Allocation | Japanese deserialize, 64 dates | Allocation |
|---|---:|---:|---:|---:|
| Target main | 1,159.93 ns | 536 B | 17,554.29 ns | 536 B |
| Initial string converter | 8,823.07 ns | 4632 B | 17,516.37 ns | 3608 B |
| Spans only, shortcut disabled | 8,937.17 ns | 536 B | 17,580.08 ns | 536 B |
| Optimized converter | 1,432.78 ns | 536 B | 17,416.46 ns | 536 B |

Removing temporary strings fixes allocation overhead but does not remove the cost of general exact parsing. The span-only diagnostic uses the same optimized source with the fast-path condition disabled; to reproduce, change `_useIsoTimestamp &&` to `_useIsoTimestamp && false &&` for that run only. The initial string-converter results are from the earlier measurement of converter commit `025cfe2`. The final converter source is `d2a35f8`. All runs use the identical fixture and job settings.

Raw summaries and CSV exports: [baseline](results/before.md), [optimized](results/after.md), [initial string converter](results/string-converter.md), [spans only](results/span-only.md). These summaries retain environment information and timing error estimates. This is a short local run, not a throughput or concurrency benchmark.
