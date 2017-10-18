``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410126 Hz, Resolution=293.2443 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2114.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2114.0


```
 |                                                        Method |        Mean |      Error |     StdDev |      Median |   Gen 0 | Allocated |
 |-------------------------------------------------------------- |------------:|-----------:|-----------:|------------:|--------:|----------:|
 |                  WPF0001BackingFieldShouldMatchRegisteredName |   187.21 us |   4.197 us |  12.244 us |   185.40 us |       - |      42 B |
 |                  WPF0002BackingFieldShouldMatchRegisteredName |   185.97 us |   3.688 us |  10.757 us |   183.77 us |       - |      42 B |
 |                   WPF0003ClrPropertyShouldMatchRegisteredName |    75.54 us |   1.710 us |   5.041 us |    75.13 us |       - |      41 B |
 |                     WPF0004ClrMethodShouldMatchRegisteredName |   371.20 us |   7.359 us |  20.875 us |   367.84 us |       - |      44 B |
 |       WPF0005PropertyChangedCallbackShouldMatchRegisteredName |   246.67 us |   5.227 us |  15.329 us |   246.79 us |       - |      42 B |
 |           WPF0006CoerceValueCallbackShouldMatchRegisteredName |   246.69 us |   5.035 us |  14.766 us |   246.59 us |       - |      44 B |
 | WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName | 3,644.09 us |  89.603 us | 264.196 us | 3,631.48 us | 42.9688 |  280579 B |
 |                    WPF0010DefaultValueMustMatchRegisteredType |   239.84 us |   5.097 us |  14.623 us |   238.25 us |       - |      42 B |
 |                  WPF0011ContainingTypeShouldBeRegisteredOwner | 3,449.01 us |  90.753 us | 252.982 us | 3,395.98 us | 42.9688 |  280579 B |
 |                   WPF0012ClrPropertyShouldMatchRegisteredType |    74.54 us |   1.647 us |   4.751 us |    74.53 us |       - |      41 B |
 |                       WPF0013ClrMethodMustMatchRegisteredType |   391.63 us |   8.216 us |  24.226 us |   389.32 us |       - |      44 B |
 |                          WPF0014SetValueMustUseRegisteredType | 7,310.86 us |  40.342 us |  35.762 us | 7,312.99 us | 85.9375 |  561157 B |
 |              WPF0015RegisteredOwnerTypeMustBeDependencyObject | 3,558.50 us |  95.986 us | 278.474 us | 3,479.29 us | 42.9688 |  280579 B |
 |                      WPF0016DefaultValueIsSharedReferenceType |   254.31 us |   6.086 us |  17.849 us |   253.98 us |       - |      42 B |
 |                     WPF0030BackingFieldShouldBeStaticReadonly |   221.58 us |   4.951 us |  14.442 us |   218.99 us |       - |      42 B |
 |                                             WPF0031FieldOrder |   181.79 us |   3.616 us |   8.452 us |   180.61 us |       - |      42 B |
 |             WPF0032ClrPropertyGetAndSetSameDependencyProperty |    74.93 us |   1.481 us |   3.402 us |    75.24 us |       - |     664 B |
 |                          WPF0040SetUsingDependencyPropertyKey | 5,261.37 us | 140.051 us | 408.536 us | 5,240.65 us | 85.9375 |  561157 B |
 |                         WPF0041SetMutableUsingSetCurrentValue | 4,618.44 us | 129.103 us | 374.551 us | 4,562.20 us | 46.8750 |  312644 B |
 |                         WPF0042AvoidSideEffectsInClrAccessors |   444.77 us |   8.848 us |  23.617 us |   445.87 us |       - |      44 B |
 |                   WPF0043DontUseSetCurrentValueForDataContext | 3,527.66 us |  81.269 us | 239.623 us | 3,501.57 us | 42.9688 |  280579 B |
 |                    WPF0050XmlnsPrefixMustMatchXmlnsDefinition |   158.28 us |   3.534 us |  10.252 us |   157.10 us |       - |      42 B |
 |                WPF0051XmlnsDefinitionMustMapExistingNamespace |   118.32 us |   2.459 us |   7.174 us |   117.61 us |  0.4883 |    3568 B |
 |                WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |   119.94 us |   2.365 us |   5.711 us |   119.66 us |       - |      42 B |
