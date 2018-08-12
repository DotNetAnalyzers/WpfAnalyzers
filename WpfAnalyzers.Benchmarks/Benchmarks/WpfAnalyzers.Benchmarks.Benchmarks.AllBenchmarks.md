``` ini

BenchmarkDotNet=v0.11.0, OS=Windows 10.0.17134.81 (1803/April2018Update/Redstone4)
Intel Xeon CPU E5-2637 v4 3.50GHz (Max: 3.49GHz), 2 CPU, 16 logical and 8 physical cores
Frequency=3410072 Hz, Resolution=293.2489 ns, Timer=TSC
  [Host]     : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3110.0
  DefaultJob : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3110.0


```
|                                             Method |          Mean |       Error |      StdDev |        Median | Allocated |
|--------------------------------------------------- |--------------:|------------:|------------:|--------------:|----------:|
|                                   CallbackAnalyzer |  4,584.135 us |  30.1761 us |  25.1984 us |  4,582.894 us |   16384 B |
|                       ClrMethodDeclarationAnalyzer |  1,515.259 us |  26.2201 us |  23.2435 us |  1,514.044 us |  155648 B |
|                     ClrPropertyDeclarationAnalyzer |    439.387 us |   8.5542 us |  14.0547 us |    441.046 us |   49152 B |
|   DependencyPropertyBackingFieldOrPropertyAnalyzer |  1,435.180 us |  25.1037 us |  23.4820 us |  1,442.492 us |       0 B |
|                           OverrideMetadataAnalyzer |  7,640.503 us |  98.2074 us |  91.8632 us |  7,603.065 us |       0 B |
|                           PropertyMetadataAnalyzer |    917.800 us |  17.8665 us |  18.3476 us |    922.561 us |       0 B |
|                               RegistrationAnalyzer |  7,513.184 us |  61.4602 us |  54.4829 us |  7,490.311 us |       0 B |
|          RoutedEventBackingFieldOrPropertyAnalyzer |  1,406.891 us |  23.3612 us |  21.8521 us |  1,407.595 us |       0 B |
|                        RoutedEventCallbackAnalyzer | 16,074.949 us | 108.4907 us | 101.4823 us | 16,096.728 us |       0 B |
|                RoutedEventEventDeclarationAnalyzer |      7.506 us |   0.1693 us |   0.4636 us |      7.624 us |       0 B |
|                                   SetValueAnalyzer |  7,400.786 us |  84.9011 us |  75.2626 us |  7,378.290 us |       0 B |
|                             ValueConverterAnalyzer |  1,323.649 us |  26.0580 us |  28.9634 us |  1,327.831 us |  262144 B |
|       WPF0011ContainingTypeShouldBeRegisteredOwner |  7,707.892 us |  88.1960 us |  82.4986 us |  7,697.491 us |       0 B |
|   WPF0015RegisteredOwnerTypeMustBeDependencyObject |  7,534.377 us |  33.1874 us |  27.7130 us |  7,529.167 us |       0 B |
|              WPF0041SetMutableUsingSetCurrentValue |  9,339.920 us | 122.2551 us | 114.3575 us |  9,323.264 us |   65536 B |
|         WPF0050XmlnsPrefixMustMatchXmlnsDefinition |    289.658 us |   5.7169 us |  10.3088 us |    289.143 us |       0 B |
|     WPF0051XmlnsDefinitionMustMapExistingNamespace |    279.403 us |   5.5376 us |  12.3857 us |    280.932 us |       0 B |
|     WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces |    272.846 us |   5.4225 us |   8.6007 us |    274.188 us |       0 B |
|         WPF0080MarkupExtensionDoesNotHaveAttribute |    990.082 us |   3.0520 us |   2.3828 us |    990.008 us |  139264 B |
| WPF0081MarkupExtensionReturnTypeMustUseCorrectType |    284.791 us |   5.6509 us |  10.6138 us |    279.320 us |   32768 B |
|                         WPF0082ConstructorArgument |    258.697 us |   5.1679 us |  12.4812 us |    262.165 us |       0 B |
|             WPF0083UseConstructorArgumentAttribute |    405.774 us |   8.0101 us |  17.4132 us |    404.097 us |   49152 B |
