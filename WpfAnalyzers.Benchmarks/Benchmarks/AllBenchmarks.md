``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical and 8 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4018.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4018.0), X64 RyuJIT


```
|                                           Method |        Mean |     Error |     StdDev |      Median | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------------------- |------------:|----------:|-----------:|------------:|------:|------:|------:|----------:|
|                                AttributeAnalyzer |   418.34 us |  8.163 us |  12.218 us |   417.40 us |     - |     - |     - |         - |
|                                 CallbackAnalyzer | 1,146.35 us | 66.184 us | 180.059 us | 1,086.00 us |     - |     - |     - |   16384 B |
|                     ClrMethodDeclarationAnalyzer | 1,242.53 us | 24.508 us |  51.696 us | 1,228.25 us |     - |     - |     - |  131072 B |
|                   ClrPropertyDeclarationAnalyzer | 2,356.61 us | 47.020 us |  88.315 us | 2,331.10 us |     - |     - |     - |  139264 B |
|                     ComponentResourceKeyAnalyzer |    94.58 us |  3.140 us |   9.060 us |    92.45 us |     - |     - |     - |         - |
| DependencyPropertyBackingFieldOrPropertyAnalyzer | 3,088.30 us | 49.269 us |  43.676 us | 3,092.35 us |     - |     - |     - |  212992 B |
|                         GetTemplateChildAnalyzer |   181.34 us |  4.803 us |  13.626 us |   177.90 us |     - |     - |     - |         - |
|                         OverrideMetadataAnalyzer |   144.62 us |  4.631 us |  13.654 us |   143.55 us |     - |     - |     - |         - |
|                         PropertyMetadataAnalyzer | 1,770.41 us | 33.871 us |  34.783 us | 1,766.00 us |     - |     - |     - |   90112 B |
|                             RegistrationAnalyzer |   605.20 us | 12.072 us |  28.218 us |   601.10 us |     - |     - |     - |   24576 B |
|                    RoutedCommandCreationAnalyzer |   194.77 us |  3.880 us |   8.352 us |   192.15 us |     - |     - |     - |         - |
|        RoutedEventBackingFieldOrPropertyAnalyzer |   122.13 us |  2.440 us |   6.678 us |   119.50 us |     - |     - |     - |         - |
|                      RoutedEventCallbackAnalyzer |   132.23 us |  7.377 us |  21.166 us |   125.60 us |     - |     - |     - |         - |
|              RoutedEventEventDeclarationAnalyzer |    69.00 us |  1.589 us |   2.740 us |    68.15 us |     - |     - |     - |         - |
|                                 SetValueAnalyzer | 2,821.25 us | 38.211 us |  31.908 us | 2,821.00 us |     - |     - |     - |  188416 B |
|                           ValueConverterAnalyzer |   875.81 us | 17.027 us |  24.958 us |   874.70 us |     - |     - |     - |   24576 B |
|     WPF0011ContainingTypeShouldBeRegisteredOwner |   328.09 us | 18.877 us |  50.712 us |   306.25 us |     - |     - |     - |         - |
| WPF0015RegisteredOwnerTypeMustBeDependencyObject |   256.48 us |  5.203 us |  14.676 us |   255.25 us |     - |     - |     - |         - |
|            WPF0041SetMutableUsingSetCurrentValue |   894.87 us | 17.135 us |  16.028 us |   896.30 us |     - |     - |     - |   40960 B |
|       WPF0050XmlnsPrefixMustMatchXmlnsDefinition |   110.29 us |  2.201 us |   3.616 us |   109.50 us |     - |     - |     - |         - |
|   WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |   103.77 us |  6.741 us |  19.232 us |    97.75 us |     - |     - |     - |         - |
|       WPF0080MarkupExtensionDoesNotHaveAttribute |   182.42 us |  3.638 us |   9.261 us |   177.30 us |     - |     - |     - |         - |
|           WPF0083UseConstructorArgumentAttribute |   138.52 us |  2.715 us |   2.267 us |   139.10 us |     - |     - |     - |         - |
