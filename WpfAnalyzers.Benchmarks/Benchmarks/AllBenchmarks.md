``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410126 Hz, Resolution=293.2443 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2114.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.2114.0


```
 |                                   Method |            Mean |          Error |         StdDev |   Gen 0 |  Gen 1 | Allocated |
 |----------------------------------------- |----------------:|---------------:|---------------:|--------:|-------:|----------:|
 |   INPC001ImplementINotifyPropertyChanged |   198,147.57 ns |  4,788.2704 ns |  14,043.168 ns |  1.7090 | 1.7090 |    9672 B |
 | INPC002MutablePublicPropertyShouldNotify |    36,382.25 ns |  1,139.1951 ns |   3,323.086 ns |       - |      - |     244 B |
 |         INPC003NotifyWhenPropertyChanges | 2,799,411.79 ns | 90,715.6071 ns | 264,621.684 ns |       - |      - |   15424 B |
 |               INPC004UseCallerMemberName | 1,296,767.72 ns | 38,337.1295 ns | 111,222.964 ns |       - |      - |     272 B |
 |   INPC005CheckIfDifferentBeforeNotifying | 1,139,137.32 ns | 26,667.5946 ns |  78,211.439 ns |       - |      - |      32 B |
 |  INPC006UseObjectEqualsForReferenceTypes |   668,505.82 ns | 19,996.2820 ns |  58,645.634 ns |       - |      - |      32 B |
 |                INPC006UseReferenceEquals |   689,712.31 ns | 17,088.4152 ns |  50,385.578 ns |       - |      - |      32 B |
 |                    INPC007MissingInvoker |        35.79 ns |      0.7905 ns |       2.294 ns |  0.0045 | 0.0045 |      24 B |
 |               INPC008StructMustNotNotify |        36.10 ns |      0.9542 ns |       2.813 ns |  0.0045 | 0.0045 |      24 B |
 | INPC009DontRaiseChangeForMissingProperty | 2,665,653.46 ns | 87,003.3854 ns | 256,531.445 ns | 35.1563 |      - |  199586 B |
 |             INPC010SetAndReturnSameField |    45,908.75 ns |  1,373.1661 ns |   4,048.811 ns |  0.2441 | 0.2441 |    1572 B |
 |                        INPC011DontShadow |        36.53 ns |      1.2892 ns |       3.781 ns |  0.0045 | 0.0045 |      24 B |
 |                 INPC012DontUseExpression | 1,648,858.00 ns | 42,249.4841 ns | 123,243.728 ns |       - |      - |     256 B |
 |                         INPC013UseNameof | 1,676,187.82 ns | 33,735.0047 ns |  98,406.592 ns |  1.9531 | 1.9531 |   18304 B |
