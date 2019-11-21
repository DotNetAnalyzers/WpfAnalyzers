``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical and 8 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4018.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4018.0), X64 RyuJIT


```
|                                           Method |        Mean |      Error |     StdDev |      Median | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------------------- |------------:|-----------:|-----------:|------------:|------:|------:|------:|----------:|
|                                AttributeAnalyzer |   421.70 us |   9.105 us |  15.706 us |   420.20 us |     - |     - |     - |         - |
|                                 CallbackAnalyzer | 1,337.47 us | 133.239 us | 392.859 us | 1,086.45 us |     - |     - |     - |   16384 B |
|                     ClrMethodDeclarationAnalyzer | 1,551.85 us | 152.062 us | 448.359 us | 1,486.65 us |     - |     - |     - |   40960 B |
|                   ClrPropertyDeclarationAnalyzer | 3,061.90 us | 272.707 us | 804.081 us | 2,466.70 us |     - |     - |     - |  147456 B |
|                     ComponentResourceKeyAnalyzer |    96.07 us |   3.986 us |  11.110 us |    93.65 us |     - |     - |     - |         - |
| DependencyPropertyBackingFieldOrPropertyAnalyzer | 2,976.39 us |  55.436 us |  51.854 us | 2,973.90 us |     - |     - |     - |  196608 B |
|                         GetTemplateChildAnalyzer |   185.51 us |   4.936 us |  14.163 us |   181.20 us |     - |     - |     - |         - |
|                         OverrideMetadataAnalyzer |   133.02 us |   4.411 us |  12.936 us |   128.90 us |     - |     - |     - |         - |
|                         PropertyMetadataAnalyzer | 1,796.49 us |  35.712 us |  54.535 us | 1,798.00 us |     - |     - |     - |   90112 B |
|                             RegistrationAnalyzer |   644.64 us |  15.882 us |  42.666 us |   639.90 us |     - |     - |     - |   24576 B |
|                    RoutedCommandCreationAnalyzer |   201.18 us |   4.524 us |  12.537 us |   198.10 us |     - |     - |     - |         - |
|        RoutedEventBackingFieldOrPropertyAnalyzer |   129.76 us |   4.699 us |  13.406 us |   129.15 us |     - |     - |     - |         - |
|                      RoutedEventCallbackAnalyzer |   107.93 us |   4.820 us |  13.594 us |   103.20 us |     - |     - |     - |         - |
|              RoutedEventEventDeclarationAnalyzer |    71.31 us |   1.422 us |   1.993 us |    71.00 us |     - |     - |     - |         - |
|                                 SetValueAnalyzer | 2,858.38 us |  68.734 us |  64.294 us | 2,843.40 us |     - |     - |     - |  188416 B |
|                           ValueConverterAnalyzer |   863.52 us |  19.409 us |  23.836 us |   857.45 us |     - |     - |     - |   24576 B |
|     WPF0011ContainingTypeShouldBeRegisteredOwner |   302.66 us |   6.007 us |  13.922 us |   305.20 us |     - |     - |     - |         - |
| WPF0015RegisteredOwnerTypeMustBeDependencyObject |   243.74 us |   4.834 us |   8.960 us |   241.30 us |     - |     - |     - |         - |
|            WPF0041SetMutableUsingSetCurrentValue |   885.99 us |  15.037 us |  12.557 us |   886.40 us |     - |     - |     - |   40960 B |
|       WPF0050XmlnsPrefixMustMatchXmlnsDefinition |   122.77 us |   5.098 us |  14.790 us |   120.90 us |     - |     - |     - |         - |
|   WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |    91.00 us |   1.586 us |   2.172 us |    90.95 us |     - |     - |     - |         - |
|       WPF0080MarkupExtensionDoesNotHaveAttribute |   190.43 us |   5.601 us |  15.614 us |   184.35 us |     - |     - |     - |         - |
|           WPF0083UseConstructorArgumentAttribute |   160.44 us |   7.040 us |  20.423 us |   156.40 us |     - |     - |     - |         - |
