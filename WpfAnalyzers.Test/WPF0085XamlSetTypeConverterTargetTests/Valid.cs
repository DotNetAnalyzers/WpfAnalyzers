﻿namespace WpfAnalyzers.Test.WPF0085XamlSetTypeConverterTargetTests;

using Gu.Roslyn.Asserts;
using NUnit.Framework;

public static class Valid
{
    private static readonly AttributeAnalyzer Analyzer = new();

    [Test]
    public static void WhenCorrectSignature()
    {
        var code = @"
namespace N
{
    using System.Windows.Controls;
    using System.Windows.Markup;

    [XamlSetTypeConverter(nameof(ReceiveTypeConverter))]
    public class FooControl : Control
    {
        public static void ReceiveTypeConverter(object targetObject, XamlSetTypeConverterEventArgs eventArgs)
        {
        }
    }
}";
        RoslynAssert.Valid(Analyzer, code);
    }
}
