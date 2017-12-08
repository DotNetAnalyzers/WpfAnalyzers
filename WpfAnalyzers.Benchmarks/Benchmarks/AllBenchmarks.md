``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU E5-2637 v4 3.50GHzIntel Xeon CPU E5-2637 v4 3.50GHz, ProcessorCount=16
Frequency=3410097 Hz, Resolution=293.2468 ns, Timer=TSC
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2116.0


```
 |                                                        Method |        Mean |      Error |     StdDev |      Median |  Gen 0 |  Gen 1 | Allocated |
 |-------------------------------------------------------------- |------------:|-----------:|-----------:|------------:|-------:|-------:|----------:|
 |                  WPF0001BackingFieldShouldMatchRegisteredName |   300.44 us |  0.6777 us |  0.5659 us |   300.26 us |      - |      - |     444 B |
 |                  WPF0002BackingFieldShouldMatchRegisteredName |   295.34 us |  2.7256 us |  2.2760 us |   294.67 us |      - |      - |     444 B |
 |                   WPF0003ClrPropertyShouldMatchRegisteredName |    98.72 us |  1.9707 us |  2.6309 us |    99.15 us |      - |      - |     441 B |
 |                     WPF0004ClrMethodShouldMatchRegisteredName |   445.80 us |  0.8068 us |  0.7152 us |   445.77 us |      - |      - |     444 B |
 |       WPF0005PropertyChangedCallbackShouldMatchRegisteredName |   231.84 us |  1.0124 us |  0.8975 us |   231.83 us | 2.9297 | 0.4883 |   19832 B |
 |           WPF0006CoerceValueCallbackShouldMatchRegisteredName |   245.97 us |  7.5196 us |  7.3852 us |   242.69 us | 2.9297 | 0.4883 |   19830 B |
 | WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName | 1,580.58 us | 10.6445 us |  8.3106 us | 1,576.71 us |      - |      - |     448 B |
 |                    WPF0010DefaultValueMustMatchRegisteredType |   240.46 us |  4.5091 us |  4.6305 us |   238.01 us | 2.9297 | 0.4883 |   19830 B |
 |                  WPF0011ContainingTypeShouldBeRegisteredOwner | 1,613.66 us |  1.3195 us |  1.1019 us | 1,613.64 us |      - |      - |     448 B |
 |                   WPF0012ClrPropertyShouldMatchRegisteredType |    96.67 us |  1.9262 us |  2.5714 us |    94.84 us |      - |      - |     441 B |
 |                       WPF0013ClrMethodMustMatchRegisteredType |   471.09 us |  9.8619 us |  9.2248 us |   468.32 us |      - |      - |     444 B |
 |                          WPF0014SetValueMustUseRegisteredType | 1,513.39 us |  2.8296 us |  2.6468 us | 1,512.91 us |      - |      - |     448 B |
 |              WPF0015RegisteredOwnerTypeMustBeDependencyObject | 1,500.73 us |  1.7738 us |  1.3848 us | 1,500.69 us |      - |      - |    2144 B |
 |                      WPF0016DefaultValueIsSharedReferenceType |   145.97 us |  0.1949 us |  0.1628 us |   145.96 us |      - |      - |     442 B |
 |                               WPF0017MetadataMustBeAssignable | 1,448.68 us |  1.5864 us |  1.4839 us | 1,448.62 us |      - |      - |     448 B |
 |                     WPF0030BackingFieldShouldBeStaticReadonly |   347.98 us |  6.9273 us | 14.3060 us |   347.78 us |      - |      - |     444 B |
 |                                             WPF0031FieldOrder |   211.21 us |  0.2004 us |  0.1565 us |   211.19 us |      - |      - |     442 B |
 |             WPF0032ClrPropertyGetAndSetSameDependencyProperty |    92.14 us |  0.5148 us |  0.4299 us |    92.01 us |      - |      - |     590 B |
 |                          WPF0040SetUsingDependencyPropertyKey | 1,549.55 us |  8.3035 us |  6.4828 us | 1,547.96 us |      - |      - |     448 B |
 |                         WPF0041SetMutableUsingSetCurrentValue | 2,350.21 us |  2.3066 us |  1.9261 us | 2,350.22 us |      - |      - |   29054 B |
 |                         WPF0042AvoidSideEffectsInClrAccessors |   553.45 us |  6.2039 us |  4.1035 us |   552.03 us |      - |      - |     448 B |
 |                   WPF0043DontUseSetCurrentValueForDataContext | 1,459.29 us |  1.8723 us |  1.6598 us | 1,459.03 us |      - |      - |     448 B |
 |                    WPF0050XmlnsPrefixMustMatchXmlnsDefinition |   374.48 us | 10.5452 us |  9.8640 us |   370.73 us | 2.4414 | 0.4883 |   17584 B |
 |                WPF0051XmlnsDefinitionMustMapExistingNamespace |   233.59 us |  0.4460 us |  0.4172 us |   233.53 us | 1.9531 | 0.2441 |   13404 B |
 |                WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |   230.97 us |  0.3310 us |  0.2584 us |   230.92 us | 1.2207 | 0.2441 |    9012 B |
 |                                   WPF0060DocumentDependencyPropertyBackingField |   389.11 us |  2.2155 us |  1.8500 us |   388.66 us |      - |      - |     444 B |
 |                                WPF0061ClrMethodShouldHaveDocs |   690.81 us |  3.1730 us |  2.4773 us |   690.27 us |      - |      - |     448 B |
 |                       WPF0070ConverterDoesNotHaveDefaultField |   131.51 us |  0.1918 us |  0.1700 us |   131.50 us |      - |      - |     442 B |
 |                          WPF0071ConverterDoesNotHaveAttribute |   131.27 us |  0.4719 us |  0.3684 us |   131.20 us |      - |      - |     442 B |
 |                     WPF0072ValueConversionMustUseCorrectTypes |    82.53 us |  1.6453 us |  2.1394 us |    82.33 us |      - |      - |     441 B |
 |                    WPF0080MarkupExtensionDoesNotHaveAttribute |   128.76 us |  0.3365 us |  0.2983 us |   128.74 us |      - |      - |     442 B |
 |            WPF0081MarkupExtensionReturnTypeMustUseCorrectType |    78.48 us |  0.2056 us |  0.1923 us |    78.47 us |      - |      - |     441 B |
