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
| **Serialize**   | **Default**            | **1**     |     **50.56 ns** |   **0.546 ns** |  **0.285 ns** | **0.0046** |      **80 B** |
| Deserialize | Default            | 1     |     81.56 ns |   0.578 ns |  0.257 ns | 0.0074 |     128 B |
| **Serialize**   | **Default**            | **64**    |  **1,210.68 ns** |   **8.592 ns** |  **4.494 ns** | **0.1986** |    **3352 B** |
| Deserialize | Default            | 64    |  3,865.82 ns |  37.857 ns | 16.809 ns | 0.3977 |    6680 B |
| **Serialize**   | **InvariantTimestamp** | **1**     |     **99.22 ns** |   **2.288 ns** |  **1.197 ns** | **0.0080** |     **136 B** |
| Deserialize | InvariantTimestamp | 1     |    182.64 ns |   4.563 ns |  2.386 ns | 0.0052 |      96 B |
| **Serialize**   | **InvariantTimestamp** | **64**    |  **3,445.20 ns** |  **42.170 ns** | **18.724 ns** | **0.4120** |    **7064 B** |
| Deserialize | InvariantTimestamp | 64    |  8,823.07 ns | 127.192 ns | 66.524 ns | 0.2455 |    4632 B |
| **Serialize**   | **JapaneseLongDate**   | **1**     |     **98.18 ns** |   **2.273 ns** |  **1.189 ns** | **0.0059** |     **104 B** |
| Deserialize | JapaneseLongDate   | 1     |    287.20 ns |   4.812 ns |  2.517 ns | 0.0046 |      80 B |
| **Serialize**   | **JapaneseLongDate**   | **64**    |  **4,154.85 ns** |  **15.859 ns** |  **8.295 ns** | **0.2983** |    **5104 B** |
| Deserialize | JapaneseLongDate   | 64    | 17,516.37 ns | 134.488 ns | 59.714 ns | 0.2095 |    3608 B |
