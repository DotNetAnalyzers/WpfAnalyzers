using System;
using Gu.Roslyn.Asserts;
using WpfAnalyzers.Test.Netcore;

[assembly: CLSCompliant(false)]

[assembly: TransitiveMetadataReferences(typeof(System.Windows.Controls.Button))]
[assembly: TransitiveMetadataReferences(typeof(ValidWithAll))]
