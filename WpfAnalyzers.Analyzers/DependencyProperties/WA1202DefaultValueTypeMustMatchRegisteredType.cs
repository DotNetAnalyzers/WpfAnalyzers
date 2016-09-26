//namespace WpfAnalyzers.DependencyProperties
//{
//    using System;
//    using System.Collections.Immutable;
//    using Microsoft.CodeAnalysis;
//    using Microsoft.CodeAnalysis.CSharp;
//    using Microsoft.CodeAnalysis.CSharp.Syntax;
//    using Microsoft.CodeAnalysis.Diagnostics;

//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    internal class WA1202DefaultValueTypeMustMatchRegisteredType : DiagnosticAnalyzer
//    {
//        public const string DiagnosticId = "WA1202";
//        private const string Title = "DependencyProperty default value must be of the type it is registered as.";
//        private const string MessageFormat = "DependencyProperty '{0}' default value must be of type {1}";
//        private const string Description = Title;
//        private const string HelpLink = "http://stackoverflow.com/";

//        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(
//            DiagnosticId,
//            Title,
//            MessageFormat,
//            AnalyzerCategory.DependencyProperties,
//            DiagnosticSeverity.Error,
//            AnalyzerConstants.EnabledByDefault,
//            Description,
//            HelpLink);

//        /// <inheritdoc/>
//        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

//        /// <inheritdoc/>
//        public override void Initialize(AnalysisContext context)
//        {
//            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
//            context.EnableConcurrentExecution();
//            context.RegisterSyntaxNodeAction(HandleFieldDeclaration, SyntaxKind.FieldDeclaration);
//        }

//        private static void HandleFieldDeclaration(SyntaxNodeAnalysisContext context)
//        {
//            var fieldSymbol = (IFieldSymbol)context.ContainingSymbol;
//            if (!fieldSymbol.IsDependencyPropertyField())
//            {
//                return;
//            }

//            var fieldDeclaration = context.Node as FieldDeclarationSyntax;
//            if (fieldDeclaration == null || fieldDeclaration.IsMissing)
//            {
//                return;
//            }

//            var type = fieldDeclaration.DependencyPropertyRegisteredType();
//            var defaultValue = fieldDeclaration.DependencyPropertyRegisteredDefaultValue();
//            throw new NotImplementedException();
//        }
//    }
//}