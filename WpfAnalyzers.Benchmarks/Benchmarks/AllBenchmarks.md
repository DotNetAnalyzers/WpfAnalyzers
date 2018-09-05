``` ini

BenchmarkDotNet=v0.11.0, OS=Windows 10.0.17134.228 (1803/April2018Update/Redstone4)
Intel Xeon CPU E5-2637 v4 3.50GHz (Max: 3.49GHz), 2 CPU, 16 logical and 8 physical cores
Frequency=3410080 Hz, Resolution=293.2483 ns, Timer=TSC
  [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3132.0
  DefaultJob : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3132.0


```
|                                           Method |       Mean |     Error |     StdDev |     Median | Allocated |
|------------------------------------------------- |-----------:|----------:|-----------:|-----------:|----------:|
|                                AttributeAnalyzer | 698.583 us | 43.116 us | 125.773 us | 627.258 us |       0 B |
|                                 CallbackAnalyzer | 288.157 us |  6.858 us |  19.232 us | 288.263 us |   16384 B |
|                     ClrMethodDeclarationAnalyzer | 137.243 us |  3.129 us |   9.026 us | 136.067 us |   16384 B |
|                   ClrPropertyDeclarationAnalyzer |  92.379 us |  2.822 us |   8.277 us |  89.147 us |   16384 B |
|                     ComponentResourceKeyAnalyzer |  82.995 us |  2.474 us |   7.178 us |  81.523 us |       0 B |
| DependencyPropertyBackingFieldOrPropertyAnalyzer | 135.889 us |  3.213 us |   9.061 us | 137.240 us |       0 B |
|                         GetTemplateChildAnalyzer | 350.206 us |  6.932 us |  17.893 us | 351.018 us |       0 B |
|                         OverrideMetadataAnalyzer | 262.413 us |  6.375 us |  18.083 us | 262.457 us |       0 B |
|                         PropertyMetadataAnalyzer | 154.996 us |  3.091 us |   8.667 us | 156.008 us |       0 B |
|                             RegistrationAnalyzer | 294.208 us |  5.876 us |  14.303 us | 288.263 us |       0 B |
|                    RoutedCommandCreationAnalyzer |  36.060 us |  2.103 us |   5.965 us |  34.310 us |       0 B |
|        RoutedEventBackingFieldOrPropertyAnalyzer | 135.645 us |  3.584 us |  10.167 us | 134.894 us |       0 B |
|                      RoutedEventCallbackAnalyzer | 502.134 us |  9.957 us |  22.271 us | 503.801 us |       0 B |
|              RoutedEventEventDeclarationAnalyzer |   9.806 us |  2.459 us |   7.251 us |   5.718 us |       0 B |
|                                 SetValueAnalyzer | 280.668 us |  5.565 us |  15.419 us | 280.639 us |       0 B |
|                           ValueConverterAnalyzer | 106.491 us |  2.233 us |   6.224 us | 104.396 us |   24576 B |
|     WPF0011ContainingTypeShouldBeRegisteredOwner | 311.543 us |  7.524 us |  21.343 us | 306.444 us |       0 B |
| WPF0015RegisteredOwnerTypeMustBeDependencyObject | 283.659 us |  5.613 us |  13.662 us | 285.477 us |       0 B |
|            WPF0041SetMutableUsingSetCurrentValue | 351.722 us |  7.291 us |  20.324 us | 349.405 us |       0 B |
|       WPF0050XmlnsPrefixMustMatchXmlnsDefinition | 294.440 us | 10.240 us |  29.049 us | 286.504 us |       0 B |
|   WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces | 210.353 us |  6.755 us |  19.271 us | 206.300 us |       0 B |
|       WPF0080MarkupExtensionDoesNotHaveAttribute |  95.813 us |  3.076 us |   8.726 us |  92.960 us |   16384 B |
|           WPF0083UseConstructorArgumentAttribute | 103.892 us |  4.112 us |  11.664 us |  97.945 us |   16384 B |
