namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
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
            Descriptors.WPF0004ClrMethodShouldMatchRegisteredName,
            Descriptors.WPF0013ClrMethodMustMatchRegisteredType,
            Descriptors.WPF0033UseAttachedPropertyBrowsableForTypeAttribute,
            Descriptors.WPF0034AttachedPropertyBrowsableForTypeAttributeArgument,
            Descriptors.WPF0042AvoidSideEffectsInClrAccessors,
            Descriptors.WPF0061DocumentClrMethod);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => Handle(x), SyntaxKind.MethodDeclaration);
        }

        private static void Handle(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is MethodDeclarationSyntax methodDeclaration &&
                context.ContainingSymbol is IMethodSymbol method &&
                method.IsStatic &&
                method.Parameters.TryElementAt(0, out var parameter) &&
                parameter.Type.IsAssignableTo(KnownSymbols.DependencyObject, context.Compilation))
            {
                if (ClrMethod.IsAttachedGet(methodDeclaration, context.SemanticModel, context.CancellationToken, out var getValueCall, out var fieldOrProperty))
                {
                    if (DependencyProperty.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out _, out var registeredName))
                    {
                        if (!method.Name.IsParts("Get", registeredName))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0004ClrMethodShouldMatchRegisteredName,
                                    methodDeclaration.Identifier.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add("ExpectedName", "Get" + registeredName),
                                    method.Name,
                                    "Get" + registeredName));
                        }

                        if (method.DeclaredAccessibility.IsEither(Accessibility.Protected, Accessibility.Internal, Accessibility.Public) &&
                            !HasStandardText(methodDeclaration, fieldOrProperty, registeredName, out var location))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0061DocumentClrMethod, location));
                        }
                    }

                    if (DependencyProperty.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredType) &&
                        !Equals(method.ReturnType, registeredType))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0013ClrMethodMustMatchRegisteredType,
                                methodDeclaration.ReturnType.GetLocation(),
                                "Return type",
                                registeredType));
                    }

                    if (Attribute.TryFind(methodDeclaration, KnownSymbols.AttachedPropertyBrowsableForTypeAttribute, context.SemanticModel, context.CancellationToken, out var attribute))
                    {
                        if (attribute.TrySingleArgument(out var argument) &&
                            argument.Expression is TypeOfExpressionSyntax typeOf &&
                            TypeOf.TryGetType(typeOf, method.ContainingType, context.SemanticModel, context.CancellationToken, out var argumentType) &&
                            !argumentType.IsAssignableTo(parameter.Type, context.Compilation))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0034AttachedPropertyBrowsableForTypeAttributeArgument,
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
                                    Descriptors.WPF0033UseAttachedPropertyBrowsableForTypeAttribute,
                                    methodDeclaration.Identifier.GetLocation(),
                                    parameter.Type.ToMinimalDisplayString(context.SemanticModel, methodDeclaration.SpanStart)));
                    }

                    if (methodDeclaration.Body is { } body &&
                        TryGetSideEffect(body, getValueCall, out var sideEffect))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0042AvoidSideEffectsInClrAccessors, sideEffect.GetLocation()));
                    }
                }
                else if (ClrMethod.IsAttachedSet(methodDeclaration, context.SemanticModel, context.CancellationToken, out var setValueCall, out fieldOrProperty))
                {
                    if (DependencyProperty.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out _, out var registeredName))
                    {
                        if (!method.Name.IsParts("Set", registeredName))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0004ClrMethodShouldMatchRegisteredName,
                                    methodDeclaration.Identifier.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add("ExpectedName", "Set" + registeredName),
                                    method.Name,
                                    "Set" + registeredName));
                        }

                        if (method.DeclaredAccessibility.IsEither(Accessibility.Protected, Accessibility.Internal, Accessibility.Public) &&
                            !HasStandardText(methodDeclaration, fieldOrProperty, registeredName, out var location))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0061DocumentClrMethod, location));
                        }
                    }

                    if (DependencyProperty.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out var registeredType) &&
                        method.Parameters.TryElementAt(1, out var valueParameter) &&
                        !Equals(valueParameter.Type, registeredType))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0013ClrMethodMustMatchRegisteredType,
                                methodDeclaration.ParameterList.Parameters[1].Type.GetLocation(),
                                "Value type",
                                registeredType));
                    }

                    if (methodDeclaration.Body is { } body &&
                        TryGetSideEffect(body, setValueCall, out var sideEffect))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0042AvoidSideEffectsInClrAccessors, sideEffect.GetLocation()));
                    }
                }
            }
        }

        private static bool TryGetSideEffect(BlockSyntax body, InvocationExpressionSyntax getOrSet, [NotNullWhen(true)] out StatementSyntax? sideEffect)
        {
            foreach (var statement in body.Statements)
            {
                switch (statement)
                {
                    case ExpressionStatementSyntax { Expression: { } expression }
                        when expression == getOrSet:
                        continue;
                    case ReturnStatementSyntax { Expression: { } expression }
                        when expression == getOrSet:
                        continue;
                    case ReturnStatementSyntax { Expression: CastExpressionSyntax { Expression: { } expression } }
                        when expression == getOrSet:
                        continue;
                    case IfStatementSyntax { Condition: { } condition, Statement: ThrowStatementSyntax { }, Else: null }
                        when NullCheck.IsNullCheck(condition, null, CancellationToken.None, out _):
                        continue;
                    case IfStatementSyntax { Condition: { } condition, Statement: BlockSyntax { Statements: { Count: 0 } }, Else: null }
                        when NullCheck.IsNullCheck(condition, null, CancellationToken.None, out _):
                        continue;
                    case IfStatementSyntax { Condition: { } condition, Statement: BlockSyntax { Statements: { Count: 1 } statements }, Else: null }
                        when statements[0] is ThrowStatementSyntax &&
                             NullCheck.IsNullCheck(condition, null, CancellationToken.None, out _):
                        continue;
                    case IfStatementSyntax { Statement: null, Else: null }:
                        continue;
                    default:
                        sideEffect = statement;
                        return true;
                }
            }

            sideEffect = null;
            return false;
        }

        private static bool HasStandardText(MethodDeclarationSyntax methodDeclaration, BackingFieldOrProperty backingField, string registeredName, [NotNullWhen(true)] out Location? location)
        {
            location = null;
            if (methodDeclaration.ParameterList is { } parameterList &&
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
