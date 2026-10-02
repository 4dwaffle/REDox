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
| **Serialize**   | **Default**            | **1**     |     **53.18 ns** |   **1.572 ns** |  **0.822 ns** | **0.0046** |      **80 B** |
| Deserialize | Default            | 1     |     86.66 ns |   1.465 ns |  0.650 ns | 0.0075 |     128 B |
| **Serialize**   | **Default**            | **64**    |  **1,237.72 ns** |  **31.355 ns** | **16.399 ns** | **0.1999** |    **3352 B** |
| Deserialize | Default            | 64    |  3,782.96 ns |  29.379 ns | 15.366 ns | 0.3936 |    6680 B |
| **Serialize**   | **InvariantTimestamp** | **1**     |    **102.35 ns** |   **1.112 ns** |  **0.581 ns** | **0.0079** |     **136 B** |
| Deserialize | InvariantTimestamp | 1     |     47.10 ns |   0.772 ns |  0.404 ns | 0.0017 |      32 B |
| **Serialize**   | **InvariantTimestamp** | **64**    |  **3,957.01 ns** |  **53.246 ns** | **23.642 ns** | **0.4221** |    **7064 B** |
| Deserialize | InvariantTimestamp | 64    |  1,159.93 ns |  10.094 ns |  4.482 ns | 0.0277 |     536 B |
| **Serialize**   | **JapaneseLongDate**   | **1**     |    **125.87 ns** |   **2.527 ns** |  **1.321 ns** | **0.0061** |     **104 B** |
| Deserialize | JapaneseLongDate   | 1     |    282.01 ns |   3.043 ns |  1.591 ns | 0.0011 |      32 B |
| **Serialize**   | **JapaneseLongDate**   | **64**    |  **6,058.85 ns** |  **41.275 ns** | **18.327 ns** | **0.2876** |    **5104 B** |
| Deserialize | JapaneseLongDate   | 64    | 17,554.29 ns | 215.131 ns | 95.520 ns |      - |     536 B |
