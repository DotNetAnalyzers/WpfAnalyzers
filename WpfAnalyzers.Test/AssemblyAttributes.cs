using System;
using Gu.Roslyn.Asserts;
using WpfAnalyzers.Test;

[assembly: CLSCompliant(false)]

[assembly: TransitiveMetadataReferences(typeof(System.Windows.Controls.Button))]
[assembly: TransitiveMetadataReferences(typeof(ValidWithAll))]
