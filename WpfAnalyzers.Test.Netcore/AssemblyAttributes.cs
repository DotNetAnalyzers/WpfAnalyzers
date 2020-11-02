using Gu.Roslyn.Asserts;
using WpfAnalyzers.Test;
using WpfAnalyzers.Test.Netcore;

[assembly: TransitiveMetadataReferences(typeof(System.Windows.Controls.Button))]
[assembly: TransitiveMetadataReferences(typeof(ValidWithAll))]
