// ReSharper disable RedundantNameQualifier
namespace WpfAnalyzers.Benchmarks.Benchmarks
{
    public class AllBenchmarks
    {
        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0019 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.CallbackAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0004 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.ClrMethodDeclarationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0003 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.ClrPropertyDeclarationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0001 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.DependencyPropertyBackingFieldOrPropertyAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0017 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.OverrideMetadataAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0005 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.PropertyMetadataAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0007 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.RegistrationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0100 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.RoutedEventBackingFieldOrPropertyAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0090 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.RoutedEventCallbackAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0102 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.RoutedEventEventDeclarationAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0014 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.SetValueAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0070 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.ValueConverterAnalyzer());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0011 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0011ContainingTypeShouldBeRegisteredOwner());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0015 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0015RegisteredOwnerTypeMustBeDependencyObject());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0041 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0041SetMutableUsingSetCurrentValue());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0050 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0050XmlnsPrefixMustMatchXmlnsDefinition());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0052 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0080 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0080MarkupExtensionDoesNotHaveAttribute());

        private static readonly Gu.Roslyn.Asserts.Benchmark WPF0083 = Gu.Roslyn.Asserts.Benchmark.Create(Code.ValidCodeProject, new WpfAnalyzers.WPF0083UseConstructorArgumentAttribute());

        [BenchmarkDotNet.Attributes.Benchmark]
        public void CallbackAnalyzer()
        {
            WPF0019.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void ClrMethodDeclarationAnalyzer()
        {
            WPF0004.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void ClrPropertyDeclarationAnalyzer()
        {
            WPF0003.Run();
        }

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
        public void PropertyMetadataAnalyzer()
        {
            WPF0005.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void RegistrationAnalyzer()
        {
            WPF0007.Run();
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
        public void SetValueAnalyzer()
        {
            WPF0014.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void ValueConverterAnalyzer()
        {
            WPF0070.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0011ContainingTypeShouldBeRegisteredOwner()
        {
            WPF0011.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0015RegisteredOwnerTypeMustBeDependencyObject()
        {
            WPF0015.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0041SetMutableUsingSetCurrentValue()
        {
            WPF0041.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0050XmlnsPrefixMustMatchXmlnsDefinition()
        {
            WPF0050.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces()
        {
            WPF0052.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0080MarkupExtensionDoesNotHaveAttribute()
        {
            WPF0080.Run();
        }

        [BenchmarkDotNet.Attributes.Benchmark]
        public void WPF0083UseConstructorArgumentAttribute()
        {
            WPF0083.Run();
        }
    }
}
