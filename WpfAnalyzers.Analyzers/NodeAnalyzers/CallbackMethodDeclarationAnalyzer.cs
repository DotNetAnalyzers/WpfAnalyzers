namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class CallbackMethodDeclarationAnalyzer : DiagnosticAnalyzer
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
                TrySingleUsage(method, methodDeclaration, out var callbackArg))
            {
                // PropertyChangedCallback
                if (method.Parameters.Length == 2 &&
                    method.ReturnsVoid &&
                    method.Parameters.TryElementAt(0, out var senderParameter) &&
                    senderParameter.Type.Is(KnownSymbol.DependencyObject) &&
                    method.Parameters.TryElementAt(1, out var argParameter) &&
                    argParameter.Type == KnownSymbol.DependencyPropertyChangedEventArgs &&
                    TryGetExpectedTypes(callbackArg, method.ContainingType, context, out var senderType, out var valueType))
                {
                    HandleCasts(context, methodDeclaration, senderParameter, senderType, WPF0019CastSenderToCorrectType.Descriptor, WPF0021DirectCastSenderToExactType.Descriptor);
                    HandleCasts(context, methodDeclaration, argParameter, valueType, WPF0020CastValueToCorrectType.Descriptor, WPF0022DirectCastValueToExactType.Descriptor);
                }

                // CoerceValueCallback
                if (method.Parameters.Length == 2 &&
                    method.ReturnType == KnownSymbol.Object &&
                    method.Parameters.TryElementAt(0, out senderParameter) &&
                    senderParameter.Type.Is(KnownSymbol.DependencyObject) &&
                    method.Parameters.TryElementAt(1, out argParameter) &&
                    argParameter.Type == KnownSymbol.Object &&
                    TryGetExpectedTypes(callbackArg, method.ContainingType, context, out senderType, out valueType))
                {
                    HandleCasts(context, methodDeclaration, senderParameter, senderType, WPF0019CastSenderToCorrectType.Descriptor, WPF0021DirectCastSenderToExactType.Descriptor);
                    HandleCasts(context, methodDeclaration, argParameter, valueType, WPF0020CastValueToCorrectType.Descriptor, WPF0022DirectCastValueToExactType.Descriptor);
                }

                // ValidateValueCallback
                if (method.Parameters.Length == 1 &&
                    method.ReturnType == KnownSymbol.Boolean &&
                    method.Parameters.TryElementAt(0, out argParameter) &&
                    argParameter.Type == KnownSymbol.Object)
                {
                    if (callbackArg.Parent?.Parent is ObjectCreationExpressionSyntax callbackCreation &&
                        callbackCreation.Type is SimpleNameSyntax simpleName &&
                        simpleName.Identifier.ValueText == "ValidateValueCallback")
                    {
                        callbackArg = callbackCreation.Parent as ArgumentSyntax;
                    }

                    if (callbackArg?.Parent?.Parent is InvocationExpressionSyntax invocation &&
                        (DependencyProperty.TryGetRegisterCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                         DependencyProperty.TryGetRegisterReadOnlyCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                         DependencyProperty.TryGetRegisterAttachedCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                         DependencyProperty.TryGetRegisterAttachedReadOnlyCall(invocation, context.SemanticModel, context.CancellationToken, out _)) &&
                        TryGetRegisteredType(invocation, 1, method.ContainingType, context, out valueType))
                    {
                        HandleCasts(context, methodDeclaration, argParameter, valueType, WPF0020CastValueToCorrectType.Descriptor, WPF0022DirectCastValueToExactType.Descriptor);
                    }
                }
            }
        }

        private static void HandleCasts(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodDeclaration, IParameterSymbol parameter, ITypeSymbol expectedType, DiagnosticDescriptor wrongTypeDescriptor, DiagnosticDescriptor notExactTypeDescriptor)
        {
            if (expectedType == null)
            {
                return;
            }

            using (var walker = SpecificIdentifierNameWalker.Borrow(methodDeclaration, parameter.Name))
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
                        if (!expectedType.Is(castType))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    wrongTypeDescriptor,
                                    castExpression.Type.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add("ExpectedType", expectedTypeName),
                                    expectedTypeName));
                        }
                        else
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    notExactTypeDescriptor,
                                    castExpression.Type.GetLocation(),
                                    ImmutableDictionary<string, string>.Empty.Add("ExpectedType", expectedTypeName),
                                    expectedTypeName));
                        }
                    }

                    if (parent is BinaryExpressionSyntax binaryExpression &&
                        binaryExpression.IsKind(SyntaxKind.AsExpression) &&
                        context.SemanticModel.GetTypeInfoSafe(binaryExpression.Right, context.CancellationToken)
                               .Type is ITypeSymbol asType &&
                        asType.TypeKind != TypeKind.Interface &&
                        expectedType.TypeKind != TypeKind.Interface &&
                        !(asType.Is(expectedType) || expectedType.Is(asType)))
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
                        context.SemanticModel.GetTypeInfoSafe(isDeclaration.Type, context.CancellationToken)
                               .Type is ITypeSymbol isType &&
                        isType.TypeKind != TypeKind.Interface &&
                        expectedType.TypeKind != TypeKind.Interface &&
                        !(isType.Is(expectedType) || expectedType.Is(isType)))
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
                                    context.SemanticModel.GetTypeInfoSafe(labelDeclaration.Type, context.CancellationToken).Type is ITypeSymbol caseType &&
                                    caseType.TypeKind != TypeKind.Interface &&
                                    !(caseType.Is(expectedType) || expectedType.Is(caseType)))
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

        private static bool TrySingleUsage(IMethodSymbol method, MethodDeclarationSyntax methodDeclaration, out ArgumentSyntax argument)
        {
            argument = null;
            if (method.DeclaredAccessibility != Accessibility.Private)
            {
                return false;
            }

            using (var walker = SpecificIdentifierNameWalker.Borrow(methodDeclaration.Parent as ClassDeclarationSyntax, method.MetadataName))
            {
                foreach (var identifierName in walker.IdentifierNames)
                {
                    if (identifierName.Parent is ArgumentSyntax candidate)
                    {
                        if (argument != null)
                        {
                            return false;
                        }

                        argument = candidate;
                        continue;
                    }

                    return false;
                }
            }

            return argument != null;
        }

        private static bool TryGetRegisteredType(InvocationExpressionSyntax registration, int index, INamedTypeSymbol containingType, SyntaxNodeAnalysisContext context, out ITypeSymbol type)
        {
            type = null;
            return registration.TryGetArgumentAtIndex(index, out var senderTypeArg) &&
                   senderTypeArg.Expression is TypeOfExpressionSyntax senderTypeOf &&
                   TypeOf.TryGetType(senderTypeOf, containingType, context.SemanticModel, context.CancellationToken, out type);
        }

        private static bool TryGetExpectedTypes(ArgumentSyntax argument, INamedTypeSymbol containingType, SyntaxNodeAnalysisContext context, out ITypeSymbol senderType, out ITypeSymbol valueType)
        {
            senderType = null;
            valueType = null;
            if (argument == null)
            {
                return false;
            }

            if (argument.Parent?.Parent is ObjectCreationExpressionSyntax callbackCreation &&
                callbackCreation.Type is SimpleNameSyntax simpleName &&
                (simpleName.Identifier.ValueText == "PropertyChangedCallback" ||
                 simpleName.Identifier.ValueText == "CoerceValueCallback"))
            {
                if (callbackCreation.Parent is ArgumentSyntax parent)
                {
                    argument = parent;
                }
                else
                {
                    return false;
                }
            }

            if (argument.Parent is ArgumentListSyntax argumentList &&
                argumentList.Parent is ObjectCreationExpressionSyntax metaDataCreation &&
                PropertyMetadata.TryGetConstructor(metaDataCreation, context.SemanticModel, context.CancellationToken, out _) &&
                metaDataCreation.Parent is ArgumentSyntax metaDataArgument &&
                metaDataArgument.Parent?.Parent is InvocationExpressionSyntax registerInvocation)
            {
                if (DependencyProperty.TryGetRegisterCall(registerInvocation, context.SemanticModel, context.CancellationToken, out _) ||
                    DependencyProperty.TryGetRegisterReadOnlyCall(registerInvocation, context.SemanticModel, context.CancellationToken, out _))
                {
                    senderType = containingType;
                    return TryGetRegisteredType(registerInvocation, 1, containingType, context, out valueType);
                }

                if (DependencyProperty.TryGetRegisterAttachedCall(registerInvocation, context.SemanticModel, context.CancellationToken, out _) ||
                    DependencyProperty.TryGetRegisterAttachedReadOnlyCall(registerInvocation, context.SemanticModel, context.CancellationToken, out _))
                {
                    return TryGetRegisteredType(registerInvocation, 1, containingType, context, out valueType);
                }
            }

            return false;
        }
    }
}
