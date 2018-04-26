namespace WpfAnalyzers.Test
{
    using System;
    using System.Linq;
    using NUnit.Framework;

    [Explicit]
    public class Sandbox
    {
        [Test]
        public void Test()
        {
            var metaDataType = typeof(System.Windows.PropertyMetadata);
            foreach (var type in AppDomain.CurrentDomain
                                          .GetAssemblies()
                                          .SelectMany(a => a.GetTypes())
                                          .Where(t => t.IsPublic)
                                          .Where(t => metaDataType.IsAssignableFrom(t)))
            {
                Console.WriteLine($"internal static readonly QualifiedType {type.Name} = Create(\"{type.FullName}\");");
            }
        }
    }
}
