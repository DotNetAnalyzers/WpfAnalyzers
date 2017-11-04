``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410097 Hz, Resolution=293.2468 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                   Method |     Mean |     Error |    StdDev |  Gen 0 |  Gen 1 | Allocated |
 |------------------------- |---------:|----------:|----------:|-------:|-------:|----------:|
 | RunOnWpfAnalyzersProject | 236.5 us | 0.8643 us | 0.7662 us | 2.9297 | 0.4883 |  19.37 KB |
