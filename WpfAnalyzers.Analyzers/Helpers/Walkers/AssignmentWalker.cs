namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal sealed class AssignmentWalker : PooledWalker<AssignmentWalker>
    {
        private readonly List<AssignmentExpressionSyntax> assignments = new List<AssignmentExpressionSyntax>();

        private AssignmentWalker()
        {
        }

        /// <summary>
        /// Gets a list with all <see cref="AssignmentExpressionSyntax"/> in the scope.
        /// </summary>
        public IReadOnlyList<AssignmentExpressionSyntax> Assignments => this.assignments;

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            this.assignments.Add(node);
            base.VisitAssignmentExpression(node);
        }

        internal static AssignmentWalker Borrow(SyntaxNode node) => BorrowAndVisit(node, () => new AssignmentWalker());

        internal static bool TrySingle(SyntaxNode node, out AssignmentExpressionSyntax result)
        {
            using (var walker = Borrow(node))
            {
                return walker.assignments.TrySingle(out result);
            }
        }

        protected override void Clear()
        {
            this.assignments.Clear();
        }
    }
}
