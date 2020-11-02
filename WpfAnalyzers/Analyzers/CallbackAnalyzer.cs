namespace WpfAnalyzers
{
    using System.Collections;
    using System.Collections.Generic;
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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
            Descriptors.WPF0005PropertyChangedCallbackShouldMatchRegisteredName,
            Descriptors.WPF0019CastSenderToCorrectType,
            Descriptors.WPF0020CastValueToCorrectType,
            Descriptors.WPF0021DirectCastSenderToExactType,
            Descriptors.WPF0022DirectCastValueToExactType,
            Descriptors.WPF0062DocumentPropertyChangedCallback);

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
                        using var usages = GetCallbackArguments(context, method, methodDeclaration);
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
                    else if (TryMatchValidateValueCallback(method, out argParameter))
                    {
                        using var usages = GetCallbackArguments(context, method, methodDeclaration);
                        foreach (var callbackArgument in usages)
                        {
                            if (TryGetValueType(callbackArgument, method.ContainingType, context, out var valueType))
                            {
                                HandleCasts(context, methodDeclaration, argParameter, valueType, Descriptors.WPF0020CastValueToCorrectType, Descriptors.WPF0022DirectCastValueToExactType);
                            }
                        }
                    }
                }
                else if (method is { ReturnsVoid: true, IsVirtual: true } &&
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

                    if (method.DeclaredAccessibility.IsEither(Accessibility.Protected, Accessibility.Internal, Accessibility.Public))
                    {
                        foreach (var (location, text) in new OnPropertyChangedDocumentationErrors(new SymbolAndDeclaration<IMethodSymbol, MethodDeclarationSyntax>(method, methodDeclaration), singleInvocation, fieldOrProperty))
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Descriptors.WPF0062DocumentPropertyChangedCallback,
                                    location,
                                    ImmutableDictionary<string, string>.Empty.Add(nameof(DocComment), text)));
                        }
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
                   parameters.TryElementAt<IParameterSymbol>(0, out senderParameter) &&
                   senderParameter.Type.IsAssignableTo(KnownSymbols.DependencyObject, context.Compilation) &&
                   parameters.TryElementAt<IParameterSymbol>(1, out argParameter) &&
                   argParameter.Type == KnownSymbols.DependencyPropertyChangedEventArgs;
        }

        private static bool TryMatchCoerceValueCallback(IMethodSymbol candidate, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out IParameterSymbol? senderParameter, [NotNullWhen(true)] out IParameterSymbol? argParameter)
        {
            senderParameter = null;
            argParameter = null;
            return candidate is { ReturnsVoid: false, Parameters: { Length: 2 } parameters } &&
                   candidate.ReturnType == KnownSymbols.Object &&
                   parameters.TryElementAt<IParameterSymbol>(0, out senderParameter) &&
                   senderParameter.Type.IsAssignableTo(KnownSymbols.DependencyObject, context.Compilation) &&
                   parameters.TryElementAt<IParameterSymbol>(1, out argParameter) &&
                   argParameter.Type == KnownSymbols.Object;
        }

        private static bool TryMatchValidateValueCallback(IMethodSymbol candidate, [NotNullWhen(true)] out IParameterSymbol? argParameter)
        {
            argParameter = null;
            return candidate is { ReturnsVoid: false, Parameters: { Length: 1 } parameters } &&
                   candidate.ReturnType == KnownSymbols.Boolean &&
                   parameters.TryElementAt<IParameterSymbol>(0, out argParameter) &&
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
                    if (pathItem.ValueText == senderParameter!.Name)
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

                        using var walker = SpecificIdentifierNameWalker.Borrow(declaration, senderParameter.Name);
                        return walker.IdentifierNames.TrySingle(out var identifierName) &&
                               context.SemanticModel.TryGetSymbol(identifierName, context.CancellationToken, out IParameterSymbol? symbol) &&
                               symbol.Name == senderParameter.Name;
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
                        using var walker = IdentifierNameWalker.Borrow(argument.Expression);
                        if (!walker.TryFind(argParameter!.Name, out _))
                        {
                            return false;
                        }

                        if (!walker.TryFind("NewValue", out _) &&
                            !walker.TryFind("OldValue", out _))
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }

            bool TryGetStaticCallbackArgument(IMethodSymbol method, MethodDeclarationSyntax methodDeclaration, out ArgumentSyntax? result)
            {
                using var usages = GetCallbackArguments(context, method, methodDeclaration);
                return usages.TrySingle(out result);
            }

            bool Try(out BackingFieldOrProperty backing)
            {
                return IsCalledOnSender() &&
                       ArgsUsesParameter() &&
                       callbackArgument!.Parent is ArgumentListSyntax { Parent: ObjectCreationExpressionSyntax metaDataCreation } &&
                       PropertyMetadata.TryGetDependencyProperty(metaDataCreation, context.SemanticModel, context.CancellationToken, out backing);
            }
        }

        private static void HandleCasts(SyntaxNodeAnalysisContext context, SyntaxNode methodOrLambda, IParameterSymbol parameter, ITypeSymbol expectedType, DiagnosticDescriptor wrongTypeDescriptor, DiagnosticDescriptor notExactTypeDescriptor)
        {
            if (expectedType == null)
            {
                return;
            }

            using var walker = SpecificIdentifierNameWalker.Borrow(methodOrLambda, parameter.Name);
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

        private static bool TryGetSingleInvocation(IMethodSymbol method, MethodDeclarationSyntax methodDeclaration, SyntaxNodeAnalysisContext context, [NotNullWhen(true)] out InvocationExpressionSyntax? invocation)
        {
            invocation = null;
            if (methodDeclaration.Parent is ClassDeclarationSyntax classDeclaration)
            {
                using var walker = SpecificIdentifierNameWalker.Borrow(classDeclaration, methodDeclaration.Identifier.ValueText);
                foreach (var identifierName in walker.IdentifierNames)
                {
                    if (identifierName.Parent is MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax candidate } &&
                        context.SemanticModel.TryGetSymbol(identifierName, context.CancellationToken, out IMethodSymbol? symbol) &&
                        Equals(symbol, method))
                    {
                        if (invocation is { })
                        {
                            invocation = null;
                            return false;
                        }

                        invocation = candidate;
                    }
                }
            }

            return invocation is { };
        }

        private static PooledSet<ArgumentSyntax> GetCallbackArguments(SyntaxNodeAnalysisContext context, IMethodSymbol method, MethodDeclarationSyntax methodDeclaration)
        {
            // Set is not perfect here but using it as there is no pooled list
            var callbacks = PooledSet<ArgumentSyntax>.Borrow();
            if (methodDeclaration.Parent is ClassDeclarationSyntax classDeclaration)
            {
                using var walker = SpecificIdentifierNameWalker.Borrow(classDeclaration, method.MetadataName);
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

        private struct OnPropertyChangedDocumentationErrors : IEnumerable<(Location Location, string Text)>
        {
#pragma warning disable RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
            private readonly SymbolAndDeclaration<IMethodSymbol, MethodDeclarationSyntax> method;
#pragma warning restore RS1008 // Avoid storing per-compilation data into the fields of a diagnostic analyzer.
            private readonly InvocationExpressionSyntax invocation;
            private readonly BackingFieldOrProperty backing;

            internal OnPropertyChangedDocumentationErrors(SymbolAndDeclaration<IMethodSymbol, MethodDeclarationSyntax> method, InvocationExpressionSyntax invocation, BackingFieldOrProperty backing)
            {
                this.method = method;
                this.invocation = invocation;
                this.backing = backing;
            }

            public IEnumerator<(Location Location, string Text)> GetEnumerator()
            {
                if (this.method.Declaration.ParameterList is null)
                {
                    yield break;
                }

                var parameters = this.method.Declaration.ParameterList.Parameters;
                var summaryFormat = "<summary>This method is invoked when the <see cref=\"{backing}\"/> changes.</summary>";
                var oldValueFormat = "<param name=\"{oldValue}\">The old value of <see cref=\"{backing}\"/>.</param>";
                var newValueFormat = "<param name=\"{newValue}\">The new value of <see cref=\"{backing}\"/>.</param>";
                if (this.method.Declaration.TryGetDocumentationComment(out var comment))
                {
                    if (comment.VerifySummary(summaryFormat, this.backing.Name) is { } summaryError)
                    {
                        yield return summaryError;
                    }

                    for (var i = 0; i < this.method.Symbol.Parameters.Length; i++)
                    {
                        if (this.IsParameter("OldValue", parameters[i]) &&
                            comment.VerifyParameter(oldValueFormat, this.method.Symbol.Parameters[i], this.backing.Name) is { } oldValueError)
                        {
                            yield return oldValueError;
                        }
                        else if (this.IsParameter("NewValue", parameters[i]) &&
                            comment.VerifyParameter(newValueFormat, this.method.Symbol.Parameters[i], this.backing.Name) is { } newValueError)
                        {
                            yield return newValueError;
                        }
                    }
                }
                else
                {
                    switch (parameters.Count)
                    {
                        case 0:
                            yield return (
                                this.method.Declaration.Identifier.GetLocation(),
                                $"/// {DocComment.Format(summaryFormat, this.backing.Name)}");
                            break;
                        case 1:
                            if (this.TryFindParameter("NewValue", out _))
                            {
                                yield return (
                                    this.method.Declaration.Identifier.GetLocation(),
                                    $"/// {DocComment.Format(summaryFormat, this.backing.Name)}\n" +
                                    $"/// {DocComment.Format(newValueFormat, this.method.Symbol.Parameters[0].Name, this.backing.Name)}");
                            }

                            if (this.TryFindParameter("OldValue", out _))
                            {
                                yield return (
                                    this.method.Declaration.Identifier.GetLocation(),
                                    $"/// {DocComment.Format(summaryFormat, this.backing.Name)}\n" +
                                    $"/// {DocComment.Format(oldValueFormat, this.method.Symbol.Parameters[0].Name, this.backing.Name)}");
                            }

                            break;
                        case 2:
                            if (this.TryFindParameter("OldValue", out var oldParameter) &&
                                this.TryFindParameter("NewValue", out var newParameter))
                            {
                                if (parameters.IndexOf(oldParameter) < parameters.IndexOf(newParameter))
                                {
                                    yield return (
                                        this.method.Declaration.Identifier.GetLocation(),
                                        $"/// {DocComment.Format(summaryFormat, this.backing.Name)}\n" +
                                        $"/// {DocComment.Format(oldValueFormat, this.method.Symbol.Parameters[0].Name, this.backing.Name)}\n" +
                                        $"/// {DocComment.Format(newValueFormat, this.method.Symbol.Parameters[1].Name, this.backing.Name)}");
                                }
                                else
                                {
                                    yield return (
                                        this.method.Declaration.Identifier.GetLocation(),
                                        $"/// {DocComment.Format(summaryFormat, this.backing.Name)}\n" +
                                        $"/// {DocComment.Format(newValueFormat, this.method.Symbol.Parameters[0].Name, this.backing.Name)}\n" +
                                        $"/// {DocComment.Format(oldValueFormat, this.method.Symbol.Parameters[1].Name, this.backing.Name)}");
                                }
                            }

                            break;
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            private bool IsParameter(string name, ParameterSyntax p) => this.TryFindParameter(name, out var match) && p == match;

            private bool TryFindParameter(string propertyName, [NotNullWhen(true)] out ParameterSyntax? parameter)
            {
                if (this.invocation.ArgumentList is { } argumentList)
                {
                    foreach (var argument in argumentList.Arguments)
                    {
                        using var walker = SpecificIdentifierNameWalker.Borrow(argument.Expression, propertyName);
                        if (walker.IdentifierNames.TrySingle(out _) &&
                            this.method.Declaration.TryFindParameter(argument, out parameter))
                        {
                            return true;
                        }
                    }
                }

                parameter = null;
                return false;
            }
        }
    }
}
