namespace WpfAnalyzers
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    internal static class ImplementINotifyPropertyChangedHelper
    {
        // ReSharper disable once InconsistentNaming
        private static readonly TypeSyntax INotifyPropertyChangedInterface = SyntaxFactory.ParseTypeName("INotifyPropertyChanged");

        private static readonly StatementSyntax[] InvokeStatements =
        {
            SyntaxFactory.ParseStatement("this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));")
        };

        private static readonly SeparatedSyntaxList<ParameterSyntax> InvokerParameters =
            SyntaxFactory.ParseParameterList("([CallerMemberName] string propertyName = null)").Parameters;

        private static readonly UsingDirectiveSyntax UsingSystemComponentModel = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.ComponentModel"));
        private static readonly UsingDirectiveSyntax UsingSystemRuntimeCompilerServices = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Runtime.CompilerServices"));

        internal static TypeDeclarationSyntax WithPropertyChangedEvent(
            this TypeDeclarationSyntax typeDeclaration,
            SyntaxGenerator syntaxGenerator)
        {
            var propertyChangedEvent = (EventFieldDeclarationSyntax)syntaxGenerator.EventDeclaration(
                "PropertyChanged",
                SyntaxFactory.ParseTypeName("PropertyChangedEventHandler"),
                Accessibility.Public);

            MemberDeclarationSyntax existsingMember;

            if (typeDeclaration.Members.TryGetFirst(
                                   x => x.IsKind(SyntaxKind.EventDeclaration) ||
                                        x.IsKind(SyntaxKind.PropertyDeclaration) ||
                                        x.IsKind(SyntaxKind.MethodDeclaration),
                                   out existsingMember))
            {
                return typeDeclaration.InsertNodesBefore(existsingMember, new[] { propertyChangedEvent });
            }

            if (typeDeclaration.Members.TryGetLast(
                                   x => x.IsKind(SyntaxKind.FieldDeclaration) ||
                                        x.IsKind(SyntaxKind.ConstructorDeclaration),
                                   out existsingMember))
            {
                return typeDeclaration.InsertNodesAfter(existsingMember, new[] { propertyChangedEvent });
            }

            return (TypeDeclarationSyntax)syntaxGenerator.AddMembers(typeDeclaration, propertyChangedEvent);
        }

        internal static TypeDeclarationSyntax WithInvoker(
            this TypeDeclarationSyntax typeDeclaration,
            SyntaxGenerator syntaxGenerator,
            ITypeSymbol type)
        {
            var invoker = (MethodDeclarationSyntax)syntaxGenerator.MethodDeclaration(
                "OnPropertyChanged",
                accessibility: Accessibility.Protected,
                modifiers: type.IsSealed ? DeclarationModifiers.None : DeclarationModifiers.Virtual,
                parameters: InvokerParameters,
                statements: InvokeStatements);

            MemberDeclarationSyntax existsingMember;
            if (typeDeclaration.Members.TryGetFirst(
                                   x =>
                                   {
                                       var methodDeclarationSyntax = x as MethodDeclarationSyntax;
                                       return methodDeclarationSyntax != null &&
                                              (methodDeclarationSyntax.Modifiers.Any(SyntaxKind.PrivateKeyword) ||
                                               methodDeclarationSyntax.Modifiers.Any(SyntaxKind.ProtectedKeyword));
                                   },
                                   out existsingMember))
            {
                return typeDeclaration.InsertNodesBefore(existsingMember, new[] { invoker });
            }

            return (TypeDeclarationSyntax)syntaxGenerator.AddMembers(typeDeclaration, invoker);
        }

        internal static TypeDeclarationSyntax WithINotifyPropertyChangedInterface(
            this TypeDeclarationSyntax typeDeclaration,
            SyntaxGenerator syntaxGenerator,
            ITypeSymbol type)
        {
            var baseList = typeDeclaration.BaseList;
            if (baseList != null)
            {
                foreach (var baseType in baseList.Types)
                {
                    if ((baseType.Type as IdentifierNameSyntax)?.Identifier.ValueText.Contains("INotifyPropertyChanged") == true)
                    {
                        return typeDeclaration;
                    }
                }
            }

            if (!type.Is(KnownSymbol.INotifyPropertyChanged))
            {
                return (TypeDeclarationSyntax)syntaxGenerator.AddInterfaceType(typeDeclaration, INotifyPropertyChangedInterface);
            }

            return typeDeclaration;
        }

        internal static CompilationUnitSyntax WithUsings(this CompilationUnitSyntax syntaxRoot)
        {
            return syntaxRoot.WithUsing(UsingSystemComponentModel)
                             .WithUsing(UsingSystemRuntimeCompilerServices);
        }

        private static CompilationUnitSyntax WithUsing(this CompilationUnitSyntax syntaxRoot, UsingDirectiveSyntax usingDirective)
        {
            var @namespace = syntaxRoot.Members.FirstOrDefault() as NamespaceDeclarationSyntax;
            if (@namespace == null)
            {
                if (syntaxRoot.Usings.HasUsing(usingDirective))
                {
                    return syntaxRoot;
                }

                return syntaxRoot.WithUsing(x => x.Usings, (x, u) => x.AddUsings(u), usingDirective);
            }

            if (@namespace.Usings.HasUsing(usingDirective) || syntaxRoot.Usings.HasUsing(usingDirective))
            {
                return syntaxRoot;
            }

            if (@namespace.Usings.Any() || !syntaxRoot.Usings.Any())
            {
                return syntaxRoot.ReplaceNode(@namespace, @namespace.WithUsing(x => x.Usings, (x, u) => x.AddUsings(u), usingDirective));
            }

            return syntaxRoot.WithUsing(x => x.Usings, (x, u) => x.AddUsings(u), usingDirective);
        }

        private static T WithUsing<T>(
            this T node,
            Func<T, SyntaxList<UsingDirectiveSyntax>> selector,
            Func<T, UsingDirectiveSyntax, T> add,
            UsingDirectiveSyntax usingDirective)
            where T : SyntaxNode
        {
            var insertName = (usingDirective.Name as IdentifierNameSyntax)?.Identifier.ValueText;
            if (insertName == null)
            {
                return add(node, usingDirective);
            }

            var usings = selector(node);
            if (!usings.Any())
            {
                return add(node, usingDirective);
            }

            foreach (var @using in usings)
            {
                var identifierNameSyntax = @using.Name as IdentifierNameSyntax;
                if (identifierNameSyntax != null)
                {
                    var name = identifierNameSyntax.Identifier.ValueText;
                    if (!name.StartsWith("System"))
                    {
                        return node.InsertNodesBefore(@using, new[] { usingDirective });
                    }

                    if (string.Compare(insertName, name, StringComparison.Ordinal) < 0)
                    {
                        return node.InsertNodesBefore(@using, new[] { usingDirective });
                    }
                }
            }

            return add(node, usingDirective);
        }

        private static bool HasUsing(this SyntaxList<UsingDirectiveSyntax> usings, UsingDirectiveSyntax @using)
        {
            foreach (var existing in usings)
            {
                if (existing.Name.IsEquivalentTo(@using.Name))
                {
                    return true;
                }
            }

            return false;
        }
    }
}