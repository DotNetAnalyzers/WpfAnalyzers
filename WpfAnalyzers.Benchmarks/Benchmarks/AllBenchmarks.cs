// ReSharper disable RedundantNameQualifier
namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class AllBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark AttributeAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.AttributeAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark CallbackAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.CallbackAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark ClrMethodDeclarationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.ClrMethodDeclarationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark ClrPropertyDeclarationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.ClrPropertyDeclarationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark ComponentResourceKeyAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.ComponentResourceKeyAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark DependencyPropertyBackingFieldOrPropertyAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.DependencyPropertyBackingFieldOrPropertyAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark GetTemplateChildAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.GetTemplateChildAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark OverrideMetadataAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.OverrideMetadataAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark PropertyMetadataAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.PropertyMetadataAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark RegistrationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.RegistrationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark RoutedCommandCreationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.RoutedCommandCreationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark RoutedEventBackingFieldOrPropertyAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.RoutedEventBackingFieldOrPropertyAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark RoutedEventCallbackAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.RoutedEventCallbackAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark RoutedEventEventDeclarationAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.RoutedEventEventDeclarationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark SetValueAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.SetValueAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark ValueConverterAnalyzerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.ValueConverterAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0011ContainingTypeShouldBeRegisteredOwnerBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0011ContainingTypeShouldBeRegisteredOwner());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0015RegisteredOwnerTypeMustBeDependencyObjectBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0015RegisteredOwnerTypeMustBeDependencyObject());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0041SetMutableUsingSetCurrentValueBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0041SetMutableUsingSetCurrentValue());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0050XmlnsPrefixMustMatchXmlnsDefinitionBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0050XmlnsPrefixMustMatchXmlnsDefinition());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0052XmlnsDefinitionsDoesNotMapAllNamespacesBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0080MarkupExtensionDoesNotHaveAttributeBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0080MarkupExtensionDoesNotHaveAttribute());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0083UseConstructorArgumentAttributeBenchmark = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0083UseConstructorArgumentAttribute());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void AttributeAnalyzer()
        {
            AttributeAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void CallbackAnalyzer()
        {
            CallbackAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void ClrMethodDeclarationAnalyzer()
        {
            ClrMethodDeclarationAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void ClrPropertyDeclarationAnalyzer()
        {
            ClrPropertyDeclarationAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void ComponentResourceKeyAnalyzer()
        {
            ComponentResourceKeyAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void DependencyPropertyBackingFieldOrPropertyAnalyzer()
        {
            DependencyPropertyBackingFieldOrPropertyAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void GetTemplateChildAnalyzer()
        {
            GetTemplateChildAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void OverrideMetadataAnalyzer()
        {
            OverrideMetadataAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void PropertyMetadataAnalyzer()
        {
            PropertyMetadataAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RegistrationAnalyzer()
        {
            RegistrationAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RoutedCommandCreationAnalyzer()
        {
            RoutedCommandCreationAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RoutedEventBackingFieldOrPropertyAnalyzer()
        {
            RoutedEventBackingFieldOrPropertyAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RoutedEventCallbackAnalyzer()
        {
            RoutedEventCallbackAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RoutedEventEventDeclarationAnalyzer()
        {
            RoutedEventEventDeclarationAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void SetValueAnalyzer()
        {
            SetValueAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void ValueConverterAnalyzer()
        {
            ValueConverterAnalyzerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0011ContainingTypeShouldBeRegisteredOwner()
        {
            WPF0011ContainingTypeShouldBeRegisteredOwnerBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0015RegisteredOwnerTypeMustBeDependencyObject()
        {
            WPF0015RegisteredOwnerTypeMustBeDependencyObjectBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0041SetMutableUsingSetCurrentValue()
        {
            WPF0041SetMutableUsingSetCurrentValueBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0050XmlnsPrefixMustMatchXmlnsDefinition()
        {
            WPF0050XmlnsPrefixMustMatchXmlnsDefinitionBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces()
        {
            WPF0052XmlnsDefinitionsDoesNotMapAllNamespacesBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0080MarkupExtensionDoesNotHaveAttribute()
        {
            WPF0080MarkupExtensionDoesNotHaveAttributeBenchmark.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0083UseConstructorArgumentAttribute()
        {
            WPF0083UseConstructorArgumentAttributeBenchmark.Run();
        }
    }
}
