``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410097 Hz, Resolution=293.2468 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                                                        Method |        Mean |      Error |     StdDev |      Median |   Gen 0 | Allocated |
 |-------------------------------------------------------------- |------------:|-----------:|-----------:|------------:|--------:|----------:|
 |                  WPF0001BackingFieldShouldMatchRegisteredName |   175.47 us |   3.988 us |  11.378 us |   174.94 us |       - |      42 B |
 |                  WPF0002BackingFieldShouldMatchRegisteredName |   179.07 us |   4.707 us |  13.729 us |   177.91 us |       - |      42 B |
 |                   WPF0003ClrPropertyShouldMatchRegisteredName |    81.03 us |   2.377 us |   6.933 us |    81.32 us |       - |      41 B |
 |                     WPF0004ClrMethodShouldMatchRegisteredName |   480.45 us |  33.730 us |  99.453 us |   440.23 us |       - |      44 B |
 |       WPF0005PropertyChangedCallbackShouldMatchRegisteredName |   154.67 us |   3.074 us |   8.819 us |   155.01 us |       - |      42 B |
 |           WPF0006CoerceValueCallbackShouldMatchRegisteredName |   157.09 us |   3.730 us |  10.939 us |   155.95 us |       - |      42 B |
 | WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName | 1,251.89 us |  35.891 us | 105.826 us | 1,236.75 us |       - |    3600 B |
 |                    WPF0010DefaultValueMustMatchRegisteredType |   164.01 us |   3.580 us |  10.386 us |   162.54 us |       - |      42 B |
 |                  WPF0011ContainingTypeShouldBeRegisteredOwner | 3,663.34 us |  84.014 us | 247.719 us | 3,670.19 us | 42.9688 |  294595 B |
 |                   WPF0012ClrPropertyShouldMatchRegisteredType |    77.40 us |   1.901 us |   5.605 us |    77.18 us |       - |      41 B |
 |                       WPF0013ClrMethodMustMatchRegisteredType |   410.48 us |  11.019 us |  32.317 us |   408.71 us |       - |      44 B |
 |                          WPF0014SetValueMustUseRegisteredType | 5,733.76 us | 131.408 us | 385.396 us | 5,699.68 us | 85.9375 |  589126 B |
 |              WPF0015RegisteredOwnerTypeMustBeDependencyObject | 3,710.97 us | 122.342 us | 358.807 us | 3,691.32 us | 42.9688 |  295043 B |
 |                      WPF0016DefaultValueIsSharedReferenceType |   212.88 us |   4.229 us |  10.687 us |   213.00 us |       - |      42 B |
 |                     WPF0030BackingFieldShouldBeStaticReadonly |   210.98 us |   4.193 us |  11.825 us |   207.85 us |       - |      42 B |
 |                                             WPF0031FieldOrder |   178.64 us |   4.247 us |  12.255 us |   178.91 us |       - |      42 B |
 |             WPF0032ClrPropertyGetAndSetSameDependencyProperty |    74.45 us |   2.222 us |   6.516 us |    73.76 us |       - |     191 B |
 |                          WPF0040SetUsingDependencyPropertyKey | 5,378.59 us | 138.598 us | 402.098 us | 5,437.65 us | 85.9375 |  590022 B |
 |                         WPF0041SetMutableUsingSetCurrentValue | 4,847.19 us | 170.700 us | 497.941 us | 4,760.53 us | 46.8750 |  327107 B |
 |                         WPF0042AvoidSideEffectsInClrAccessors |   508.49 us |  18.256 us |  53.255 us |   504.77 us |       - |      48 B |
 |                   WPF0043DontUseSetCurrentValueForDataContext | 3,613.89 us |  90.317 us | 260.584 us | 3,604.91 us | 42.9688 |  295043 B |
 |                    WPF0050XmlnsPrefixMustMatchXmlnsDefinition |   174.22 us |   3.974 us |  11.591 us |   173.45 us |       - |      42 B |
 |                WPF0051XmlnsDefinitionMustMapExistingNamespace |   124.04 us |   2.876 us |   8.434 us |   123.56 us |  0.4883 |    3640 B |
 |                WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |   121.83 us |   2.587 us |   7.588 us |   120.58 us |       - |      41 B |
