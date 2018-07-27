namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class CallbackAnalyzer : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            WPF0019CastSenderToCorrectType.Descriptor,
            WPF0020CastValueToCorrectType.Descriptor,
            WPF0021DirectCastSenderToExactType.Descriptor,
            WPF0022DirectCastValueToExactType.Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(HandleMethod, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(HandleLambda, SyntaxKind.ParenthesizedLambdaExpression);
        }

        private static void HandleMethod(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is MethodDeclarationSyntax methodDeclaration &&
                context.ContainingSymbol is IMethodSymbol method &&
                method.IsStatic)
            {
                if (TryMatchPropertyChangedCallback(method, context, out var senderParameter, out var argParameter) ||
                    TryMatchCoerceValueCallback(method, context, out senderParameter, out argParameter))
                {
                    using (var usages = GetCallbackArguments(context, method, methodDeclaration))
                    {
                        foreach (var callbackArgument in usages)
                        {
                            if (TryGetSenderType(callbackArgument, method.ContainingType, context, out var senderType))
                            {
                                HandleCasts(context, methodDeclaration, senderParameter, senderType, WPF0019CastSenderToCorrectType.Descriptor, WPF0021DirectCastSenderToExactType.Descriptor);
                            }

                            if (TryGetValueType(callbackArgument, method.ContainingType, context, out var valueType))
                            {
                                HandleCasts(context, methodDeclaration, argParameter, valueType, WPF0020CastValueToCorrectType.Descriptor, WPF0022DirectCastValueToExactType.Descriptor);
                            }
                        }
                    }
                }
                else if (TryMatchValidateValueCallback(method, out argParameter))
                {
                    using (var usages = GetCallbackArguments(context, method, methodDeclaration))
                    {
                        foreach (var callbackArgument in usages)
                        {
                            if (TryGetValueType(callbackArgument, method.ContainingType, context, out var valueType))
                            {
                                HandleCasts(context, methodDeclaration, argParameter, valueType, WPF0020CastValueToCorrectType.Descriptor, WPF0022DirectCastValueToExactType.Descriptor);
                            }
                        }
                    }
                }
            }
        }

        private static void HandleLambda(SyntaxNodeAnalysisContext context)
        {
            if (context.IsExcludedFromAnalysis())
            {
                return;
            }

            if (context.Node is ParenthesizedLambdaExpressionSyntax lambda &&
                lambda.Parent is ArgumentSyntax argument &&
                TryGetCallbackArgument(argument, out var callbackArgument) &&
                context.SemanticModel.TryGetSymbol(lambda, context.CancellationToken, out IMethodSymbol method))
            {
                if (TryMatchPropertyChangedCallback(method, context, out var senderParameter, out var argParameter) ||
                    TryMatchCoerceValueCallback(method, context, out senderParameter, out argParameter))
                {
                    if (TryGetSenderType(callbackArgument, method.ContainingType, context, out var senderType))
                    {
                        HandleCasts(context, lambda, senderParameter, senderType, WPF0019CastSenderToCorrectType.Descriptor, WPF0021DirectCastSenderToExactType.Descriptor);
                    }

                    if (TryGetValueType(callbackArgument, method.ContainingType, context, out var valueType))
                    {
                        HandleCasts(context, lambda, argParameter, valueType, WPF0020CastValueToCorrectType.Descriptor, WPF0022DirectCastValueToExactType.Descriptor);
                    }
                }
                else if (TryMatchValidateValueCallback(method, out argParameter) &&
                         TryGetValueType(callbackArgument, method.ContainingType, context, out var valueType))
                {
                    HandleCasts(context, lambda, argParameter, valueType, WPF0020CastValueToCorrectType.Descriptor, WPF0022DirectCastValueToExactType.Descriptor);
                }
            }
        }

        private static bool TryMatchPropertyChangedCallback(IMethodSymbol methodSymbol, SyntaxNodeAnalysisContext context, out IParameterSymbol senderParameter, out IParameterSymbol argParameter)
        {
            senderParameter = null;
            argParameter = null;
            return methodSymbol.Parameters.Length == 2 &&
                   methodSymbol.ReturnsVoid &&
                   methodSymbol.Parameters.TryElementAt(0, out senderParameter) &&
                   senderParameter.Type.IsAssignableTo(KnownSymbol.DependencyObject, context.Compilation) &&
                   methodSymbol.Parameters.TryElementAt(1, out argParameter) &&
                   argParameter.Type == KnownSymbol.DependencyPropertyChangedEventArgs;
        }

        private static bool TryMatchCoerceValueCallback(IMethodSymbol candidate, SyntaxNodeAnalysisContext context, out IParameterSymbol senderParameter, out IParameterSymbol argParameter)
        {
            senderParameter = null;
            argParameter = null;
            return candidate.Parameters.Length == 2 &&
                   candidate.ReturnType == KnownSymbol.Object &&
                   candidate.Parameters.TryElementAt(0, out senderParameter) &&
                   senderParameter.Type.IsAssignableTo(KnownSymbol.DependencyObject, context.Compilation) &&
                   candidate.Parameters.TryElementAt(1, out argParameter) &&
                   argParameter.Type == KnownSymbol.Object;
        }

        private static bool TryMatchValidateValueCallback(IMethodSymbol candidate, out IParameterSymbol argParameter)
        {
            argParameter = null;
            return candidate.Parameters.Length == 1 &&
                   candidate.ReturnType == KnownSymbol.Boolean &&
                   candidate.Parameters.TryElementAt(0, out argParameter) &&
                   argParameter.Type == KnownSymbol.Object;
        }

        private static void HandleCasts(SyntaxNodeAnalysisContext context, SyntaxNode methodOrLambda, IParameterSymbol parameter, ITypeSymbol expectedType, DiagnosticDescriptor wrongTypeDescriptor, DiagnosticDescriptor notExactTypeDescriptor)
        {
            if (expectedType == null)
            {
                return;
            }

            using (var walker = SpecificIdentifierNameWalker.Borrow(methodOrLambda, parameter.Name))
            {
                foreach (var identifierName in walker.IdentifierNames)
                {
                    var parent = identifierName.Parent;
                    if (parameter.Type == KnownSymbol.DependencyPropertyChangedEventArgs &&
                        parent is MemberAccessExpressionSyntax memberAccess &&
                        (memberAccess.Name.Identifier.ValueText == "NewValue" ||
                         memberAccess.Name.Identifier.ValueText == "OldValue"))
                    {
                        parent = memberAccess.Parent;
                    }

                    if (parent is CastExpressionSyntax castExpression &&
                        context.SemanticModel.GetTypeInfoSafe(castExpression.Type, context.CancellationToken).Type is ITypeSymbol castType &&
                        !Equals(castType, expectedType))
                    {
                        var expectedTypeName = expectedType.ToMinimalDisplayString(context.SemanticModel, castExpression.SpanStart, SymbolDisplayFormat.MinimallyQualifiedFormat);
                        if (!expectedType.IsAssignableTo(castType, context.Compilation))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    wrongTypeDescriptor,
                                    castExpression.Type.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add("ExpectedType", expectedTypeName),
                                    expectedTypeName));
                        }

                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                notExactTypeDescriptor,
                                castExpression.Type.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add("ExpectedType", expectedTypeName),
                                expectedTypeName));
                    }

                    if (parent is BinaryExpressionSyntax binaryExpression &&
                        binaryExpression.IsKind(SyntaxKind.AsExpression) &&
                        context.SemanticModel.TryGetType(binaryExpression.Right, context.CancellationToken, out var asType) &&
                        asType.TypeKind != TypeKind.Interface &&
                        expectedType.TypeKind != TypeKind.Interface &&
                        !(asType.IsAssignableTo(expectedType, context.Compilation) ||
                          expectedType.IsAssignableTo(asType, context.Compilation)))
                    {
                        var expectedTypeName = expectedType.ToMinimalDisplayString(
                            context.SemanticModel,
                            binaryExpression.SpanStart,
                            SymbolDisplayFormat.MinimallyQualifiedFormat);
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                wrongTypeDescriptor,
                                binaryExpression.Right.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add("ExpectedType", expectedTypeName),
                                expectedTypeName));
                    }

                    if (parent is IsPatternExpressionSyntax isPattern &&
                        expectedType != KnownSymbol.Object &&
                        isPattern.Pattern is DeclarationPatternSyntax isDeclaration &&
                        context.SemanticModel.TryGetType(isDeclaration.Type, context.CancellationToken, out var isType) &&
                        isType.TypeKind != TypeKind.Interface &&
                        expectedType.TypeKind != TypeKind.Interface &&
                        !(isType.IsAssignableTo(expectedType, context.Compilation) || expectedType.IsAssignableTo(isType, context.Compilation)))
                    {
                        var expectedTypeName = expectedType.ToMinimalDisplayString(
                            context.SemanticModel,
                            isPattern.SpanStart,
                            SymbolDisplayFormat.MinimallyQualifiedFormat);
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                wrongTypeDescriptor,
                                isDeclaration.Type.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add("ExpectedType", expectedTypeName),
                                expectedTypeName));
                    }

                    if (parent is SwitchStatementSyntax switchStatement &&
                        expectedType.TypeKind != TypeKind.Interface &&
                        expectedType != KnownSymbol.Object)
                    {
                        foreach (var section in switchStatement.Sections)
                        {
                            foreach (var label in section.Labels)
                            {
                                if (label is CasePatternSwitchLabelSyntax patternLabel &&
                                    patternLabel.Pattern is DeclarationPatternSyntax labelDeclaration &&
                                    context.SemanticModel.TryGetType(labelDeclaration.Type, context.CancellationToken, out var caseType) &&
                                    caseType.TypeKind != TypeKind.Interface &&
                                    !(caseType.IsAssignableTo(expectedType, context.Compilation) ||
                                      expectedType.IsAssignableTo(caseType, context.Compilation)))
                                {
                                    var expectedTypeName = expectedType.ToMinimalDisplayString(
                                        context.SemanticModel,
                                        label.SpanStart,
                                        SymbolDisplayFormat.MinimallyQualifiedFormat);
                                    context.ReportDiagnostic(
                                        Diagnostic.Create(
                                            wrongTypeDescriptor,
                                            labelDeclaration.Type.GetLocation(),
                                            ImmutableDictionary<string, string>.Empty.Add(
                                                "ExpectedType",
                                                expectedTypeName),
                                            expectedTypeName));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static PooledSet<ArgumentSyntax> GetCallbackArguments(SyntaxNodeAnalysisContext context, IMethodSymbol method, MethodDeclarationSyntax methodDeclaration)
        {
            // Set is not perfect here but using it as there is no pooled list
            var callbacks = PooledSet<ArgumentSyntax>.Borrow();
            using (var walker = SpecificIdentifierNameWalker.Borrow(methodDeclaration.Parent as ClassDeclarationSyntax, method.MetadataName))
            {
                foreach (var identifierName in walker.IdentifierNames)
                {
                    if (context.SemanticModel.TryGetSymbol(identifierName, context.CancellationToken, out IMethodSymbol symbol) &&
                        Equals(symbol, method))
                    {
                        switch (identifierName.Parent)
                        {
                            case ArgumentSyntax argument when TryGetCallbackArgument(argument, out argument):
                                callbacks.Add(argument);
                                break;
                            case InvocationExpressionSyntax invocation when
                                invocation.Parent is ParenthesizedLambdaExpressionSyntax lambda &&
                                lambda.Parent is ArgumentSyntax argument &&
                                TryGetCallbackArgument(argument, out argument):
                                callbacks.Add(argument);
                                break;
                        }
                    }
                }
            }

            return callbacks;
        }

        private static bool TryGetCallbackArgument(ArgumentSyntax candidate, out ArgumentSyntax result)
        {
            if (candidate.Parent is ArgumentListSyntax argumentList &&
                argumentList.Parent is ObjectCreationExpressionSyntax callbackCreation &&
                callbackCreation.Parent is ArgumentSyntax parent &&
                (callbackCreation.Type == KnownSymbol.PropertyChangedCallback ||
                 callbackCreation.Type == KnownSymbol.CoerceValueCallback ||
                 callbackCreation.Type == KnownSymbol.ValidateValueCallback))
            {
                result = parent;
            }
            else
            {
                result = candidate;
            }

            return true;
        }

        private static bool TryGetSenderType(ArgumentSyntax argument, INamedTypeSymbol containingType, SyntaxNodeAnalysisContext context, out ITypeSymbol senderType)
        {
            senderType = null;
            if (argument == null)
            {
                return false;
            }

            if (argument.Parent is ArgumentListSyntax argumentList &&
                argumentList.Parent is ObjectCreationExpressionSyntax metaDataCreation &&
                PropertyMetadata.TryGetConstructor(metaDataCreation, context.SemanticModel, context.CancellationToken, out _) &&
                metaDataCreation.Parent is ArgumentSyntax metaDataArgument &&
                metaDataArgument.Parent?.Parent is InvocationExpressionSyntax registerInvocation)
            {
                if (DependencyProperty.TryGetRegisterCall(registerInvocation, context.SemanticModel, context.CancellationToken, out _) ||
                    DependencyProperty.TryGetRegisterReadOnlyCall(registerInvocation, context.SemanticModel, context.CancellationToken, out _) ||
                    DependencyProperty.TryGetAddOwnerCall(registerInvocation, context.SemanticModel, context.CancellationToken, out _) ||
                    DependencyProperty.TryGetOverrideMetadataCall(registerInvocation, context.SemanticModel, context.CancellationToken, out _))
                {
                    senderType = containingType;
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetValueType(ArgumentSyntax argument, INamedTypeSymbol containingType, SyntaxNodeAnalysisContext context, out ITypeSymbol type)
        {
            type = null;

            if (argument?.Parent is ArgumentListSyntax argumentList)
            {
                switch (argumentList.Parent)
                {
                    case ObjectCreationExpressionSyntax objectCreation:
                        return TryGetValueType(objectCreation.Parent as ArgumentSyntax, containingType, context, out type);
                    case InvocationExpressionSyntax invocation when
                        DependencyProperty.TryGetRegisterCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                        DependencyProperty.TryGetRegisterReadOnlyCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                        DependencyProperty.TryGetRegisterAttachedCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                        DependencyProperty.TryGetRegisterAttachedReadOnlyCall(invocation, context.SemanticModel, context.CancellationToken, out _):
                        {
                            return invocation.TryGetArgumentAtIndex(1, out var arg) &&
                                   arg.Expression is TypeOfExpressionSyntax senderTypeOf &&
                                   TypeOf.TryGetType(senderTypeOf, containingType, context.SemanticModel, context.CancellationToken, out type);
                        }

                    case InvocationExpressionSyntax invocation when
                        invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                        (DependencyProperty.TryGetAddOwnerCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                         DependencyProperty.TryGetOverrideMetadataCall(invocation, context.SemanticModel, context.CancellationToken, out _)):
                        {
                            return BackingFieldOrProperty.TryCreate(context.SemanticModel.GetSymbolSafe(memberAccess.Expression, context.CancellationToken), out var fieldOrProperty) &&
                                   DependencyProperty.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out type);
                        }
                }
            }

            return false;
        }
    }
}
