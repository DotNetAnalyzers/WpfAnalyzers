``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.17134.885 (1803/April2018Update/Redstone4)
Intel Core i7-7500U CPU 2.70GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
Frequency=2835936 Hz, Resolution=352.6173 ns, Timer=TSC
  [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3416.0
  DefaultJob : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3416.0


```
|                                           Method |         Mean |       Error |      StdDev |       Median | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------------------------------- |-------------:|------------:|------------:|-------------:|------:|------:|------:|----------:|
|                                AttributeAnalyzer | 1,118.022 us | 110.6571 us | 326.2750 us | 1,232.045 us |     - |     - |     - |         - |
|                                 CallbackAnalyzer | 1,156.849 us | 120.9502 us | 356.6244 us |   925.268 us |     - |     - |     - |         - |
|                     ClrMethodDeclarationAnalyzer | 1,381.678 us | 127.4215 us | 375.7051 us | 1,140.893 us |     - |     - |     - |   60600 B |
|                   ClrPropertyDeclarationAnalyzer | 1,093.794 us | 118.3503 us | 348.9586 us |   858.270 us |     - |     - |     - |   40960 B |
|                     ComponentResourceKeyAnalyzer |    25.631 us |   1.2419 us |   2.3628 us |    25.036 us |     - |     - |     - |         - |
| DependencyPropertyBackingFieldOrPropertyAnalyzer |   815.574 us |  10.0411 us |   7.8394 us |   816.309 us |     - |     - |     - |   49152 B |
|                         GetTemplateChildAnalyzer |   138.567 us |   2.7474 us |   6.1450 us |   137.168 us |     - |     - |     - |         - |
|                         OverrideMetadataAnalyzer |   104.957 us |   2.9080 us |   8.3435 us |   101.554 us |     - |     - |     - |         - |
|                         PropertyMetadataAnalyzer |   848.203 us |  16.6446 us |  19.1679 us |   851.747 us |     - |     - |     - |   40960 B |
|                             RegistrationAnalyzer |   413.484 us |   8.1543 us |  12.4525 us |   412.562 us |     - |     - |     - |   16384 B |
|                    RoutedCommandCreationAnalyzer |    71.455 us |   1.9420 us |   2.7851 us |    71.229 us |     - |     - |     - |         - |
|        RoutedEventBackingFieldOrPropertyAnalyzer |    34.671 us |   0.9813 us |   2.7677 us |    33.499 us |     - |     - |     - |         - |
|                      RoutedEventCallbackAnalyzer |    67.196 us |   1.9428 us |   5.6055 us |    64.705 us |     - |     - |     - |         - |
|              RoutedEventEventDeclarationAnalyzer |     5.790 us |   0.2506 us |   0.6903 us |     5.642 us |     - |     - |     - |         - |
|                                 SetValueAnalyzer |   912.275 us |  18.1877 us |  35.0415 us |   908.342 us |     - |     - |     - |   49152 B |
|                           ValueConverterAnalyzer |    68.546 us |   1.6435 us |   3.3572 us |    67.350 us |     - |     - |     - |         - |
|     WPF0011ContainingTypeShouldBeRegisteredOwner |   223.330 us |   4.4516 us |   9.9566 us |   222.854 us |     - |     - |     - |         - |
| WPF0015RegisteredOwnerTypeMustBeDependencyObject |   187.207 us |   3.7227 us |   7.7706 us |   185.829 us |     - |     - |     - |         - |
|            WPF0041SetMutableUsingSetCurrentValue |   622.428 us |  12.2437 us |  18.3258 us |   617.433 us |     - |     - |     - |   24576 B |
|       WPF0050XmlnsPrefixMustMatchXmlnsDefinition |   207.583 us |   4.0931 us |   5.6027 us |   205.576 us |     - |     - |     - |         - |
|   WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |   123.034 us |   2.4375 us |   4.0726 us |   121.124 us |     - |     - |     - |         - |
|       WPF0080MarkupExtensionDoesNotHaveAttribute |    85.341 us |   1.9351 us |   3.6818 us |    84.276 us |     - |     - |     - |         - |
|           WPF0083UseConstructorArgumentAttribute |    96.353 us |   2.2802 us |   2.2394 us |    95.736 us |     - |     - |     - |         - |
