namespace WpfAnalyzers.Test
{
    using System.Threading;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;
    using WpfAnalyzers.PropertyChanged.Helpers;

    public class PropertyTests
    {
        [TestCase("Value1", false)]
        [TestCase("Value2", false)]
        [TestCase("Value3", false)]
        [TestCase("Value4", false)]
        [TestCase("Lazy1", true)]
        [TestCase("Lazy2", true)]
        [TestCase("Lazy3", true)]
        [TestCase("Lazy4", true)]
        [TestCase("Lazy5", true)]
        public void IsLazy(string code, bool expected)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(@"
namespace RoslynSandBox
{
    using System;

    public class Foo
    {
        private readonly int value;
        private int value5;

        private string lazy1;
        private string lazy2;
        private string lazy3;
        private string lazy4;
        private Action lazy5;
 
        public int Value1 { get; }

        public int Value2 => this.value;
       
        public string Lazy1
        {
            get
            {
                if (this.lazy1 != null)
                {
                    return this.lazy1;
                }

                this.lazy1 = new string(' ', 1);
                return this.lazy1;
            }
        }

        public string Lazy2
        {
            get
            {
                if (this.lazy2 == null)
                {
                    this.lazy2 = new string(' ', 1);
                }

                return this.lazy2;
            }
        }

        public string Lazy3
        {
            get
            {
                return this.lazy3 ?? (this.lazy3 = new string(' ', 1));
            }
        }

        public string Lazy4 => this.lazy4 ?? (this.lazy4 = new string(' ', 1));

        public Action Lazy5 => this.lazy5 ?? (this.lazy5 = new Action(() => this.lazy5 = null));

        public int Value3
        {
            get
            {
                this.value;
            }
        }

        public int Value4 { get; set; }

        public int Value5
        {
            get { return this.value5; }
            set { this.value5 = value; }
        }
    }
}");
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.All);
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var property = syntaxTree.PropertyDeclarationSyntax(code);
            Assert.AreEqual(expected, Property.IsLazy(property, semanticModel, CancellationToken.None));
        }
    }
}
