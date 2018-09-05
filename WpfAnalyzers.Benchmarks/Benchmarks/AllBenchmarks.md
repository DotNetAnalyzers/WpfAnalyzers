``` ini

BenchmarkDotNet=v0.11.0, OS=Windows 10.0.17134.228 (1803/April2018Update/Redstone4)
Intel Xeon CPU E5-2637 v4 3.50GHz (Max: 3.49GHz), 2 CPU, 16 logical and 8 physical cores
Frequency=3410080 Hz, Resolution=293.2483 ns, Timer=TSC
  [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3132.0
  DefaultJob : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3132.0


```
|                                           Method |       Mean |      Error |     StdDev |     Median | Allocated |
|------------------------------------------------- |-----------:|-----------:|-----------:|-----------:|----------:|
|                                AttributeAnalyzer | 607.829 us | 12.6520 us | 29.0700 us | 607.024 us |       0 B |
|                                 CallbackAnalyzer | 282.641 us |  6.7441 us | 18.4620 us | 281.225 us |   16384 B |
|                     ClrMethodDeclarationAnalyzer | 129.551 us |  4.7234 us | 13.2448 us | 127.270 us |   16384 B |
|                   ClrPropertyDeclarationAnalyzer |  90.564 us |  3.1382 us |  9.0042 us |  85.042 us |   16384 B |
|                     ComponentResourceKeyAnalyzer |  82.659 us |  3.2299 us |  9.3190 us |  78.297 us |       0 B |
| DependencyPropertyBackingFieldOrPropertyAnalyzer | 139.586 us |  3.6651 us | 10.2167 us | 136.947 us |       0 B |
|                         GetTemplateChildAnalyzer | 324.238 us |  6.4671 us | 15.8638 us | 323.453 us |       0 B |
|                         OverrideMetadataAnalyzer | 259.030 us |  5.8571 us | 16.8991 us | 258.792 us |       0 B |
|                         PropertyMetadataAnalyzer | 155.457 us |  3.9278 us | 11.0784 us | 154.395 us |       0 B |
|                             RegistrationAnalyzer | 317.843 us |  7.9672 us | 21.4033 us | 318.174 us |       0 B |
|                    RoutedCommandCreationAnalyzer |  27.343 us |  0.5932 us |  1.5419 us |  27.272 us |       0 B |
|        RoutedEventBackingFieldOrPropertyAnalyzer | 130.724 us |  3.5370 us | 10.3734 us | 129.909 us |       0 B |
|                      RoutedEventCallbackAnalyzer | 489.912 us |  9.7877 us | 22.0924 us | 495.590 us |       0 B |
|              RoutedEventEventDeclarationAnalyzer |   3.762 us |  0.2493 us |  0.6611 us |   3.519 us |       0 B |
|                                 SetValueAnalyzer | 277.564 us |  6.6496 us | 18.8637 us | 280.639 us |       0 B |
|                           ValueConverterAnalyzer | 105.872 us |  3.3474 us |  9.6579 us | 106.156 us |   24576 B |
|     WPF0011ContainingTypeShouldBeRegisteredOwner | 283.574 us |  6.3057 us | 17.5778 us | 286.943 us |       0 B |
| WPF0015RegisteredOwnerTypeMustBeDependencyObject | 290.362 us |  6.1660 us | 17.7903 us | 290.023 us |       0 B |
|            WPF0041SetMutableUsingSetCurrentValue | 340.560 us |  6.7883 us | 17.7638 us | 341.488 us |       0 B |
|       WPF0050XmlnsPrefixMustMatchXmlnsDefinition | 290.797 us | 10.5455 us | 30.2570 us | 280.932 us |       0 B |
|   WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces | 210.864 us |  6.9741 us | 19.7844 us | 206.447 us |       0 B |
|       WPF0080MarkupExtensionDoesNotHaveAttribute |  93.301 us |  3.8401 us | 10.8312 us |  88.854 us |   16384 B |
|           WPF0083UseConstructorArgumentAttribute |  98.101 us |  3.6369 us | 10.1383 us |  93.253 us |   16384 B |
