// ReSharper disable RedundantNameQualifier
namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class AllBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0001 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.DependencyPropertyBackingFieldOrPropertyAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0017 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.OverrideMetadataAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0003 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.PropertyDeclarationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0100 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.RoutedEventBackingFieldOrPropertyAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0090 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.RoutedEventCallbackAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0102 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.RoutedEventEventDeclarationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0004 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0004ClrMethodShouldMatchRegisteredName());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0005 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0005PropertyChangedCallbackShouldMatchRegisteredName());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0006 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0006CoerceValueCallbackShouldMatchRegisteredName());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0007 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0010 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0010DefaultValueMustMatchRegisteredType());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0011 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0011ContainingTypeShouldBeRegisteredOwner());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0013 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0013ClrMethodMustMatchRegisteredType());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0014 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0014SetValueMustUseRegisteredType());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0015 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0015RegisteredOwnerTypeMustBeDependencyObject());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0016 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0016DefaultValueIsSharedReferenceType());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0030 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0030BackingFieldShouldBeStaticReadonly());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0031 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0031FieldOrder());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0033 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0033UseAttachedPropertyBrowsableForTypeAttribute());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0034 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0034AttachedPropertyBrowsableForTypeAttributeArgument());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0040 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0040SetUsingDependencyPropertyKey());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0041 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0041SetMutableUsingSetCurrentValue());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0042 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0042AvoidSideEffectsInClrAccessors());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0043 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0043DontUseSetCurrentValueForDataContext());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0050 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0050XmlnsPrefixMustMatchXmlnsDefinition());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0051 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0051XmlnsDefinitionMustMapExistingNamespace());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0052 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0061 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0061ClrMethodShouldHaveDocs());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0070 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0070ConverterDoesNotHaveDefaultField());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0071 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0071ConverterDoesNotHaveAttribute());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0072 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0072ValueConversionMustUseCorrectTypes());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0080 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0080MarkupExtensionDoesNotHaveAttribute());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0081 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0081MarkupExtensionReturnTypeMustUseCorrectType());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0082 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0082ConstructorArgument());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0083 = Gu.Roslyn.Asserts.Benchmark.Create(Code.AnalyzersProject, new WpfAnalyzers.WPF0083UseConstructorArgumentAttribute());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void DependencyPropertyBackingFieldOrPropertyAnalyzer()
        {
            WPF0001.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void OverrideMetadataAnalyzer()
        {
            WPF0017.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void PropertyDeclarationAnalyzer()
        {
            WPF0003.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RoutedEventBackingFieldOrPropertyAnalyzer()
        {
            WPF0100.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RoutedEventCallbackAnalyzer()
        {
            WPF0090.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RoutedEventEventDeclarationAnalyzer()
        {
            WPF0102.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0004ClrMethodShouldMatchRegisteredName()
        {
            WPF0004.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0005PropertyChangedCallbackShouldMatchRegisteredName()
        {
            WPF0005.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0006CoerceValueCallbackShouldMatchRegisteredName()
        {
            WPF0006.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName()
        {
            WPF0007.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0010DefaultValueMustMatchRegisteredType()
        {
            WPF0010.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0011ContainingTypeShouldBeRegisteredOwner()
        {
            WPF0011.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0013ClrMethodMustMatchRegisteredType()
        {
            WPF0013.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0014SetValueMustUseRegisteredType()
        {
            WPF0014.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0015RegisteredOwnerTypeMustBeDependencyObject()
        {
            WPF0015.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0016DefaultValueIsSharedReferenceType()
        {
            WPF0016.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0030BackingFieldShouldBeStaticReadonly()
        {
            WPF0030.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0031FieldOrder()
        {
            WPF0031.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0033UseAttachedPropertyBrowsableForTypeAttribute()
        {
            WPF0033.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0034AttachedPropertyBrowsableForTypeAttributeArgument()
        {
            WPF0034.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0040SetUsingDependencyPropertyKey()
        {
            WPF0040.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0041SetMutableUsingSetCurrentValue()
        {
            WPF0041.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0042AvoidSideEffectsInClrAccessors()
        {
            WPF0042.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0043DontUseSetCurrentValueForDataContext()
        {
            WPF0043.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0050XmlnsPrefixMustMatchXmlnsDefinition()
        {
            WPF0050.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0051XmlnsDefinitionMustMapExistingNamespace()
        {
            WPF0051.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces()
        {
            WPF0052.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0061ClrMethodShouldHaveDocs()
        {
            WPF0061.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0070ConverterDoesNotHaveDefaultField()
        {
            WPF0070.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0071ConverterDoesNotHaveAttribute()
        {
            WPF0071.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0072ValueConversionMustUseCorrectTypes()
        {
            WPF0072.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0080MarkupExtensionDoesNotHaveAttribute()
        {
            WPF0080.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0081MarkupExtensionReturnTypeMustUseCorrectType()
        {
            WPF0081.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0082ConstructorArgument()
        {
            WPF0082.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0083UseConstructorArgumentAttribute()
        {
            WPF0083.Run();
        }
    }
}
