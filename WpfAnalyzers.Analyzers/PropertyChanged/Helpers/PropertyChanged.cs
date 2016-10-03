namespace WpfAnalyzers.PropertyChanged.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class PropertyChanged
    {
        internal static IEnumerable<string> RaisesPropertyChangedFor(this AccessorDeclarationSyntax setter, SemanticModel semanticModel)
        {
            foreach (var invocation in setter.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var symbolInfo = semanticModel.GetSymbolInfo(invocation);
                var methodSymbol = symbolInfo.Symbol as IMethodSymbol;
                if (methodSymbol == null)
                {
                    continue;
                }

                if (methodSymbol.ContainingType.Name == "PropertyChangedEventHandler" &&
                    methodSymbol.Name == "Invoke")
                {
                    var newPropChangeArgs = (ObjectCreationExpressionSyntax)invocation.ArgumentList.Arguments[1].Expression;
                    string name;
                    if (newPropChangeArgs.ArgumentList.Arguments[0].TryGetString(semanticModel, out name))
                    {
                        yield return name;
                    }

                    continue;
                }

                if (methodSymbol.Name == "OnPropertyChanged")
                {
                    if(methodSymbol.Parameters[0].GetAttributes())
                    throw new NotImplementedException();
                }
            }
        }
    }
}
