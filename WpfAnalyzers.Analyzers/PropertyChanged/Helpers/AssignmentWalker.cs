namespace WpfAnalyzers.PropertyChanged.Helpers
{
    using System.Collections.Generic;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class AssignmentWalker : CSharpSyntaxWalker
    {
        private static readonly Pool<AssignmentWalker> Cache = new Pool<AssignmentWalker>(
                                                                   () => new AssignmentWalker(),
                                                                   x => x.assignments.Clear());

        private readonly List<AssignmentExpressionSyntax> assignments = new List<AssignmentExpressionSyntax>();

        private AssignmentWalker()
        {
        }

        public IReadOnlyList<AssignmentExpressionSyntax> Assignments => this.assignments;

        public static Pool<AssignmentWalker>.Pooled Create(SyntaxNode node)
        {
            var pooled = Cache.GetOrCreate();
            pooled.Item.Visit(node);
            return pooled;
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            this.assignments.Add(node);
            base.VisitAssignmentExpression(node);
        }
    }
}