namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class ClrMethodDeclarationAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0004ClrMethodShouldMatchRegisteredName.Descriptor,
            WPF0013ClrMethodMustMatchRegisteredType.Descriptor,
            WPF0033UseAttachedPropertyBrowsableForTypeAttribute.Descriptor,
            WPF0034AttachedPropertyBrowsableForTypeAttributeArgument.Descriptor,
            WPF0042AvoidSideEffectsInClrAccessors.Descriptor,
            WPF0061DocumentClrMethod.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(Handle, SyntaxKind.MethodDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is MethodDeclarationSyntax methodDeclaration &&
                context.ContainingSymbol is IMethodSymbol method &&
                method.IsStatic &&
                method.Parameters.TryElementAt(0, out var parameter) &&
                parameter.Type.IsAssignableTo(KnownSymbol.DependencyObject, context.Compilation))
            {
                if (ClrMethod.IsAttachedGet(methodDeclaration, context.SemanticModel, context.CancellationToken, out var getValueCall, out var fieldOrProperty))
                {
                    if (DependencyProperty.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredName) &&
                        !method.Name.IsParts("Get", registeredName))
                    {
                        context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0004ClrMethodShouldMatchRegisteredName.Descriptor,
                            methodDeclaration.Identifier.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add("ExpectedName", "Get" + registeredName),
                            method.Name,
                            "Get" + registeredName));
                    }

                    if (DependencyProperty.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredType) &&
                        !Equals(method.ReturnType, registeredType))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                WPF0013ClrMethodMustMatchRegisteredType.Descriptor,
                                methodDeclaration.ReturnType.GetLocation(),
                                "Return type",
                                registeredType));
                    }

                    if (Attribute.TryFind(methodDeclaration, KnownSymbol.AttachedPropertyBrowsableForTypeAttribute, context.SemanticModel, context.CancellationToken, out var attribute))
                    {
                        if (attribute.TrySingleArgument(out var argument) &&
                            argument.Expression is TypeOfExpressionSyntax typeOf &&
                            TypeOf.TryGetType(typeOf, method.ContainingType, context.SemanticModel, context.CancellationToken, out var argumentType) &&
                            !argumentType.IsAssignableTo(parameter.Type, context.Compilation))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    WPF0034AttachedPropertyBrowsableForTypeAttributeArgument.Descriptor,
                                    argument.GetLocation(),
                                    parameter.Type.ToMinimalDisplayString(
                                        context.SemanticModel,
                                        argument.SpanStart)));
                        }
                    }
                    else
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                    WPF0033UseAttachedPropertyBrowsableForTypeAttribute.Descriptor,
                                    methodDeclaration.Identifier.GetLocation(),
                                    parameter.Type.ToMinimalDisplayString(context.SemanticModel, methodDeclaration.SpanStart)));
                    }

                    if (methodDeclaration.Body is BlockSyntax body &&
                        body.Statements.TryFirst(x => !x.Contains(getValueCall), out var statement))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(WPF0042AvoidSideEffectsInClrAccessors.Descriptor, statement.GetLocation()));
                    }

                    if (method.DeclaredAccessibility.IsEither(Accessibility.Protected, Accessibility.Internal, Accessibility.Public) &&
                        !HasStandardText(methodDeclaration, fieldOrProperty, registeredName, out var location))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(WPF0061DocumentClrMethod.Descriptor, location));
                    }
                }
                else if (ClrMethod.IsAttachedSet(methodDeclaration, context.SemanticModel, context.CancellationToken, out var setValueCall, out fieldOrProperty))
                {
                    if (DependencyProperty.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredName) &&
                        !method.Name.IsParts("Set", registeredName))
                    {
                        context.ReportDiagnostic(
                        Diagnostic.Create(
                            WPF0004ClrMethodShouldMatchRegisteredName.Descriptor,
                            methodDeclaration.Identifier.GetLocation(),
                            ImmutableDictionary<string, string>.Empty.Add("ExpectedName", "Set" + registeredName),
                            method.Name,
                            "Set" + registeredName));
                    }

                    if (DependencyProperty.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredType) &&
                        method.Parameters.TryElementAt(1, out var valueParameter) &&
                        !Equals(valueParameter.Type, registeredType))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                WPF0013ClrMethodMustMatchRegisteredType.Descriptor,
                                methodDeclaration.ParameterList.Parameters[1].Type.GetLocation(),
                                "Value type",
                                registeredType));
                    }

                    if (methodDeclaration.Body is BlockSyntax body &&
                        body.Statements.TryFirst(x => !x.Contains(setValueCall), out var statement))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(WPF0042AvoidSideEffectsInClrAccessors.Descriptor, statement.GetLocation()));
                    }

                    if (method.DeclaredAccessibility.IsEither(Accessibility.Protected, Accessibility.Internal, Accessibility.Public) &&
                        !HasStandardText(methodDeclaration, fieldOrProperty, registeredName, out var location))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(WPF0061DocumentClrMethod.Descriptor, location));
                    }
                }
            }
        }

        private static bool HasStandardText(MethodDeclarationSyntax methodDeclaration, BackingFieldOrProperty backingField, string registeredName, out Location location)
        {
            location = null;
            if (methodDeclaration.ParameterList is ParameterListSyntax parameterList &&
                parameterList.Parameters.TryElementAt(0, out var parameter))
            {
                if (parameterList.Parameters.Count == 1)
                {
                    if (HasDocComment(out var comment, out location) &&
                        HasSummary(comment, $"<summary>Helper for getting <see cref=\"{backingField.Name}\"/> from <paramref name=\"{parameter.Identifier.ValueText}\"/>.</summary>", out location) &&
                        HasParam(comment, parameter, $"<param name=\"{parameter.Identifier.ValueText}\"><see cref=\"{parameter.Type}\"/> to read <see cref=\"{backingField.Name}\"/> from.</param>", out location) &&
                        HasReturns(comment, $"<returns>{registeredName} property value.</returns>", out location))
                    {
                        location = null;
                        return true;
                    }

                    return false;
                }

                if (parameterList.Parameters.Count == 2)
                {
                    return HasDocComment(out var comment, out location) &&
                           HasSummary(comment, $"<summary>Helper for setting <see cref=\"{backingField.Name}\"/> on <paramref name=\"{parameter.Identifier.ValueText}\"/>.</summary>", out location) &&
                           HasParam(comment, parameter, $"<param name=\"{parameter.Identifier.ValueText}\"><see cref=\"{parameter.Type}\"/> to set <see cref=\"{backingField.Name}\"/> on.</param>", out location) &&
                           parameterList.Parameters.TryElementAt(1, out parameter) &&
                           HasParam(comment, parameter, $"<param name=\"{parameter.Identifier.ValueText}\">{registeredName} property value.</param>", out location);
                }
            }

            return false;

            bool HasDocComment(out DocumentationCommentTriviaSyntax comment, out Location errorLocation)
            {
                if (methodDeclaration.TryGetDocumentationComment(out comment))
                {
                    errorLocation = null;
                    return true;
                }

                errorLocation = methodDeclaration.Identifier.GetLocation();
                return false;
            }

            bool HasSummary(DocumentationCommentTriviaSyntax comment, string expected, out Location errorLocation)
            {
                if (comment.TryGetSummary(out var summary))
                {
                    if (summary.ToString() == expected)
                    {
                        errorLocation = null;
                        return true;
                    }

                    errorLocation = summary.GetLocation();
                    return false;
                }

                errorLocation = comment.GetLocation();
                return false;
            }

            bool HasParam(DocumentationCommentTriviaSyntax comment, ParameterSyntax current, string expected, out Location errorLocation)
            {
                if (comment.TryGetParam(current.Identifier.ValueText, out var param))
                {
                    if (param.ToString() == expected)
                    {
                        errorLocation = null;
                        return true;
                    }

                    errorLocation = param.GetLocation();
                    return false;
                }

                errorLocation = comment.GetLocation();
                return false;
            }

            bool HasReturns(DocumentationCommentTriviaSyntax comment, string expected, out Location errorLocation)
            {
                if (comment.TryGetReturns(out var returns))
                {
                    if (returns.ToString() == expected)
                    {
                        errorLocation = null;
                        return true;
                    }

                    errorLocation = returns.GetLocation();
                    return false;
                }

                errorLocation = comment.GetLocation();
                return false;
            }
        }
    }
}
