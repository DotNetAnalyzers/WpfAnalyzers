``` ini

BenchmarkDotNet=v0.11.0, OS=Windows 10.0.17134.228 (1803/April2018Update/Redstone4)
Intel Xeon CPU E5-2637 v4 3.50GHz (Max: 3.49GHz), 2 CPU, 16 logical and 8 physical cores
Frequency=3410080 Hz, Resolution=293.2483 ns, Timer=TSC
  [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3132.0
  DefaultJob : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3132.0


```
|                Method |     Mean |    Error |   StdDev |   Median | Allocated |
|---------------------- |---------:|---------:|---------:|---------:|----------:|
| RunOnValidCodeProject | 38.09 us | 3.040 us | 8.869 us | 35.04 us |       0 B |
