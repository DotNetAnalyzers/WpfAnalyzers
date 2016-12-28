namespace WpfAnalyzers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Editing;

    using WpfAnalyzers.PropertyChanged;
    using WpfAnalyzers.PropertyChanged.Helpers;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakePropertyNotifyCodeFixProvider))]
    [Shared]
    internal class MakePropertyNotifyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(WPF1010MutablePublicPropertyShouldNotify.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider() => BacthFixer.Default;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                          .ConfigureAwait(false);
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                 .ConfigureAwait(false);
            foreach (var diagnostic in context.Diagnostics)
            {
                var token = syntaxRoot.FindToken(diagnostic.Location.SourceSpan.Start);
                if (string.IsNullOrEmpty(token.ValueText))
                {
                    continue;
                }

                var propertyDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<PropertyDeclarationSyntax>();
                var typeDeclaration = propertyDeclaration?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
                if (typeDeclaration == null)
                {
                    continue;
                }

                var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken);
                var type = semanticModel.GetDeclaredSymbolSafe(typeDeclaration, context.CancellationToken);

                IMethodSymbol invoker;
                if (PropertyChanged.Helpers.PropertyChanged.TryGetInvoker(type, semanticModel, context.CancellationToken, out invoker) &&
                    invoker.Parameters[0].Type == KnownSymbol.String)
                {
                    var syntaxGenerator = SyntaxGenerator.GetGenerator(context.Document);
                    var fix = CreateFix(
                        syntaxGenerator,
                        propertyDeclaration,
                        property,
                        invoker,
                        semanticModel,
                        context.CancellationToken);

                    if (fix.NotifyingProperty == propertyDeclaration)
                    {
                        continue;
                    }

                    var updatedTypeDeclaration = typeDeclaration.ReplaceNode(propertyDeclaration, fix.NotifyingProperty)
                                                                .WithBackingField(syntaxGenerator, fix.BackingField, fix.NotifyingProperty);

                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Convert to notifying property.",
                            _ => Task.FromResult(context.Document.WithSyntaxRoot(syntaxRoot.ReplaceNode(typeDeclaration, new[] { updatedTypeDeclaration }))),
                            this.GetType().FullName),
                        diagnostic);
                }
            }
        }

        private static Fix CreateFix(
            SyntaxGenerator syntaxGenerator,
            PropertyDeclarationSyntax propertyDeclaration,
            IPropertySymbol property,
            IMethodSymbol invoker,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            string backingFieldName;
            if (Property.IsMutableAutoProperty(propertyDeclaration))
            {
                backingFieldName = MakePropertyNotifyHelper.BackingFieldNameForAutoProperty(propertyDeclaration);
                var backingField = (FieldDeclarationSyntax)syntaxGenerator.FieldDeclaration(backingFieldName, propertyDeclaration.Type, Accessibility.Private, DeclarationModifiers.None, null);
                var notifyingProperty = propertyDeclaration.WithGetterReturningBackingField(syntaxGenerator, backingFieldName)
                                                                   .WithNotifyingSetter(property, syntaxGenerator, backingFieldName, invoker);
                return new Fix(propertyDeclaration, notifyingProperty, backingField);
            }

            ExpressionStatementSyntax assignStatement;
            if (IsSimpleAssignmentOnly(propertyDeclaration, semanticModel, cancellationToken, out assignStatement, out backingFieldName))
            {
                var notifyingProperty = propertyDeclaration.WithGetterReturningBackingField(syntaxGenerator, backingFieldName)
                                                                   .WithNotifyingSetter(property, syntaxGenerator, assignStatement, backingFieldName, invoker);
                return new Fix(propertyDeclaration, notifyingProperty, null);
            }

            return new Fix(propertyDeclaration, null, null);
        }

        private static bool IsSimpleAssignmentOnly(PropertyDeclarationSyntax propertyDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken, out ExpressionStatementSyntax assignStatement, out string fieldName)
        {
            fieldName = null;
            assignStatement = null;
            AccessorDeclarationSyntax setter;
            if (!propertyDeclaration.TryGetSetAccessorDeclaration(out setter) ||
                setter.Body == null ||
                setter.Body.Statements.Count != 1)
            {
                return false;
            }

            assignStatement = setter.Body.Statements[0] as ExpressionStatementSyntax;
            var assignment = assignStatement?.Expression as AssignmentExpressionSyntax;
            if (assignment == null || (assignment.Right as IdentifierNameSyntax)?.Identifier.ValueText != "value")
            {
                return false;
            }

            var fieldSymbol = semanticModel.GetSymbolSafe(assignment.Left, cancellationToken) as IFieldSymbol;
            if (fieldSymbol == null)
            {
                return false;
            }

            fieldName = fieldSymbol.Name;
            return true;
        }

        private struct Fix
        {
            internal readonly PropertyDeclarationSyntax OldProperty;
            internal readonly PropertyDeclarationSyntax NotifyingProperty;
            internal readonly FieldDeclarationSyntax BackingField;

            public Fix(PropertyDeclarationSyntax oldProperty, PropertyDeclarationSyntax notifyingProperty, FieldDeclarationSyntax backingField)
            {
                this.OldProperty = oldProperty;
                this.NotifyingProperty = notifyingProperty;
                this.BackingField = backingField;
            }
        }

        private class BacthFixer : FixAllProvider
        {
            public static readonly BacthFixer Default = new BacthFixer();
            private static readonly ImmutableArray<FixAllScope> SupportedFixAllScopes = ImmutableArray.Create(FixAllScope.Document);

            private BacthFixer()
            {
            }

            public override IEnumerable<FixAllScope> GetSupportedFixAllScopes()
            {
                return SupportedFixAllScopes;
            }

            public override Task<CodeAction> GetFixAsync(FixAllContext fixAllContext)
            {
                switch (fixAllContext.Scope)
                {
                    case FixAllScope.Document:
                        return this.FixDocumentAsync(fixAllContext);
                    case FixAllScope.Project:
                    case FixAllScope.Solution:
                    case FixAllScope.Custom:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private async Task<CodeAction> FixDocumentAsync(FixAllContext context)
            {
                var syntaxRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                                              .ConfigureAwait(false);
                var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken)
                                                 .ConfigureAwait(false);
                var syntaxGenerator = SyntaxGenerator.GetGenerator(context.Document);

                var diagnostics = await context.GetDocumentDiagnosticsAsync(context.Document).ConfigureAwait(false);
                var fixes = new List<Fix>();
                foreach (var diagnostic in diagnostics)
                {
                    if (diagnostic.Id != WPF1010MutablePublicPropertyShouldNotify.DiagnosticId)
                    {
                        continue;
                    }

                    var propertyDeclaration = syntaxRoot.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<PropertyDeclarationSyntax>();

                    var typeDeclaration = propertyDeclaration?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
                    if (typeDeclaration == null)
                    {
                        continue;
                    }

                    var property = semanticModel.GetDeclaredSymbolSafe(propertyDeclaration, context.CancellationToken);
                    var type = semanticModel.GetDeclaredSymbolSafe(typeDeclaration, context.CancellationToken);

                    IMethodSymbol invoker;
                    if (PropertyChanged.Helpers.PropertyChanged.TryGetInvoker(type, semanticModel, context.CancellationToken, out invoker) &&
                        invoker.Parameters[0].Type == KnownSymbol.String)
                    {
                        var fix = CreateFix(
                            syntaxGenerator,
                            propertyDeclaration,
                            property,
                            invoker,
                            semanticModel,
                            context.CancellationToken);

                        if (fix.NotifyingProperty == propertyDeclaration)
                        {
                            continue;
                        }

                        fixes.Add(fix);
                    }
                }

                var fixedTypes = new List<FixedTypes>();
                foreach (var typeFixes in fixes.GroupBy(x => x.OldProperty.FirstAncestorOrSelf<TypeDeclarationSyntax>()))
                {
                    var fixedTypeDeclaration = typeFixes.Key.ReplaceNodes(
                        typeFixes.Select(x => x.OldProperty),
                        (o, r) => typeFixes.Single(x => x.OldProperty == o)
                                           .NotifyingProperty);

                    foreach (var fix in typeFixes)
                    {
                        fixedTypeDeclaration = fixedTypeDeclaration.WithBackingField(
                            syntaxGenerator,
                            fix.BackingField,
                            fix.NotifyingProperty);
                    }

                    fixedTypes.Add(new FixedTypes(typeFixes.Key, fixedTypeDeclaration));
                }

                return CodeAction.Create(
                    "Convert to notifying property.",
                    _ =>
                        Task.FromResult(
                            context.Document.WithSyntaxRoot(
                                syntaxRoot.ReplaceNodes(fixedTypes.Select(x => x.OldType), (o, r) => fixedTypes.Single(x => x.OldType == o).FixedType))),
                    this.GetType().FullName);
            }
        }
    }
}
