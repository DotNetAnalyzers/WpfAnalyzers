``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410117 Hz, Resolution=293.2451 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                   Method |     Mean |     Error |    StdDev |  Gen 0 | Allocated |
 |------------------------- |---------:|----------:|----------:|-------:|----------:|
 | RunOnWpfAnalyzersProject | 2.428 ms | 0.0564 ms | 0.1619 ms | 3.9063 |  33.25 KB |
