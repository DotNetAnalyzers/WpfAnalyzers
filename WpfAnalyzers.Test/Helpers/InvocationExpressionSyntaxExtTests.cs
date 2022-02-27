namespace WpfAnalyzers.Test;

using Gu.Roslyn.AnalyzerExtensions;
using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

public static class InvocationExpressionSyntaxExtTests
{
    [TestCase("Method1()",            "Method1")]
    [TestCase("this.Method1()",       "Method1")]
    [TestCase("new Foo()?.Method1()", "Method1")]
    [TestCase("this.Method2<int>()",  "Method2")]
    public static void TryGetInvokedMethodName(string invocationString, string expected)
    {
        var code = @"
namespace N
{
    public class Foo
    {
        public Foo()
        {
            var i = Method1();
            i = this.Method1();
            i = new Foo()?.Method1() ?? 0;
            i = Method2<int>();
            i = this.Method2<int>();
        }

        private int Method1() => 1;

        private int Method2<T>() => 2;
    }
}";
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var invocation = syntaxTree.FindInvocation(invocationString);
        Assert.AreEqual(true,     invocation.TryGetMethodName(out var name));
        Assert.AreEqual(expected, name);
    }
}