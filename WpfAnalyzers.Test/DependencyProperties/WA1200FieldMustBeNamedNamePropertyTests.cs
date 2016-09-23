﻿//namespace WpfAnalyzers.Test.DependencyProperties
//{
//    using System.Collections.Generic;
//    using System.Threading;
//    using System.Threading.Tasks;
//    using Microsoft.CodeAnalysis.CodeFixes;
//    using Microsoft.CodeAnalysis.Diagnostics;
//    using NUnit.Framework;
//    using WpfAnalyzers.DependencyProperties;

//    public class WA1200FieldMustBeNamedNamePropertyUnitTests : CodeFixVerifier
//    {
//        [Test]
//        public async Task HappyPath()
//        {
//            var testCode = @"
//    using System.Windows;
//    using System.Windows.Controls;

//    public class FooControl : Control
//    {
//        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
//            ""Bar"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

//        public int Bar
//        {
//            get { return (int) GetValue(BarProperty); }
//            set { SetValue(BarProperty, value); }
//        }
//    }";

//            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
//        }

//        [Test]
//        public async Task PropertyNotNamedNameProperty()
//        {
//            var testCode = @"
//    using System.Windows;
//    using System.Windows.Controls;

//    public class FooControl : Control
//    {
//        public static readonly DependencyProperty BarProperty = DependencyProperty.Register(
//            ""Error"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

//        public int Bar
//        {
//            get { return (int) GetValue(BarProperty); }
//            set { SetValue(BarProperty, value); }
//        }
//    }";

//            DiagnosticResult expected = this.CSharpDiagnostic().WithLocation(3, 30);

//            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);

//            var fixedCode = @"
//    using System.Windows;
//    using System.Windows.Controls;

//    public class FooControl : Control
//    {
//        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register(
//            ""Error"", typeof(int), typeof(FooControl), new PropertyMetadata(default(int)));

//        public int Bar
//        {
//            get { return (int) GetValue(ErrorProperty); }
//            set { SetValue(ErrorProperty, value); }
//        }
//    }";
//            await this.VerifyCSharpFixAsync(testCode, fixedCode).ConfigureAwait(false);
//        }

//        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
//        {
//            yield return new WA1200DependencyPropertyFieldMustBeNamedNameProperty();
//        }

//        protected override CodeFixProvider GetCSharpCodeFixProvider()
//        {
//            return new RenameDependencyPropertyFieldCodeFixProvider();
//        }
//    }
//}
