``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical and 8 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4018.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4018.0), X64 RyuJIT


```
|                Method |     Mean |   Error |   StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------- |---------:|--------:|---------:|------:|------:|------:|----------:|
| RunOnValidCodeProject | 386.3 us | 7.68 us | 17.65 us |     - |     - |     - |         - |
