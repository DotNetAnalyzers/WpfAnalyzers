``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical and 8 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4018.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4018.0), X64 RyuJIT


```
|                                           Method |        Mean |     Error |    StdDev |      Median | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------------------- |------------:|----------:|----------:|------------:|------:|------:|------:|----------:|
|                                AttributeAnalyzer |   454.68 us | 17.049 us | 45.801 us |   437.80 us |     - |     - |     - |         - |
|                                 CallbackAnalyzer | 1,035.80 us | 20.549 us | 45.962 us | 1,022.05 us |     - |     - |     - |   16384 B |
|                     ClrMethodDeclarationAnalyzer | 1,230.99 us | 23.996 us | 40.747 us | 1,223.10 us |     - |     - |     - |  131072 B |
|                   ClrPropertyDeclarationAnalyzer | 2,271.80 us | 44.331 us | 59.181 us | 2,258.30 us |     - |     - |     - |  147456 B |
|                     ComponentResourceKeyAnalyzer |   111.93 us |  7.065 us | 20.383 us |   106.00 us |     - |     - |     - |         - |
| DependencyPropertyBackingFieldOrPropertyAnalyzer | 3,025.88 us | 77.030 us | 82.421 us | 3,008.35 us |     - |     - |     - |  212992 B |
|                         GetTemplateChildAnalyzer |   177.77 us |  3.773 us | 11.124 us |   173.80 us |     - |     - |     - |         - |
|                         OverrideMetadataAnalyzer |   132.29 us |  2.635 us |  7.258 us |   129.95 us |     - |     - |     - |         - |
|                         PropertyMetadataAnalyzer | 1,751.83 us | 34.339 us | 50.334 us | 1,753.10 us |     - |     - |     - |   90112 B |
|                             RegistrationAnalyzer |   636.08 us | 12.302 us | 18.413 us |   634.20 us |     - |     - |     - |   24576 B |
|                    RoutedCommandCreationAnalyzer |   196.78 us |  3.922 us |  7.833 us |   192.50 us |     - |     - |     - |         - |
|        RoutedEventBackingFieldOrPropertyAnalyzer |   123.15 us |  2.418 us |  2.969 us |   122.50 us |     - |     - |     - |         - |
|                      RoutedEventCallbackAnalyzer |   138.86 us |  7.802 us | 21.878 us |   133.60 us |     - |     - |     - |         - |
|              RoutedEventEventDeclarationAnalyzer |    71.41 us |  5.353 us |  6.960 us |    68.30 us |     - |     - |     - |         - |
|                                 SetValueAnalyzer | 2,821.96 us | 54.273 us | 64.609 us | 2,804.90 us |     - |     - |     - |  188416 B |
|                           ValueConverterAnalyzer |   859.58 us | 16.701 us | 15.622 us |   863.10 us |     - |     - |     - |   24576 B |
|     WPF0011ContainingTypeShouldBeRegisteredOwner |   291.85 us |  5.813 us | 12.512 us |   285.50 us |     - |     - |     - |         - |
| WPF0015RegisteredOwnerTypeMustBeDependencyObject |   277.98 us |  5.501 us | 15.959 us |   276.70 us |     - |     - |     - |         - |
|            WPF0041SetMutableUsingSetCurrentValue |   854.16 us | 13.656 us | 11.403 us |   857.30 us |     - |     - |     - |   40960 B |
|       WPF0050XmlnsPrefixMustMatchXmlnsDefinition |   112.59 us |  2.822 us |  6.194 us |   110.30 us |     - |     - |     - |         - |
|   WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |    86.58 us |  1.960 us |  4.846 us |    84.60 us |     - |     - |     - |         - |
|       WPF0080MarkupExtensionDoesNotHaveAttribute |   181.59 us |  3.016 us |  2.674 us |   181.65 us |     - |     - |     - |         - |
|           WPF0083UseConstructorArgumentAttribute |   139.05 us |  1.879 us |  1.569 us |   138.80 us |     - |     - |     - |         - |
