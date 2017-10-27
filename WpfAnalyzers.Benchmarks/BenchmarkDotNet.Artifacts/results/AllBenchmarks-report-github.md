``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410087 Hz, Resolution=293.2477 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                                                        Method |        Mean |     Error |     StdDev |      Median |  Gen 0 |  Gen 1 | Allocated |
 |-------------------------------------------------------------- |------------:|----------:|-----------:|------------:|-------:|-------:|----------:|
 |                  WPF0001BackingFieldShouldMatchRegisteredName |   266.64 us |  5.669 us |  16.627 us |   264.51 us |      - |      - |      44 B |
 |                  WPF0002BackingFieldShouldMatchRegisteredName |   262.01 us |  5.210 us |  15.199 us |   259.93 us |      - |      - |      44 B |
 |                   WPF0003ClrPropertyShouldMatchRegisteredName |    85.28 us |  2.016 us |   5.880 us |    85.17 us |      - |      - |      41 B |
 |                     WPF0004ClrMethodShouldMatchRegisteredName |   388.89 us |  7.565 us |  10.606 us |   389.98 us |      - |      - |      44 B |
 |       WPF0005PropertyChangedCallbackShouldMatchRegisteredName |   180.56 us |  3.599 us |  10.613 us |   179.01 us | 1.7090 | 0.2441 |   11422 B |
 |           WPF0006CoerceValueCallbackShouldMatchRegisteredName |   185.61 us |  3.685 us |  10.689 us |   185.82 us | 1.7090 | 0.2441 |   11422 B |
 | WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName | 1,191.84 us | 25.391 us |  74.467 us | 1,174.94 us |      - |      - |      48 B |
 |                    WPF0010DefaultValueMustMatchRegisteredType |   188.42 us |  3.893 us |  11.479 us |   186.85 us | 1.7090 | 0.2441 |   11422 B |
 |                  WPF0011ContainingTypeShouldBeRegisteredOwner | 1,295.90 us | 28.116 us |  82.459 us | 1,291.92 us |      - |      - |      48 B |
 |                   WPF0012ClrPropertyShouldMatchRegisteredType |    77.41 us |  1.575 us |   4.122 us |    75.16 us |      - |      - |      41 B |
 |                       WPF0013ClrMethodMustMatchRegisteredType |   387.50 us |  7.701 us |  20.016 us |   378.87 us |      - |      - |      44 B |
 |                          WPF0014SetValueMustUseRegisteredType | 1,406.49 us | 29.078 us |  84.361 us | 1,402.99 us |      - |      - |    3184 B |
 |              WPF0015RegisteredOwnerTypeMustBeDependencyObject | 1,259.49 us | 28.875 us |  84.686 us | 1,242.64 us |      - |      - |    1328 B |
 |                      WPF0016DefaultValueIsSharedReferenceType |   127.77 us |  3.118 us |   9.194 us |   127.24 us |      - |      - |      41 B |
 |                               WPF0017MetadataMustBeAssignable | 1,314.22 us | 34.936 us | 103.010 us | 1,297.55 us |      - |      - |    3056 B |
 |                     WPF0030BackingFieldShouldBeStaticReadonly |   312.47 us |  7.827 us |  23.079 us |   309.99 us |      - |      - |      44 B |
 |                                             WPF0031FieldOrder |   191.42 us |  4.971 us |  14.658 us |   187.80 us |      - |      - |      42 B |
 |             WPF0032ClrPropertyGetAndSetSameDependencyProperty |    83.20 us |  1.890 us |   5.544 us |    83.14 us |      - |      - |     191 B |
 |                          WPF0040SetUsingDependencyPropertyKey | 1,403.54 us | 33.524 us |  98.846 us | 1,379.73 us |      - |      - |    3184 B |
 |                         WPF0041SetMutableUsingSetCurrentValue | 2,300.22 us | 55.529 us | 162.857 us | 2,290.68 us |      - |      - |   30434 B |
 |                         WPF0042AvoidSideEffectsInClrAccessors |   473.10 us | 13.265 us |  39.111 us |   463.36 us |      - |      - |      44 B |
 |                   WPF0043DontUseSetCurrentValueForDataContext | 1,363.74 us | 28.768 us |  83.919 us | 1,358.05 us |      - |      - |    1616 B |
 |                    WPF0050XmlnsPrefixMustMatchXmlnsDefinition |   171.74 us |  3.657 us |  10.611 us |   170.46 us |      - |      - |      42 B |
 |                WPF0051XmlnsDefinitionMustMapExistingNamespace |   128.40 us |  3.448 us |  10.059 us |   128.45 us | 0.4883 |      - |    3784 B |
 |                WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |   130.55 us |  3.320 us |   9.738 us |   129.70 us |      - |      - |      42 B |
