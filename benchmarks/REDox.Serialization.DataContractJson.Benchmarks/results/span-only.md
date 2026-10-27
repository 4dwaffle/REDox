```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26300.9457)
AMD Ryzen 9 9950X 4.30GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]     : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4
  Job-HDUCDG : .NET 10.0.12 (10.0.12, 10.0.1226.42308), X64 RyuJIT x86-64-v4

IterationCount=8  IterationTime=250ms  LaunchCount=1
WarmupCount=3

```
| Method      | Format             | Count | Mean         | Error      | StdDev    | Gen0   | Allocated |
|------------ |------------------- |------ |-------------:|-----------:|----------:|-------:|----------:|
| **Serialize**   | **Default**            | **1**     |     **57.83 ns** |   **2.134 ns** |  **1.116 ns** | **0.0047** |      **80 B** |
| Deserialize | Default            | 1     |     81.63 ns |   2.732 ns |  1.429 ns | 0.0074 |     128 B |
| **Serialize**   | **Default**            | **64**    |  **1,215.42 ns** |  **15.393 ns** |  **6.835 ns** | **0.1989** |    **3352 B** |
| Deserialize | Default            | 64    |  3,937.85 ns |  48.999 ns | 21.756 ns | 0.3959 |    6680 B |
| **Serialize**   | **InvariantTimestamp** | **1**     |     **93.16 ns** |   **1.844 ns** |  **0.965 ns** | **0.0041** |      **72 B** |
| Deserialize | InvariantTimestamp | 1     |    180.92 ns |   1.488 ns |  0.778 ns | 0.0015 |      32 B |
| **Serialize**   | **InvariantTimestamp** | **64**    |  **3,238.89 ns** |  **14.906 ns** |  **7.796 ns** | **0.1684** |    **2968 B** |
| Deserialize | InvariantTimestamp | 64    |  8,937.17 ns |  46.553 ns | 24.348 ns |      - |     536 B |
| **Serialize**   | **JapaneseLongDate**   | **1**     |     **94.54 ns** |   **0.928 ns** |  **0.485 ns** | **0.0031** |      **56 B** |
| Deserialize | JapaneseLongDate   | 1     |    288.96 ns |   5.158 ns |  2.290 ns | 0.0011 |      32 B |
| **Serialize**   | **JapaneseLongDate**   | **64**    |  **4,006.03 ns** |  **90.304 ns** | **47.231 ns** | **0.1110** |    **2032 B** |
| Deserialize | JapaneseLongDate   | 64    | 17,580.08 ns | 199.851 ns | 88.735 ns |      - |     536 B |
