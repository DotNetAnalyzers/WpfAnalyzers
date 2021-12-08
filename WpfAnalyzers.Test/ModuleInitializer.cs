namespace WpfAnalyzers.Test
{
    using System.Runtime.CompilerServices;
    using Gu.Roslyn.Asserts;

    internal static class ModuleInitializer
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            Settings.Default = Settings.Default.WithMetadataReferences(
                MetadataReferences.Transitive(
                    typeof(System.Windows.Window)));
        }
    }
}
