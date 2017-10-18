``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410126 Hz, Resolution=293.2443 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2114.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2114.0


```
 |                                                        Method |        Mean |      Error |     StdDev |   Gen 0 | Allocated |
 |-------------------------------------------------------------- |------------:|-----------:|-----------:|--------:|----------:|
 |                  WPF0001BackingFieldShouldMatchRegisteredName |   181.51 us |   3.623 us |   9.156 us |       - |      42 B |
 |                  WPF0002BackingFieldShouldMatchRegisteredName |   181.69 us |   3.609 us |   9.758 us |       - |      42 B |
 |                   WPF0003ClrPropertyShouldMatchRegisteredName |    73.74 us |   1.684 us |   4.885 us |       - |      41 B |
 |                     WPF0004ClrMethodShouldMatchRegisteredName |   394.68 us |   9.475 us |  27.640 us |       - |      44 B |
 |       WPF0005PropertyChangedCallbackShouldMatchRegisteredName |   245.37 us |   5.502 us |  16.049 us |       - |      42 B |
 |           WPF0006CoerceValueCallbackShouldMatchRegisteredName |   244.98 us |   4.981 us |  14.451 us |       - |      44 B |
 | WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName | 3,611.21 us |  74.409 us | 217.055 us | 42.9688 |  280579 B |
 |                    WPF0010DefaultValueMustMatchRegisteredType |   254.42 us |   6.819 us |  19.890 us |       - |      44 B |
 |                  WPF0011ContainingTypeShouldBeRegisteredOwner | 3,384.02 us |  95.683 us | 279.112 us | 42.9688 |  280579 B |
 |                   WPF0012ClrPropertyShouldMatchRegisteredType |    76.65 us |   1.743 us |   5.141 us |       - |      41 B |
 |                       WPF0013ClrMethodMustMatchRegisteredType |   392.50 us |   9.201 us |  26.548 us |       - |      44 B |
 |                          WPF0014SetValueMustUseRegisteredType | 5,529.51 us | 145.885 us | 427.854 us | 85.9375 |  561157 B |
 |              WPF0015RegisteredOwnerTypeMustBeDependencyObject | 3,481.04 us |  88.500 us | 259.554 us | 42.9688 |  280579 B |
 |                      WPF0016DefaultValueIsSharedReferenceType |   252.75 us |   7.413 us |  21.508 us |       - |      44 B |
 |                     WPF0030BackingFieldShouldBeStaticReadonly |   223.63 us |   4.885 us |  11.984 us |       - |      42 B |
 |                                             WPF0031FieldOrder |   187.50 us |   3.852 us |  11.114 us |       - |      42 B |
 |             WPF0032ClrPropertyGetAndSetSameDependencyProperty |    76.25 us |   1.744 us |   5.088 us |       - |     664 B |
 |                          WPF0040SetUsingDependencyPropertyKey | 5,571.08 us | 145.482 us | 422.070 us | 85.9375 |  561157 B |
 |                         WPF0041SetMutableUsingSetCurrentValue | 4,769.86 us | 130.537 us | 378.710 us | 46.8750 |  312579 B |
 |                         WPF0042AvoidSideEffectsInClrAccessors |   447.50 us |   8.937 us |  21.066 us |       - |      44 B |
 |                   WPF0043DontUseSetCurrentValueForDataContext | 3,608.78 us | 123.561 us | 364.322 us | 39.0625 |  276259 B |
 |                    WPF0050XmlnsPrefixMustMatchXmlnsDefinition |   159.47 us |   3.200 us |   8.868 us |       - |      42 B |
 |                WPF0051XmlnsDefinitionMustMapExistingNamespace |   123.63 us |   2.457 us |   7.089 us |  0.4883 |    3568 B |
 |                WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |   117.46 us |   2.632 us |   7.759 us |       - |      42 B |
