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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0004ClrMethodShouldMatchRegisteredName,
            Descriptors.WPF0013ClrMethodMustMatchRegisteredType,
            Descriptors.WPF0033UseAttachedPropertyBrowsableForTypeAttribute,
            Descriptors.WPF0034AttachedPropertyBrowsableForTypeAttributeArgument,
            Descriptors.WPF0042AvoidSideEffectsInClrAccessors,
            Descriptors.WPF0061DocumentClrMethod);

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
                context.ContainingSymbol is IMethodSymbol { IsStatic: true } method &&
                method.Parameters.TryElementAt(0, out var element) &&
                element.Type.IsAssignableTo(KnownSymbols.DependencyObject, context.Compilation))
            {
                if (ClrMethod.IsAttachedGet(methodDeclaration, context.SemanticModel, context.CancellationToken, out var getValueCall, out var backing))
                {
                    if (DependencyProperty.TryGetRegisteredName(backing, context.SemanticModel, context.CancellationToken, out _, out var registeredName))
                    {
                        if (!method.Name.IsParts("Get", registeredName))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0004ClrMethodShouldMatchRegisteredName,
                                    methodDeclaration.Identifier.GetLocation(),
                                    ImmutableDictionary<string, string?>.Empty.Add("ExpectedName", "Get" + registeredName),
                                    method.Name,
                                    "Get" + registeredName));
                        }

                        if (method.DeclaredAccessibility.IsEither(Accessibility.Protected, Accessibility.Internal, Accessibility.Public))
                        {
                            var summaryFormat = "<summary>Helper for getting <see cref=\"{backing}\"/> from <paramref name=\"{element}\"/>.</summary>";
                            var paramFormat = "<param name=\"{element}\"><see cref=\"{element.type}\"/> to read <see cref=\"{backing}\"/> from.</param>";
                            var returnsFormat = "<returns>{registered_name} property value.</returns>";
                            if (methodDeclaration.TryGetDocumentationComment(out var comment))
                            {
                                if (comment.VerifySummary(summaryFormat, backing.Symbol.Name, element.Name) is { } summaryError)
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.WPF0061DocumentClrMethod,
                                            summaryError.Location,
                                            ImmutableDictionary<string, string?>.Empty.Add(nameof(DocComment), summaryError.Text)));
                                }

                                if (comment.VerifyParameter(paramFormat, element, element.ToCrefType(), backing.Symbol.Name) is { } paramError)
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.WPF0061DocumentClrMethod,
                                            paramError.Location,
                                            ImmutableDictionary<string, string?>.Empty.Add(nameof(DocComment), paramError.Text)));
                                }

                                if (comment.VerifyReturns(returnsFormat, registeredName) is { } returnsError)
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.WPF0061DocumentClrMethod,
                                            returnsError.Location,
                                            ImmutableDictionary<string, string?>.Empty.Add(nameof(DocComment), returnsError.Text)));
                                }
                            }
                            else
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptors.WPF0061DocumentClrMethod,
                                        methodDeclaration.Identifier.GetLocation(),
                                        ImmutableDictionary<string, string?>.Empty.Add(
                                            nameof(DocComment),
#pragma warning disable SA1118 // Parameter should not span multiple lines
                                            $"/// {DocComment.Format(summaryFormat, backing.Symbol.Name, element.Name)}\n" +
                                            $"/// {DocComment.Format(paramFormat, element.Name, element.ToCrefType(), backing.Name)}\n" +
                                            $"/// {DocComment.Format(returnsFormat, registeredName)}\n")));
