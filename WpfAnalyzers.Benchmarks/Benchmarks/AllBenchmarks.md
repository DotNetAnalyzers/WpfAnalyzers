``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410087 Hz, Resolution=293.2477 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                                                        Method |        Mean |      Error |     StdDev |      Median |  Gen 0 |  Gen 1 | Allocated |
 |-------------------------------------------------------------- |------------:|-----------:|-----------:|------------:|-------:|-------:|----------:|
 |                  WPF0001BackingFieldShouldMatchRegisteredName |   282.99 us |   7.451 us |  21.854 us |   283.53 us |      - |      - |      44 B |
 |                  WPF0002BackingFieldShouldMatchRegisteredName |   285.31 us |   7.150 us |  20.969 us |   283.71 us |      - |      - |      44 B |
 |                   WPF0003ClrPropertyShouldMatchRegisteredName |    84.35 us |   2.282 us |   6.729 us |    83.72 us |      - |      - |      41 B |
 |                     WPF0004ClrMethodShouldMatchRegisteredName |   419.17 us |   9.993 us |  28.833 us |   415.36 us |      - |      - |      44 B |
 |       WPF0005PropertyChangedCallbackShouldMatchRegisteredName |   189.41 us |   4.004 us |  11.744 us |   187.88 us | 1.7090 | 0.2441 |   11842 B |
 |           WPF0006CoerceValueCallbackShouldMatchRegisteredName |   219.51 us |  15.698 us |  46.285 us |   197.37 us | 1.7090 | 0.2441 |   11842 B |
 | WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName | 1,271.83 us |  26.512 us |  77.755 us | 1,261.80 us |      - |      - |      48 B |
 |                    WPF0010DefaultValueMustMatchRegisteredType |   195.61 us |   5.004 us |  14.676 us |   196.56 us | 1.7090 | 0.2441 |   11842 B |
 |                  WPF0011ContainingTypeShouldBeRegisteredOwner | 1,287.62 us |  31.764 us |  92.658 us | 1,278.92 us |      - |      - |      48 B |
 |                   WPF0012ClrPropertyShouldMatchRegisteredType |    84.59 us |   1.691 us |   4.769 us |    83.64 us |      - |      - |      42 B |
 |                       WPF0013ClrMethodMustMatchRegisteredType |   425.12 us |   8.498 us |  22.086 us |   424.05 us |      - |      - |      44 B |
 |                          WPF0014SetValueMustUseRegisteredType | 1,605.46 us | 116.253 us | 342.775 us | 1,417.96 us |      - |      - |      48 B |
 |              WPF0015RegisteredOwnerTypeMustBeDependencyObject | 1,327.39 us |  30.511 us |  88.518 us | 1,316.46 us |      - |      - |    1744 B |
 |                      WPF0016DefaultValueIsSharedReferenceType |   126.70 us |   3.323 us |   9.744 us |   125.03 us |      - |      - |      41 B |
 |                               WPF0017MetadataMustBeAssignable | 1,312.36 us |  26.118 us |  75.773 us | 1,310.75 us |      - |      - |      48 B |
 |                     WPF0030BackingFieldShouldBeStaticReadonly |   305.81 us |   6.100 us |  15.191 us |   303.00 us |      - |      - |      44 B |
 |                                             WPF0031FieldOrder |   211.59 us |   4.835 us |  14.256 us |   211.03 us |      - |      - |      42 B |
 |             WPF0032ClrPropertyGetAndSetSameDependencyProperty |    79.09 us |   1.644 us |   4.638 us |    79.21 us |      - |      - |     191 B |
 |                          WPF0040SetUsingDependencyPropertyKey | 1,367.78 us |  29.362 us |  86.113 us | 1,339.70 us |      - |      - |      48 B |
 |                         WPF0041SetMutableUsingSetCurrentValue | 2,321.75 us |  48.865 us | 140.988 us | 2,319.75 us |      - |      - |   28866 B |
 |                         WPF0042AvoidSideEffectsInClrAccessors |   489.72 us |   9.730 us |  26.138 us |   487.36 us |      - |      - |      48 B |
 |                   WPF0043DontUseSetCurrentValueForDataContext | 1,342.16 us |  26.739 us |  75.853 us | 1,335.81 us |      - |      - |      48 B |
 |                    WPF0050XmlnsPrefixMustMatchXmlnsDefinition |   166.94 us |   3.511 us |  10.296 us |   165.50 us |      - |      - |      42 B |
 |                WPF0051XmlnsDefinitionMustMapExistingNamespace |   132.61 us |   3.265 us |   9.524 us |   131.96 us | 0.4883 |      - |    3856 B |
 |                WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |   123.16 us |   3.402 us |   9.924 us |   120.66 us |      - |      - |      42 B |
