namespace WpfAnalyzers.Test
{
    using System.Threading;
    using Gu.Roslyn.Asserts;
    using Microsoft.CodeAnalysis.CSharp;
    using NUnit.Framework;

    public static class ValueConverterTests
    {
        [Test]
        public static void TryGetConversionTypesDirectCast()
        {
            var code = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public sealed class CountConverter : IValueConverter
    {
        /// <summary> Gets the default instance </summary>
        public static readonly CountConverter Default = new CountConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ICollection)value).Count;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var classDeclaration = syntaxTree.FindClassDeclaration("CountConverter");
            Assert.AreEqual(true, ValueConverter.TryGetConversionTypes(classDeclaration, semanticModel, CancellationToken.None, out var sourceType, out var targetType));
            Assert.AreEqual("ICollection", sourceType.Name);
            Assert.AreEqual("Int32", targetType.Name);
        }

        [Test]
        public static void TryGetConversionTypesAsCast()
        {
            var code = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public sealed class CountConverter : IValueConverter
    {
        /// <summary> Gets the default instance </summary>
        public static readonly CountConverter Default = new CountConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var col = value as ICollection;
            if (col != null)
            {
                return col.Count;
            }

            return 0;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var classDeclaration = syntaxTree.FindClassDeclaration("CountConverter");
            Assert.AreEqual(true, ValueConverter.TryGetConversionTypes(classDeclaration, semanticModel, CancellationToken.None, out var sourceType, out var targetType));
            Assert.AreEqual("ICollection", sourceType.Name);
            Assert.AreEqual("Int32", targetType.Name);
        }

        [Test]
        public static void TryGetConversionTypesTwoAsCastWhenOneIsOther()
        {
            var code = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public sealed class CountConverter : IValueConverter
    {
        /// <summary> Gets the default instance </summary>
        public static readonly CountConverter Default = new CountConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var c = value as ICollection;
            if (c != null)
            {
                return c.Count;
            }

            var e = value as IEnumerable;
            if (e != null)
            {
                var num = 0;
                var enumerator = e.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    checked
                    {
                        ++num;
                    }
                }

                (enumerator as IDisposable)?.Dispose();
                return num;
            }

            return 0;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var classDeclaration = syntaxTree.FindClassDeclaration("CountConverter");
            Assert.AreEqual(true, ValueConverter.TryGetConversionTypes(classDeclaration, semanticModel, CancellationToken.None, out var sourceType, out var targetType));
            Assert.AreEqual("IEnumerable", sourceType.Name);
            Assert.AreEqual("Int32", targetType.Name);
        }

        [Test]
        public static void TryGetConversionTypesTwoAsCastListAndArray()
        {
            var code = @"
namespace N
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(IEnumerable), typeof(int))]
    public sealed class CountConverter : IValueConverter
    {
        /// <summary> Gets the default instance </summary>
        public static readonly CountConverter Default = new CountConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = value as List<int>;
            if (list != null)
            {
                return list.Count;
            }

            var array = value as int[];
            if (array != null)
            {
                var num = 0;
                var enumerator = array.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    checked
                    {
                        ++num;
                    }
                }

                (enumerator as IDisposable)?.Dispose();
                return num;
            }

            return 0;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}";
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create("test", new[] { syntaxTree }, MetadataReferences.FromAttributes());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var classDeclaration = syntaxTree.FindClassDeclaration("CountConverter");
            Assert.AreEqual(true, ValueConverter.TryGetConversionTypes(classDeclaration, semanticModel, CancellationToken.None, out var sourceType, out var targetType));
            Assert.AreEqual("IList`1", sourceType.MetadataName);
            Assert.AreEqual("Int32", targetType.MetadataName);
        }
    }
}