#pragma warning restore SA1118 // Parameter should not span multiple lines
                            }
                        }
                    }

                    if (DependencyProperty.TryGetRegisteredType(backing, context.SemanticModel, context.CancellationToken, out var registeredType) &&
                        !SymbolEqualityComparer.Equal(method.ReturnType, registeredType))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0013ClrMethodMustMatchRegisteredType,
                                methodDeclaration.ReturnType.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(TypeSyntax), registeredType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart)),
                                "Return type",
                                registeredType));
                    }

                    if (Attribute.TryFind(methodDeclaration, KnownSymbols.AttachedPropertyBrowsableForTypeAttribute, context.SemanticModel, context.CancellationToken, out var attribute))
                    {
                        if (attribute.TrySingleArgument(out var argument) &&
                            argument.Expression is TypeOfExpressionSyntax typeOf &&
                            TypeSymbol.TryGet(typeOf, method.ContainingType, context.SemanticModel, context.CancellationToken, out var argumentType) &&
                            !argumentType.IsAssignableTo(element.Type, context.Compilation))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0034AttachedPropertyBrowsableForTypeAttributeArgument,
                                    argument.GetLocation(),
                                    element.Type.ToMinimalDisplayString(
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
                                    element.Type.ToMinimalDisplayString(context.SemanticModel, methodDeclaration.SpanStart)));
                    }

                    if (methodDeclaration.Body is { } body &&
                        TryGetSideEffect(body, getValueCall, out var sideEffect))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.WPF0042AvoidSideEffectsInClrAccessors, sideEffect.GetLocation()));
                    }
                }
                else if (method.Parameters.TryElementAt(1, out var value) &&
                         ClrMethod.IsAttachedSet(methodDeclaration, context.SemanticModel, context.CancellationToken, out var setValueCall, out backing))
                {
                    if (DependencyProperty.TryGetRegisteredName(backing, context.SemanticModel, context.CancellationToken, out _, out var registeredName))
                    {
                        if (!method.Name.IsParts("Set", registeredName))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0004ClrMethodShouldMatchRegisteredName,
                                    methodDeclaration.Identifier.GetLocation(),
                                    ImmutableDictionary<string, string?>.Empty.Add("ExpectedName", "Set" + registeredName),
                                    method.Name,
                                    "Set" + registeredName));
                        }

                        if (method.DeclaredAccessibility.IsEither(Accessibility.Protected, Accessibility.Internal, Accessibility.Public))
                        {
                            var summaryFormat = "<summary>Helper for setting <see cref=\"{backing}\"/> on <paramref name=\"{element}\"/>.</summary>";
                            var elementFormat = "<param name=\"{element}\"><see cref=\"{element_type}\"/> to set <see cref=\"{backing}\"/> on.</param>";
                            var valueFormat = "<param name=\"{value}\">{registered_name} property value.</param>";
                            if (methodDeclaration.TryGetDocumentationComment(out var comment))
                            {
                                if (comment.VerifySummary(summaryFormat, backing.Symbol.Name, element.Name) is { } summaryError)
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.WPF0061DocumentClrMethod,
                                            summaryError.Location,
                                            ImmutableDictionary<string, string?>.Empty.Add(nameof(DocComment), summaryError.Text)));
                                }

                                if (comment.VerifyParameter(elementFormat, element, element.ToCrefType(), backing.Symbol.Name) is { } elementError)
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.WPF0061DocumentClrMethod,
                                            elementError.Location,
                                            ImmutableDictionary<string, string?>.Empty.Add(nameof(DocComment), elementError.Text)));
                                }

                                if (comment.VerifyParameter(valueFormat, value, registeredName) is { } valueError)
                                {
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            Descriptors.WPF0061DocumentClrMethod,
                                            valueError.Location,
                                            ImmutableDictionary<string, string?>.Empty.Add(nameof(DocComment), valueError.Text)));
                                }
                            }
                            else
                            {
                                context.ReportDiagnostic(
                                    Diagnostic.Create(
                                        Descriptors.WPF0061DocumentClrMethod,
                                        methodDeclaration.Identifier.GetLocation(),
                                        ImmutableDictionary<string, string?>.Empty.Add(
                                            nameof(DocComment),
#pragma warning disable SA1118 // Parameter should not span multiple lines
                                            $"/// {DocComment.Format(summaryFormat, backing.Symbol.Name, element.Name)}\n" +
                                            $"/// {DocComment.Format(elementFormat, element.Name, element.ToCrefType(), backing.Name)}\n" +
                                            $"/// {DocComment.Format(valueFormat, value.Name, registeredName)}\n")));
#pragma warning restore SA1118 // Parameter should not span multiple lines
                            }
                        }
                    }

                    if (DependencyProperty.TryGetRegisteredType(backing, context.SemanticModel, context.CancellationToken, out var registeredType) &&
                        method.Parameters.TryElementAt(1, out var valueParameter) &&
                        !SymbolEqualityComparer.Equal(valueParameter.Type, registeredType))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0013ClrMethodMustMatchRegisteredType,
                                methodDeclaration.ParameterList.Parameters[1].Type.GetLocation(),
                                ImmutableDictionary<string, string?>.Empty.Add(nameof(TypeSyntax), registeredType.ToMinimalDisplayString(context.SemanticModel, context.Node.SpanStart)),
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
    }
}
