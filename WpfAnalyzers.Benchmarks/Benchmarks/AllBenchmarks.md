``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410097 Hz, Resolution=293.2468 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                                                        Method |        Mean |      Error |      StdDev |      Median |  Gen 0 |  Gen 1 | Allocated |
 |-------------------------------------------------------------- |------------:|-----------:|------------:|------------:|-------:|-------:|----------:|
 |                  WPF0001BackingFieldShouldMatchRegisteredName |   292.56 us |  8.0200 us |  23.5214 us |   288.27 us |      - |      - |      44 B |
 |                  WPF0002BackingFieldShouldMatchRegisteredName |   423.62 us |  0.2718 us |   0.2270 us |   423.64 us |      - |      - |      44 B |
 |                   WPF0003ClrPropertyShouldMatchRegisteredName |   128.51 us |  0.7569 us |   1.1094 us |   128.63 us |      - |      - |      41 B |
 |                     WPF0004ClrMethodShouldMatchRegisteredName |   428.53 us |  8.5373 us |  21.5747 us |   424.86 us |      - |      - |      44 B |
 |       WPF0005PropertyChangedCallbackShouldMatchRegisteredName |   203.09 us |  4.0488 us |  11.0836 us |   203.99 us | 1.7090 | 0.2441 |   12264 B |
 |           WPF0006CoerceValueCallbackShouldMatchRegisteredName |   201.29 us |  4.3927 us |  12.8831 us |   200.79 us | 1.7090 | 0.2441 |   12264 B |
 | WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName | 1,337.04 us | 27.3124 us |  80.5313 us | 1,326.33 us |      - |      - |      48 B |
 |                    WPF0010DefaultValueMustMatchRegisteredType |   197.15 us |  4.1249 us |  12.1622 us |   196.12 us | 1.7090 | 0.2441 |   12264 B |
 |                  WPF0011ContainingTypeShouldBeRegisteredOwner | 1,318.74 us | 26.3270 us |  77.2124 us | 1,312.79 us |      - |      - |      48 B |
 |                   WPF0012ClrPropertyShouldMatchRegisteredType |    87.78 us |  1.7687 us |   5.2151 us |    87.18 us |      - |      - |      41 B |
 |                       WPF0013ClrMethodMustMatchRegisteredType |   430.47 us |  8.5954 us |  24.3836 us |   427.55 us |      - |      - |      44 B |
 |                          WPF0014SetValueMustUseRegisteredType | 1,411.00 us | 30.2973 us |  88.8568 us | 1,397.34 us |      - |      - |      48 B |
 |              WPF0015RegisteredOwnerTypeMustBeDependencyObject | 1,500.72 us | 34.4352 us | 101.5329 us | 1,489.69 us |      - |      - |    1744 B |
 |                      WPF0016DefaultValueIsSharedReferenceType |   129.16 us |  3.0809 us |   8.8892 us |   126.43 us |      - |      - |      41 B |
 |                               WPF0017MetadataMustBeAssignable | 1,393.11 us | 28.5579 us |  82.8516 us | 1,398.58 us |      - |      - |      48 B |
 |                     WPF0030BackingFieldShouldBeStaticReadonly |   329.67 us |  8.2705 us |  24.1254 us |   327.11 us |      - |      - |      44 B |
 |                                             WPF0031FieldOrder |   208.52 us |  5.1562 us |  15.2030 us |   205.57 us |      - |      - |      42 B |
 |             WPF0032ClrPropertyGetAndSetSameDependencyProperty |    82.89 us |  2.2614 us |   6.6322 us |    82.74 us |      - |      - |     191 B |
 |                          WPF0040SetUsingDependencyPropertyKey | 1,441.06 us | 30.9427 us |  89.2768 us | 1,422.65 us |      - |      - |      48 B |
 |                         WPF0041SetMutableUsingSetCurrentValue | 2,310.11 us | 58.3996 us | 169.4278 us | 2,290.73 us |      - |      - |   24256 B |
 |                         WPF0042AvoidSideEffectsInClrAccessors |   498.06 us | 11.2685 us |  33.2256 us |   493.00 us |      - |      - |      48 B |
 |                   WPF0043DontUseSetCurrentValueForDataContext | 1,401.83 us | 36.3689 us | 107.2345 us | 1,385.47 us |      - |      - |      64 B |
 |                    WPF0050XmlnsPrefixMustMatchXmlnsDefinition |   187.46 us |  3.7412 us |  10.7941 us |   186.39 us |      - |      - |      42 B |
 |                WPF0051XmlnsDefinitionMustMapExistingNamespace |   134.99 us |  3.5613 us |  10.3884 us |   132.48 us | 0.4883 |      - |    4000 B |
 |                WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |   132.28 us |  4.2245 us |  12.3897 us |   131.11 us |      - |      - |      42 B |
 |                                   WPF0060DocumentBackingField |   368.59 us |  9.6214 us |  28.2180 us |   365.00 us |      - |      - |      44 B |
 |                                WPF0061ClrMethodShouldHaveDocs |   676.74 us | 15.9031 us |  46.6410 us |   673.59 us |      - |      - |      48 B |
