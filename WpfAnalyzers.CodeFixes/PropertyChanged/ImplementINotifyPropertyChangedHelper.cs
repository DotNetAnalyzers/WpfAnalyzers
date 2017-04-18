namespace WpfAnalyzers
{
    using System;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    internal static class ImplementINotifyPropertyChangedHelper
    {
        private const string OnPropertyChanged = "OnPropertyChanged";

        // ReSharper disable once InconsistentNaming
        private static readonly TypeSyntax INotifyPropertyChangedInterface = SyntaxFactory.ParseTypeName("INotifyPropertyChanged");

        private static readonly StatementSyntax[] ThisPropertyChangedInvokeStatements =
        {
            SyntaxFactory.ParseStatement("this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));")
        };

        private static readonly StatementSyntax[] PropertyChangedInvokeStatements =
        {
            SyntaxFactory.ParseStatement("PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));")
        };

        private static readonly SeparatedSyntaxList<ParameterSyntax> InvokerParameters =
            SyntaxFactory.ParseParameterList("([CallerMemberName] string propertyName = null)").Parameters;

        private static readonly UsingDirectiveSyntax UsingSystemComponentModel = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.ComponentModel"));
        private static readonly UsingDirectiveSyntax UsingSystemRuntimeCompilerServices = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Runtime.CompilerServices"));

        internal static TypeDeclarationSyntax WithPropertyChangedEvent(this TypeDeclarationSyntax typeDeclaration, SyntaxGenerator syntaxGenerator)
        {
            foreach (var member in typeDeclaration.Members)
            {
                var eventFieldDeclaration = member as EventFieldDeclarationSyntax;
                if (eventFieldDeclaration == null)
                {
                    continue;
                }

                foreach (var variable in eventFieldDeclaration.Declaration.Variables)
                {
                    if (variable.Identifier.ValueText == "PropertyChanged")
                    {
                        return typeDeclaration;
                    }
                }
            }

            var propertyChangedEvent = (EventFieldDeclarationSyntax)syntaxGenerator.EventDeclaration(
                "PropertyChanged",
                SyntaxFactory.ParseTypeName("PropertyChangedEventHandler"),
                Accessibility.Public);

            if (typeDeclaration.Members.TryGetFirst(
                                   x => x.IsKind(SyntaxKind.EventDeclaration) ||
                                        x.IsKind(SyntaxKind.PropertyDeclaration) ||
                                        x.IsKind(SyntaxKind.MethodDeclaration),
                                   out MemberDeclarationSyntax existingMember))
            {
                return typeDeclaration.InsertNodesBefore(existingMember, new[] { propertyChangedEvent });
            }

            if (typeDeclaration.Members.TryGetLast(
                                   x => x.IsKind(SyntaxKind.FieldDeclaration) ||
                                        x.IsKind(SyntaxKind.ConstructorDeclaration),
                                   out existingMember))
            {
                return typeDeclaration.InsertNodesAfter(existingMember, new[] { propertyChangedEvent });
            }

            return (TypeDeclarationSyntax)syntaxGenerator.AddMembers(typeDeclaration, propertyChangedEvent);
        }

        internal static TypeDeclarationSyntax WithInvoker(
            this TypeDeclarationSyntax typeDeclaration,
            SyntaxGenerator syntaxGenerator,
            ITypeSymbol type)
        {
            foreach (var member in typeDeclaration.Members)
            {
                var method = member as MethodDeclarationSyntax;
                if (method?.Identifier.ValueText == OnPropertyChanged)
                {
                    if (method.ParameterList.Parameters.Count != 1)
                    {
                        continue;
                    }

                    if (string.Equals((method.ParameterList.Parameters[0].Type as PredefinedTypeSyntax)?.Keyword.ValueText, "string", StringComparison.OrdinalIgnoreCase))
                    {
                        return typeDeclaration;
                    }
                }
            }

            var invoker = (MethodDeclarationSyntax)syntaxGenerator.MethodDeclaration(
                OnPropertyChanged,
                accessibility: type.IsSealed ? Accessibility.Private : Accessibility.Protected,
                modifiers: type.IsSealed ? DeclarationModifiers.None : DeclarationModifiers.Virtual,
                parameters: InvokerParameters,
                statements: typeDeclaration.UsesUnderscoreNames() ? PropertyChangedInvokeStatements : ThisPropertyChangedInvokeStatements);

            if (typeDeclaration.Members.TryGetFirst(
                       x =>
                       {
                           var methodDeclarationSyntax = x as MethodDeclarationSyntax;
                           return methodDeclarationSyntax != null &&
                                  (methodDeclarationSyntax.Modifiers.Any(SyntaxKind.PrivateKeyword) ||
                                   methodDeclarationSyntax.Modifiers.Any(SyntaxKind.ProtectedKeyword));
                       },
                       out MemberDeclarationSyntax existsingMember))
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
            if (!TryGetPart(usingDirective.Name, 1, out string first))
            {
                return add(node, usingDirective);
            }

            var usings = selector(node);
            foreach (var @using in usings)
            {
                if (!TryGetPart(@using.Name, 0, out string fst) ||
    fst != "System")
                {
                    return add(node, usingDirective);
                }

                if (!TryGetPart(@using.Name, 1, out string other))
                {
                    continue;
                }

                if (string.Compare(first, other, StringComparison.Ordinal) < 0)
                {
                    return node.InsertNodesBefore(@using, new[] { usingDirective });
                }
            }

            return add(node, usingDirective);
        }

        private static bool TryGetPart(NameSyntax name, int index, out string part)
        {
            part = null;
            if (name is IdentifierNameSyntax identifierName)
            {
                if (index == 0)
                {
                    part = identifierName.Identifier.ValueText;
                }

                return part != null;
            }

            if (name is QualifiedNameSyntax qualifiedName)
            {
                var left = qualifiedName;
                while (left.Left is QualifiedNameSyntax)
                {
                    left = (QualifiedNameSyntax)left.Left;
                }

                if (index == 0)
                {
                    return TryGetPart(left.Left, 0, out part);
                }

                if (index == 1)
                {
                    part = left.Right.Identifier.ValueText;
                }
            }

            return part != null;
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