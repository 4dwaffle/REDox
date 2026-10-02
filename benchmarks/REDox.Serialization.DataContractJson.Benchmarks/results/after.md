```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26300.9457)
AMD Ryzen 9 9950X 4.30GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  Job-HDUCDG : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

IterationCount=8  IterationTime=250ms  LaunchCount=1
WarmupCount=3

```
| Method      | Format             | Count | Mean         | Error     | StdDev    | Gen0   | Allocated |
|------------ |------------------- |------ |-------------:|----------:|----------:|-------:|----------:|
| **Serialize**   | **Default**            | **1**     |     **50.37 ns** |  **0.407 ns** |  **0.213 ns** | **0.0046** |      **80 B** |
| Deserialize | Default            | 1     |     80.60 ns |  0.992 ns |  0.519 ns | 0.0076 |     128 B |
| **Serialize**   | **Default**            | **64**    |  **1,184.46 ns** | **19.268 ns** |  **8.555 ns** | **0.1984** |    **3352 B** |
| Deserialize | Default            | 64    |  3,895.27 ns | 34.197 ns | 17.886 ns | 0.3980 |    6680 B |
| **Serialize**   | **InvariantTimestamp** | **1**     |     **88.95 ns** |  **0.709 ns** |  **0.371 ns** | **0.0040** |      **72 B** |
| Deserialize | InvariantTimestamp | 1     |     54.74 ns |  0.404 ns |  0.179 ns | 0.0018 |      32 B |
| **Serialize**   | **InvariantTimestamp** | **64**    |  **3,234.53 ns** | **43.269 ns** | **19.212 ns** | **0.1673** |    **2968 B** |
| Deserialize | InvariantTimestamp | 64    |  1,432.78 ns | 21.056 ns |  9.349 ns | 0.0288 |     536 B |
| **Serialize**   | **JapaneseLongDate**   | **1**     |     **91.73 ns** |  **0.834 ns** |  **0.370 ns** | **0.0030** |      **56 B** |
| Deserialize | JapaneseLongDate   | 1     |    311.13 ns |  5.342 ns |  2.794 ns | 0.0012 |      32 B |
| **Serialize**   | **JapaneseLongDate**   | **64**    |  **4,029.28 ns** | **71.091 ns** | **37.182 ns** | **0.1127** |    **2032 B** |
| Deserialize | JapaneseLongDate   | 64    | 17,416.46 ns | 57.000 ns | 29.812 ns |      - |     536 B |
