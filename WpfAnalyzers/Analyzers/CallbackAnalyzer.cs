namespace WpfAnalyzers
{
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
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
            Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName,
            Descriptors.WPF0019CastSenderToCorrectType,
            Descriptors.WPF0020CastValueToCorrectType,
            Descriptors.WPF0021DirectCastSenderToExactType,
            Descriptors.WPF0022DirectCastValueToExactType,
            Descriptors.WPF0062DocumentPropertyChangedCallback);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(x => HandleMethod(x), SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(x => HandleLambda(x), SyntaxKind.ParenthesizedLambdaExpression);
        }

        private static void HandleMethod(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is MethodDeclarationSyntax methodDeclaration &&
                context.ContainingSymbol is IMethodSymbol method)
            {
                if (method.IsStatic)
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
                                    HandleCasts(context, methodDeclaration, senderParameter, senderType, Descriptors.WPF0019CastSenderToCorrectType, Descriptors.WPF0021DirectCastSenderToExactType);
                                }

                                if (TryGetValueType(callbackArgument, method.ContainingType, context, out var valueType))
                                {
                                    HandleCasts(context, methodDeclaration, argParameter, valueType, Descriptors.WPF0020CastValueToCorrectType, Descriptors.WPF0022DirectCastValueToExactType);
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
                                    HandleCasts(context, methodDeclaration, argParameter, valueType, Descriptors.WPF0020CastValueToCorrectType, Descriptors.WPF0022DirectCastValueToExactType);
                                }
                            }
                        }
                    }
                }
                else if (method.ReturnsVoid &&
                         method.IsVirtual &&
                         TryGetSingleInvocation(method, methodDeclaration, context, out var singleInvocation) &&
                         TryGetDpFromInstancePropertyChanged(singleInvocation, context, out var fieldOrProperty))
                {
                    if (DependencyProperty.TryGetRegisteredName(fieldOrProperty, context.SemanticModel, context.CancellationToken, out _, out var registeredName) &&
                        !method.Name.IsParts("On", registeredName, "Changed"))
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName,
                                methodDeclaration.Identifier.GetLocation(),
                                ImmutableDictionary<string, string>.Empty.Add("ExpectedName", $"On{registeredName}Changed"),
                                methodDeclaration.Identifier,
                                $"On{registeredName}Changed"));
                    }

                    if (method.DeclaredAccessibility.IsEither(Accessibility.Protected, Accessibility.Internal, Accessibility.Public) &&
                        HasStandardText(methodDeclaration, singleInvocation, fieldOrProperty, out var location, out var standardExpectedText) == false)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                                                     Descriptors.WPF0062DocumentPropertyChangedCallback,
                                                     location,
                                                     ImmutableDictionary<string, string>.Empty.Add(nameof(Descriptors.WPF0062DocumentPropertyChangedCallback), standardExpectedText)));
                    }
                }
            }
        }

        private static void HandleLambda(SyntaxNodeAnalysisContext context)
        {
            if (!context.IsExcludedFromAnalysis() &&
                context.Node is ParenthesizedLambdaExpressionSyntax { Parent: ArgumentSyntax argument } lambda &&
                TryGetCallbackArgument(argument, out var callbackArgument) &&
                context.SemanticModel.TryGetSymbol(lambda, context.CancellationToken, out IMethodSymbol? method))
            {
                if (TryMatchPropertyChangedCallback(method, context, out var senderParameter, out var argParameter) ||
                    TryMatchCoerceValueCallback(method, context, out senderParameter, out argParameter))
                {
                    if (TryGetSenderType(callbackArgument, method.ContainingType, context, out var senderType))
                    {
                        HandleCasts(context, lambda, senderParameter, senderType, Descriptors.WPF0019CastSenderToCorrectType, Descriptors.WPF0021DirectCastSenderToExactType);
                    }

                    if (TryGetValueType(callbackArgument, method.ContainingType, context, out var valueType))
                    {
                        HandleCasts(context, lambda, argParameter, valueType, Descriptors.WPF0020CastValueToCorrectType, Descriptors.WPF0022DirectCastValueToExactType);
                    }
                }
                else if (TryMatchValidateValueCallback(method, out argParameter) &&
                         TryGetValueType(callbackArgument, method.ContainingType, context, out var valueType))
                {
                    HandleCasts(context, lambda, argParameter, valueType, Descriptors.WPF0020CastValueToCorrectType, Descriptors.WPF0022DirectCastValueToExactType);
                }
            }
        }

        private static bool TryMatchPropertyChangedCallback(IMethodSymbol candidate, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out IParameterSymbol? senderParameter, [NotNullWhen(true)] out IParameterSymbol? argParameter)
        {
            senderParameter = null;
            argParameter = null;
            return candidate is { ReturnsVoid: true, Parameters: { Length: 2 } parameters } &&
                   parameters.TryElementAt(0, out senderParameter) &&
                   senderParameter.Type.IsAssignableTo(KnownSymbols.DependencyObject, context.Compilation) &&
                   parameters.TryElementAt(1, out argParameter) &&
                   argParameter.Type == KnownSymbols.DependencyPropertyChangedEventArgs;
        }

        private static bool TryMatchCoerceValueCallback(IMethodSymbol candidate, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out IParameterSymbol? senderParameter, [NotNullWhen(true)] out IParameterSymbol? argParameter)
        {
            senderParameter = null;
            argParameter = null;
            return candidate is { ReturnsVoid: false, Parameters: { Length: 2 } parameters } &&
                   candidate.ReturnType == KnownSymbols.Object &&
                   parameters.TryElementAt(0, out senderParameter) &&
                   senderParameter.Type.IsAssignableTo(KnownSymbols.DependencyObject, context.Compilation) &&
                   parameters.TryElementAt(1, out argParameter) &&
                   argParameter.Type == KnownSymbols.Object;
        }

        private static bool TryMatchValidateValueCallback(IMethodSymbol candidate, [NotNullWhen(true)] out IParameterSymbol? argParameter)
        {
            argParameter = null;
            return candidate is { ReturnsVoid: false, Parameters: { Length: 1 } parameters } &&
                   candidate.ReturnType == KnownSymbols.Boolean &&
                   parameters.TryElementAt(0, out argParameter) &&
                   argParameter.Type == KnownSymbols.Object;
        }

        private static bool TryGetDpFromInstancePropertyChanged(InvocationExpressionSyntax singleInvocation, SyntaxNodeAnalysisContext context, out BackingFieldOrProperty fieldOrProperty)
        {
            if (singleInvocation.Parent is ParenthesizedLambdaExpressionSyntax { Parent: ArgumentSyntax lambdaArg } lambda &&
                TryGetCallbackArgument(lambdaArg, out var callbackArgument) &&
                context.SemanticModel.TryGetSymbol(lambda, context.CancellationToken, out IMethodSymbol? lambdaMethod) &&
                TryMatchPropertyChangedCallback(lambdaMethod, context, out var senderParameter, out var argParameter) &&
                Try(out fieldOrProperty))
            {
                return true;
            }

            if (singleInvocation.TryFirstAncestor(out MethodDeclarationSyntax? staticCallback) &&
               context.SemanticModel.TryGetSymbol(staticCallback, context.CancellationToken, out var staticMethod) &&
               staticMethod.IsStatic &&
               TryGetStaticCallbackArgument(staticMethod, staticCallback, out callbackArgument) &&
               TryMatchPropertyChangedCallback(staticMethod, context, out senderParameter, out argParameter) &&
                Try(out fieldOrProperty))
            {
                return true;
            }

            return false;

            bool IsCalledOnSender()
            {
                if (singleInvocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                    MemberPath.TrySingle(memberAccess.Expression, out var pathItem))
                {
                    if (pathItem.ValueText == senderParameter.Name)
                    {
                        return true;
                    }

                    if (context.SemanticModel.TryGetSymbol(pathItem, context.CancellationToken, out ILocalSymbol? local) &&
                        local.TrySingleDeclaration(context.CancellationToken, out var declaration))
                    {
                        if (declaration is SingleVariableDesignationSyntax { Parent: DeclarationPatternSyntax { Parent: IsPatternExpressionSyntax isPattern } })
                        {
                            return isPattern.Expression is IdentifierNameSyntax { Identifier: { } identifier } &&
                                   identifier.ValueText == senderParameter.Name;
                        }

                        using (var walker = SpecificIdentifierNameWalker.Borrow(declaration, senderParameter.Name))
                        {
                            return walker.IdentifierNames.TrySingle(out var identifierName) &&
                                   context.SemanticModel.TryGetSymbol(identifierName, context.CancellationToken, out IParameterSymbol? symbol) &&
                                   symbol.Name == senderParameter.Name;
                        }
                    }
                }

                return false;
            }

            bool ArgsUsesParameter()
            {
                if (singleInvocation.ArgumentList is { } argumentList)
                {
                    foreach (var argument in argumentList.Arguments)
                    {
                        using (var walker = IdentifierNameWalker.Borrow(argument.Expression))
                        {
                            if (!walker.TryFind(argParameter.Name, out _))
                            {
                                return false;
                            }

                            if (!walker.TryFind("NewValue", out _) &&
                                !walker.TryFind("OldValue", out _))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }

                return false;
            }

            bool TryGetStaticCallbackArgument(IMethodSymbol method, MethodDeclarationSyntax methodDeclaration, out ArgumentSyntax? result)
            {
                using (var usages = GetCallbackArguments(context, method, methodDeclaration))
                {
                    return usages.TrySingle(out result);
                }
            }

            bool Try(out BackingFieldOrProperty backing)
            {
                return IsCalledOnSender() &&
                       ArgsUsesParameter() &&
                       callbackArgument.Parent is ArgumentListSyntax { Parent: ObjectCreationExpressionSyntax metaDataCreation } &&
                       PropertyMetadata.TryGetDependencyProperty(metaDataCreation, context.SemanticModel, context.CancellationToken, out backing);
            }
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
                    if (parameter.Type == KnownSymbols.DependencyPropertyChangedEventArgs &&
                        parent is MemberAccessExpressionSyntax { Name: IdentifierNameSyntax { Identifier: { } identifier } } memberAccess &&
                        (identifier.ValueText == "NewValue" ||
                         identifier.ValueText == "OldValue"))
                    {
                        parent = memberAccess.Parent;
                    }

                    if (parent is CastExpressionSyntax castExpression &&
                        context.SemanticModel.GetTypeInfoSafe(castExpression.Type, context.CancellationToken).Type is { } castType &&
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

                    if (parent is IsPatternExpressionSyntax { Pattern: DeclarationPatternSyntax isDeclaration } &&
                        expectedType != KnownSymbols.Object &&
                        context.SemanticModel.TryGetType(isDeclaration.Type, context.CancellationToken, out var isType) &&
                        isType.TypeKind != TypeKind.Interface &&
                        expectedType.TypeKind != TypeKind.Interface &&
                        !(isType.IsAssignableTo(expectedType, context.Compilation) || expectedType.IsAssignableTo(isType, context.Compilation)))
                    {
                        var expectedTypeName = expectedType.ToMinimalDisplayString(
                            context.SemanticModel,
                            isDeclaration.SpanStart,
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
                        expectedType != KnownSymbols.Object)
                    {
                        foreach (var section in switchStatement.Sections)
                        {
                            foreach (var label in section.Labels)
                            {
                                if (label is CasePatternSwitchLabelSyntax { Pattern: DeclarationPatternSyntax { Type: { } type } } &&
                                    context.SemanticModel.TryGetType(type, context.CancellationToken, out var caseType) &&
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
                                            type.GetLocation(),
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

        private static bool? HasStandardText(MethodDeclarationSyntax methodDeclaration, InvocationExpressionSyntax invocation, BackingFieldOrProperty backingField, [NotNullWhen(true)] out Location? location, out string? expectedText)
        {
            expectedText = null;
            location = null;
            var standardSummaryText = $"<summary>This method is invoked when the <see cref=\"{backingField.Name}\"/> changes.</summary>";
            if (methodDeclaration.ParameterList is { } parameterList)
            {
                if (HasDocComment(out var comment, out location) &&
                    HasSummary(comment, standardSummaryText, out location))
                {
                    if (parameterList.Parameters.Count == 0)
                    {
                        return true;
                    }

                    if (parameterList.Parameters.Count == 1)
                    {
                        if (TryGetNewValue(out var parameter, out var standardParamText) ||
                            TryGetOldValue(out parameter, out standardParamText))
                        {
                            if (HasParam(comment, parameter, standardParamText, out location))
                            {
                                return true;
                            }

                            expectedText = StringBuilderPool.Borrow()
                                                            .Append("/// ").AppendLine(standardSummaryText)
                                                            .Append("/// ").AppendLine(standardParamText)
                                                            .Return();
                            return false;
                        }

                        return null;
                    }

                    if (parameterList.Parameters.Count == 2)
                    {
                        if (TryGetOldValue(out var oldParameter, out var standardOldParamText) &&
                            TryGetNewValue(out var newParameter, out var standardNewParamText))
                        {
                            if (HasParam(comment, oldParameter, standardOldParamText, out location) &&
                                HasParam(comment, newParameter, standardNewParamText, out location))
                            {
                                return true;
                            }

                            if (parameterList.Parameters.IndexOf(oldParameter) < parameterList.Parameters.IndexOf(newParameter))
                            {
                                expectedText = StringBuilderPool.Borrow()
                                                                .Append("/// ").AppendLine(standardSummaryText)
                                                                .Append("/// ").AppendLine(standardOldParamText)
                                                                .Append("/// ").AppendLine(standardNewParamText)
                                                                .Return();
                            }
                            else
                            {
                                expectedText = StringBuilderPool.Borrow()
                                                                .Append("/// ").AppendLine(standardSummaryText)
                                                                .Append("/// ").AppendLine(standardNewParamText)
                                                                .Append("/// ").AppendLine(standardOldParamText)
                                                                .Return();
                            }

                            return false;
                        }

                        return null;
                    }

                    return false;
                }

                if (parameterList.Parameters.Count == 0)
                {
                    expectedText = $"/// {standardSummaryText}";
                }

                if (parameterList.Parameters.Count == 1)
                {
                    if (TryGetNewValue(out _, out var standardParamText) ||
                        TryGetOldValue(out _, out standardParamText))
                    {
                        expectedText = StringBuilderPool.Borrow()
                                                        .Append("/// ").AppendLine(standardSummaryText)
                                                        .Append("/// ").AppendLine(standardParamText)
                                                        .Return();
                        return false;
                    }

                    return null;
                }

                if (parameterList.Parameters.Count == 2)
                {
                    if (TryGetOldValue(out var oldParameter, out var standardOldParamText) &&
                        TryGetNewValue(out var newParameter, out var standardNewParamText))
                    {
                        if (parameterList.Parameters.IndexOf(oldParameter) < parameterList.Parameters.IndexOf(newParameter))
                        {
                            expectedText = StringBuilderPool.Borrow()
                                                        .Append("/// ").AppendLine(standardSummaryText)
                                                        .Append("/// ").AppendLine(standardOldParamText)
                                                        .Append("/// ").AppendLine(standardNewParamText)
                                                        .Return();
                        }
                        else
                        {
                            expectedText = StringBuilderPool.Borrow()
                                                            .Append("/// ").AppendLine(standardSummaryText)
                                                            .Append("/// ").AppendLine(standardNewParamText)
                                                            .Append("/// ").AppendLine(standardOldParamText)
                                                            .Return();
                        }

                        return false;
                    }

                    return null;
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

            static bool HasSummary(DocumentationCommentTriviaSyntax comment, string expected, out Location errorLocation)
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

            bool TryGetNewValue(out ParameterSyntax parameter, out string standardText)
            {
                standardText = null;
                if (TryFindParameter("NewValue", out parameter))
                {
                    standardText = $"<param name=\"{parameter.Identifier.ValueText}\">The new value of <see cref=\"{backingField.Name}\"/>.</param>";
                    return true;
                }

                return false;
            }

            bool TryGetOldValue(out ParameterSyntax parameter, out string standardText)
            {
                standardText = null;
                if (TryFindParameter("OldValue", out parameter))
                {
                    standardText = $"<param name=\"{parameter.Identifier.ValueText}\">The old value of <see cref=\"{backingField.Name}\"/>.</param>";
                    return true;
                }

                return false;
            }

            bool TryFindParameter(string propertyName, out ParameterSyntax parameter)
            {
                parameter = null;
                if (invocation.ArgumentList is { } argumentList)
                {
                    foreach (var argument in argumentList.Arguments)
                    {
                        using (var walker = SpecificIdentifierNameWalker.Borrow(argument.Expression, propertyName))
                        {
                            if (walker.IdentifierNames.TrySingle(out _) &&
                                methodDeclaration.TryFindParameter(argument, out parameter))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
        }

        private static bool HasParam(DocumentationCommentTriviaSyntax comment, ParameterSyntax current, string expected, [NotNullWhen(true)] out Location? errorLocation)
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

        private static bool TryGetSingleInvocation(IMethodSymbol method, MethodDeclarationSyntax methodDeclaration, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out InvocationExpressionSyntax? invocation)
        {
            invocation = null;
            if (methodDeclaration.Parent is ClassDeclarationSyntax classDeclaration)
            {
                using (var walker = SpecificIdentifierNameWalker.Borrow(classDeclaration, methodDeclaration.Identifier.ValueText))
                {
                    foreach (var identifierName in walker.IdentifierNames)
                    {
                        if (identifierName.Parent is MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax candidate } &&
                            context.SemanticModel.TryGetSymbol(identifierName, context.CancellationToken, out IMethodSymbol? symbol) &&
                            Equals(symbol, method))
                        {
                            if (invocation != null)
                            {
                                invocation = null;
                                return false;
                            }

                            invocation = candidate;
                        }
                    }
                }
            }

            return invocation != null;
        }

        private static PooledSet<ArgumentSyntax> GetCallbackArguments(SyntaxNodeAnalysisContext context, IMethodSymbol method, MethodDeclarationSyntax methodDeclaration)
        {
            // Set is not perfect here but using it as there is no pooled list
            var callbacks = PooledSet<ArgumentSyntax>.Borrow();
            if (methodDeclaration.Parent is ClassDeclarationSyntax classDeclaration)
            {
                using (var walker = SpecificIdentifierNameWalker.Borrow(classDeclaration, method.MetadataName))
                {
                    foreach (var identifierName in walker.IdentifierNames)
                    {
                        if (context.SemanticModel.TryGetSymbol(identifierName, context.CancellationToken, out IMethodSymbol? symbol) &&
                            Equals(symbol, method))
                        {
                            switch (identifierName.Parent)
                            {
                                case ArgumentSyntax argument
                                    when TryGetCallbackArgument(argument, out var callbackArg):
                                    callbacks.Add(callbackArg);
                                    break;
                                case InvocationExpressionSyntax { Parent: ParenthesizedLambdaExpressionSyntax { Parent: ArgumentSyntax argument } }
                                    when TryGetCallbackArgument(argument, out var callbackArg):
                                    callbacks.Add(callbackArg);
                                    break;
                            }
                        }
                    }
                }
            }

            return callbacks;
        }

        private static bool TryGetCallbackArgument(ArgumentSyntax candidate, [NotNullWhen(true)] out ArgumentSyntax? result)
        {
            if (candidate.Parent is ArgumentListSyntax { Parent: ObjectCreationExpressionSyntax { Parent: ArgumentSyntax parent } callbackCreation } &&
                (callbackCreation.Type == KnownSymbols.PropertyChangedCallback ||
                 callbackCreation.Type == KnownSymbols.CoerceValueCallback ||
                 callbackCreation.Type == KnownSymbols.ValidateValueCallback))
            {
                result = parent;
            }
            else
            {
                result = candidate;
            }

            return true;
        }

        private static bool TryGetSenderType(ArgumentSyntax argument, INamedTypeSymbol containingType, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out ITypeSymbol? senderType)
        {
            senderType = null;
            if (argument is { Parent: ArgumentListSyntax { Parent: ObjectCreationExpressionSyntax { Parent: ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax register } } } metaDataCreation } } &&
                PropertyMetadata.TryGetConstructor(metaDataCreation, context.SemanticModel, context.CancellationToken, out _))
            {
                if (DependencyProperty.TryGetRegisterCall(register, context.SemanticModel, context.CancellationToken, out _) ||
                    DependencyProperty.TryGetRegisterReadOnlyCall(register, context.SemanticModel, context.CancellationToken, out _) ||
                    DependencyProperty.TryGetAddOwnerCall(register, context.SemanticModel, context.CancellationToken, out _) ||
                    DependencyProperty.TryGetOverrideMetadataCall(register, context.SemanticModel, context.CancellationToken, out _))
                {
                    senderType = containingType;
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetValueType(ArgumentSyntax argument, INamedTypeSymbol containingType, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out ITypeSymbol? type)
        {
            type = null;
            if (argument is { Parent: ArgumentListSyntax { Parent: { } parent } })
            {
                switch (parent)
                {
                    case ObjectCreationExpressionSyntax { Parent: ArgumentSyntax parentArgument }:
                        return TryGetValueType(parentArgument, containingType, context, out type);
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

                    case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: { } expression } } invocation when
                        DependencyProperty.TryGetAddOwnerCall(invocation, context.SemanticModel, context.CancellationToken, out _) ||
                        DependencyProperty.TryGetOverrideMetadataCall(invocation, context.SemanticModel, context.CancellationToken, out _):
                        {
                            return context.SemanticModel.TryGetSymbol(expression, context.CancellationToken, out var symbol) &&
                                   BackingFieldOrProperty.TryCreateForDependencyProperty(symbol, out var fieldOrProperty) &&
                                   DependencyProperty.TryGetRegisteredType(fieldOrProperty, context.SemanticModel, context.CancellationToken, out type);
                        }
                }
            }

            return false;
        }
    }
}
