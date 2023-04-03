namespace WpfAnalyzers.Benchmarks.Benchmarks;

using Gu.Roslyn.Asserts;
using Microsoft.CodeAnalysis;

public static class Code
{
    public static Solution ValidCodeProject { get; } = CodeFactory.CreateSolution(
        ProjectFile.Find("ValidCode.csproj"));
}
