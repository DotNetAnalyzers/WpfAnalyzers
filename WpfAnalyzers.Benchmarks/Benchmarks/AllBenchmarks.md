``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Xeon CPU E5-2637 v4 3.50GHz, 2 CPU, 16 logical and 8 physical cores
  [Host]     : .NET Framework 4.8 (4.8.4018.0), X64 RyuJIT
  DefaultJob : .NET Framework 4.8 (4.8.4018.0), X64 RyuJIT


```
|                                           Method |        Mean |      Error |     StdDev |      Median | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------------------- |------------:|-----------:|-----------:|------------:|------:|------:|------:|----------:|
|                                AttributeAnalyzer |   441.52 us |   9.335 us |  24.428 us |   438.40 us |     - |     - |     - |         - |
|                                 CallbackAnalyzer | 1,265.77 us |  89.421 us | 252.214 us | 1,184.35 us |     - |     - |     - |   16384 B |
|                     ClrMethodDeclarationAnalyzer | 1,438.48 us |  52.068 us | 151.058 us | 1,363.00 us |     - |     - |     - |   40960 B |
|                   ClrPropertyDeclarationAnalyzer | 2,476.13 us |  57.498 us | 162.175 us | 2,424.30 us |     - |     - |     - |  147456 B |
|                     ComponentResourceKeyAnalyzer |   108.51 us |   5.579 us |  16.451 us |   101.40 us |     - |     - |     - |         - |
| DependencyPropertyBackingFieldOrPropertyAnalyzer | 3,231.79 us | 105.664 us | 301.465 us | 3,102.35 us |     - |     - |     - |  196608 B |
|                         GetTemplateChildAnalyzer |   185.80 us |   6.823 us |  19.467 us |   181.55 us |     - |     - |     - |         - |
|                         OverrideMetadataAnalyzer |   159.55 us |   8.013 us |  22.602 us |   155.05 us |     - |     - |     - |         - |
|                         PropertyMetadataAnalyzer | 1,934.19 us |  85.178 us | 248.469 us | 1,788.10 us |     - |     - |     - |   90112 B |
|                             RegistrationAnalyzer |   657.95 us |  27.656 us |  79.351 us |   622.20 us |     - |     - |     - |   24576 B |
|                    RoutedCommandCreationAnalyzer |   218.80 us |   9.704 us |  27.210 us |   213.90 us |     - |     - |     - |         - |
|        RoutedEventBackingFieldOrPropertyAnalyzer |   124.40 us |   3.678 us |  10.251 us |   120.15 us |     - |     - |     - |         - |
|                      RoutedEventCallbackAnalyzer |   112.11 us |   5.010 us |  14.212 us |   106.90 us |     - |     - |     - |         - |
|              RoutedEventEventDeclarationAnalyzer |    76.53 us |   2.923 us |   8.619 us |    71.70 us |     - |     - |     - |         - |
|                                 SetValueAnalyzer | 3,146.36 us | 121.298 us | 349.973 us | 3,038.65 us |     - |     - |     - |  188416 B |
|                           ValueConverterAnalyzer |   986.10 us |  49.303 us | 139.865 us |   926.30 us |     - |     - |     - |   24576 B |
|     WPF0011ContainingTypeShouldBeRegisteredOwner |   304.43 us |   8.902 us |  23.761 us |   296.20 us |     - |     - |     - |         - |
| WPF0015RegisteredOwnerTypeMustBeDependencyObject |   279.57 us |   5.978 us |  14.888 us |   277.20 us |     - |     - |     - |         - |
|            WPF0041SetMutableUsingSetCurrentValue |   986.19 us |  40.147 us | 113.236 us |   936.90 us |     - |     - |     - |   40960 B |
|       WPF0050XmlnsPrefixMustMatchXmlnsDefinition |   147.21 us |  10.513 us |  30.997 us |   130.85 us |     - |     - |     - |         - |
|   WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |    93.24 us |   3.410 us |   9.672 us |    90.80 us |     - |     - |     - |         - |
|       WPF0080MarkupExtensionDoesNotHaveAttribute |   205.16 us |  12.898 us |  37.828 us |   183.70 us |     - |     - |     - |         - |
|           WPF0083UseConstructorArgumentAttribute |   148.85 us |   4.930 us |  13.158 us |   143.60 us |     - |     - |     - |         - |
