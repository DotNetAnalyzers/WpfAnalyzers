``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410117 Hz, Resolution=293.2451 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                                                        Method |        Mean |     Error |     StdDev |  Gen 0 | Allocated |
 |-------------------------------------------------------------- |------------:|----------:|-----------:|-------:|----------:|
 |                  WPF0001BackingFieldShouldMatchRegisteredName |   182.71 us |  4.859 us |  14.098 us |      - |      42 B |
 |                  WPF0002BackingFieldShouldMatchRegisteredName |   176.23 us |  3.737 us |  10.600 us |      - |      42 B |
 |                   WPF0003ClrPropertyShouldMatchRegisteredName |    77.83 us |  1.554 us |   4.279 us |      - |      41 B |
 |                     WPF0004ClrMethodShouldMatchRegisteredName |   433.03 us |  9.539 us |  27.827 us |      - |      44 B |
 |       WPF0005PropertyChangedCallbackShouldMatchRegisteredName |   162.08 us |  4.145 us |  11.960 us |      - |      42 B |
 |           WPF0006CoerceValueCallbackShouldMatchRegisteredName |   163.65 us |  4.033 us |  11.765 us |      - |      42 B |
 | WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName | 1,227.92 us | 32.087 us |  93.600 us |      - |      48 B |
 |                    WPF0010DefaultValueMustMatchRegisteredType |   160.44 us |  3.207 us |   9.097 us |      - |      42 B |
 |                  WPF0011ContainingTypeShouldBeRegisteredOwner | 1,159.74 us | 26.809 us |  77.350 us |      - |      48 B |
 |                   WPF0012ClrPropertyShouldMatchRegisteredType |    76.06 us |  1.951 us |   5.534 us |      - |      41 B |
 |                       WPF0013ClrMethodMustMatchRegisteredType |   453.32 us | 10.221 us |  29.815 us |      - |      44 B |
 |                          WPF0014SetValueMustUseRegisteredType | 1,377.15 us | 35.446 us | 103.396 us |      - |    3312 B |
 |              WPF0015RegisteredOwnerTypeMustBeDependencyObject | 1,286.37 us | 27.483 us |  77.966 us |      - |      48 B |
 |                      WPF0016DefaultValueIsSharedReferenceType |   217.03 us |  5.614 us |  16.108 us |      - |      42 B |
 |                     WPF0030BackingFieldShouldBeStaticReadonly |   210.16 us |  4.308 us |  12.498 us |      - |      42 B |
 |                                             WPF0031FieldOrder |   179.16 us |  4.306 us |  12.285 us |      - |      42 B |
 |             WPF0032ClrPropertyGetAndSetSameDependencyProperty |    73.33 us |  1.844 us |   5.320 us |      - |     191 B |
 |                          WPF0040SetUsingDependencyPropertyKey | 1,405.02 us | 35.656 us | 104.574 us |      - |    3312 B |
 |                         WPF0041SetMutableUsingSetCurrentValue | 2,428.12 us | 51.763 us | 151.813 us | 3.9063 |   34048 B |
 |                         WPF0042AvoidSideEffectsInClrAccessors |   486.25 us | 10.091 us |  25.867 us |      - |      44 B |
 |                   WPF0043DontUseSetCurrentValueForDataContext | 1,304.25 us | 37.005 us | 108.529 us |      - |    1680 B |
 |                    WPF0050XmlnsPrefixMustMatchXmlnsDefinition |   173.92 us |  4.237 us |  12.428 us |      - |      42 B |
 |                WPF0051XmlnsDefinitionMustMapExistingNamespace |   130.51 us |  3.327 us |   9.547 us | 0.4883 |    3640 B |
 |                WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |   139.40 us |  4.263 us |  12.569 us |      - |      42 B |
