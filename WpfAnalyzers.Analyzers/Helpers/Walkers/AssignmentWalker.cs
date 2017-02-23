namespace WpfAnalyzers
{
    using System.Collections.Generic;
    using System.Threading;
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

        /// <summary>
        /// Gets a list with all <see cref="AssignmentExpressionSyntax"/> in the scope.
        /// </summary>
        public IReadOnlyList<AssignmentExpressionSyntax> Assignments => this.assignments;

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            this.assignments.Add(node);
            base.VisitAssignmentExpression(node);
        }

        internal static Pool<AssignmentWalker>.Pooled Create(SyntaxNode node)
        {
            var pooled = Cache.GetOrCreate();
            pooled.Item.Visit(node);
            return pooled;
        }

        internal static bool Assigns(IFieldSymbol field, SyntaxNode scope, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            if (field == null ||
                scope == null)
            {
                return false;
            }

            using (var pooledAssignments = Create(scope))
            {
                foreach (var assignment in pooledAssignments.Item.Assignments)
                {
                    var assignedSymbol = semanticModel.GetSymbolSafe(assignment.Left, cancellationToken);
                    if (field.Equals(assignedSymbol))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
