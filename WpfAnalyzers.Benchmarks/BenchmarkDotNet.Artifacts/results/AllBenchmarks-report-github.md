``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410117 Hz, Resolution=293.2451 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                                                        Method |        Mean |      Error |      StdDev |      Median |         P95 |   Gen 0 | Allocated |
 |-------------------------------------------------------------- |------------:|-----------:|------------:|------------:|------------:|--------:|----------:|
 |                  WPF0001BackingFieldShouldMatchRegisteredName |   169.14 us |  3.1715 us |   2.9666 us |   167.37 us |   174.63 us |       - |      42 B |
 |                  WPF0002BackingFieldShouldMatchRegisteredName |   165.23 us |  2.6682 us |   2.4958 us |   165.47 us |   168.59 us |       - |      42 B |
 |                   WPF0003ClrPropertyShouldMatchRegisteredName |    68.92 us |  0.1650 us |   0.1288 us |    68.94 us |    69.09 us |       - |      41 B |
 |                     WPF0004ClrMethodShouldMatchRegisteredName |   391.44 us |  7.7655 us |  19.6244 us |   387.63 us |   425.18 us |       - |      44 B |
 |       WPF0005PropertyChangedCallbackShouldMatchRegisteredName |   149.65 us |  2.9921 us |   6.5045 us |   148.17 us |   159.85 us |       - |      42 B |
 |           WPF0006CoerceValueCallbackShouldMatchRegisteredName |   138.86 us |  1.2727 us |   0.9937 us |   138.60 us |   140.17 us |       - |      42 B |
 | WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName | 1,150.35 us | 22.5583 us |  29.3322 us | 1,153.07 us | 1,190.32 us |       - |    3600 B |
 |                    WPF0010DefaultValueMustMatchRegisteredType |   142.58 us |  2.8281 us |   6.7759 us |   140.49 us |   154.60 us |       - |      42 B |
 |                  WPF0011ContainingTypeShouldBeRegisteredOwner | 1,227.04 us | 24.2521 us |  47.8714 us | 1,222.41 us | 1,308.04 us |       - |    3600 B |
 |                   WPF0012ClrPropertyShouldMatchRegisteredType |    67.59 us |  0.1931 us |   0.1508 us |    67.55 us |    67.81 us |       - |      41 B |
 |                       WPF0013ClrMethodMustMatchRegisteredType |   380.21 us | 13.3504 us |  13.1118 us |   377.22 us |   405.19 us |       - |      44 B |
 |                          WPF0014SetValueMustUseRegisteredType | 1,281.62 us | 25.4566 us |  47.1855 us | 1,289.39 us | 1,355.93 us |       - |    3312 B |
 |              WPF0015RegisteredOwnerTypeMustBeDependencyObject | 3,336.23 us | 77.0954 us | 175.5853 us | 3,277.68 us | 3,877.38 us | 42.9688 |  296131 B |
 |                      WPF0016DefaultValueIsSharedReferenceType |   202.52 us |  4.0101 us |   8.1007 us |   201.89 us |   217.43 us |       - |      42 B |
 |                     WPF0030BackingFieldShouldBeStaticReadonly |   185.60 us |  1.1626 us |   0.9077 us |   185.50 us |   186.91 us |       - |      42 B |
 |                                             WPF0031FieldOrder |   154.51 us |  0.5698 us |   0.4449 us |   154.56 us |   155.06 us |       - |      42 B |
 |             WPF0032ClrPropertyGetAndSetSameDependencyProperty |    68.26 us |  1.3367 us |   1.6416 us |    67.66 us |    70.82 us |       - |     191 B |
 |                          WPF0040SetUsingDependencyPropertyKey | 1,195.74 us | 23.0505 us |  29.1515 us | 1,194.12 us | 1,248.23 us |       - |    3312 B |
 |                         WPF0041SetMutableUsingSetCurrentValue | 2,140.49 us | 42.4453 us |  82.7863 us | 2,111.44 us | 2,315.01 us |  3.9063 |   34048 B |
 |                         WPF0042AvoidSideEffectsInClrAccessors |   445.06 us |  8.8712 us |  14.8217 us |   443.36 us |   466.70 us |       - |      44 B |
 |                   WPF0043DontUseSetCurrentValueForDataContext | 3,347.89 us | 67.2573 us | 151.8110 us | 3,290.99 us | 3,679.43 us | 42.9688 |  296131 B |
 |                    WPF0050XmlnsPrefixMustMatchXmlnsDefinition |   144.72 us |  2.9796 us |   4.5501 us |   142.60 us |   152.19 us |       - |      42 B |
 |                WPF0051XmlnsDefinitionMustMapExistingNamespace |   107.04 us |  1.0050 us |   0.8392 us |   106.87 us |   108.39 us |  0.4883 |    3640 B |
 |                WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |   119.56 us |  2.7410 us |   2.5639 us |   119.37 us |   124.69 us |       - |      41 B |
