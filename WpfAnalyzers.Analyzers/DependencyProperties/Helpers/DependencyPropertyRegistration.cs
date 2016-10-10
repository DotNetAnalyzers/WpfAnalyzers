namespace WpfAnalyzers.DependencyProperties
{
    using System.Diagnostics;
    using System.Threading;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DependencyPropertyRegistration
    {
        internal static bool IsAnyDependencyPropertyRegister(this MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.IsDependencyPropertyRegister() ||
                   memberAccess.IsDependencyPropertyRegisterReadOnly() ||
                   memberAccess.IsDependencyPropertyRegisterAttached() ||
                   memberAccess.IsDependencyPropertyRegisterAttachedReadOnly();
        }

        internal static bool IsDependencyPropertyRegister(this MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.IsDependencyPropertyCall(Names.Register);
        }

        internal static bool IsDependencyPropertyRegisterAttached(this MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.IsDependencyPropertyCall(Names.RegisterAttached);
        }

        internal static bool IsDependencyPropertyRegisterReadOnly(this MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.IsDependencyPropertyCall(Names.RegisterReadOnly);
        }

        internal static bool IsDependencyPropertyRegisterAttachedReadOnly(this MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.IsDependencyPropertyCall(Names.RegisterAttachedReadOnly);
        }

        internal static bool IsDependencyPropertyAddOwner(this MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess == null || memberAccess.IsMissing)
            {
                return false;
            }

            return memberAccess.Name?.Identifier.ValueText == Names.AddOwner;
        }

        internal static bool IsDependencyPropertyKeyProperty(this MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess == null || memberAccess.IsMissing)
            {
                return false;
            }

            return memberAccess.Name?.Identifier.ValueText == Names.DependencyProperty;
        }

        private static bool IsDependencyPropertyCall(this MemberAccessExpressionSyntax memberAccess, string name)
        {
            if (memberAccess == null || memberAccess.IsMissing)
            {
                return false;
            }

            if ((memberAccess.Expression as IdentifierNameSyntax)?.Identifier.ValueText != Names.DependencyProperty)
            {
                return false;
            }

            return memberAccess.Name?.Identifier.ValueText == name;
        }

        internal static bool TryGetRegisteredName(this MemberAccessExpressionSyntax registration, SemanticModel semanticModel, CancellationToken cancellationToken, out string result)
        {
            Debug.Assert(registration.IsAnyDependencyPropertyRegister(), "Must be a register call");
            var args = (registration.Parent as InvocationExpressionSyntax)?.ArgumentList;
            var nameArg = args?.Arguments.FirstOrDefault();
            return nameArg.TryGetString(semanticModel, cancellationToken, out result);
        }

        internal static bool TryGetRegisteredType(this MemberAccessExpressionSyntax registration, SemanticModel semanticModel, CancellationToken cancellationToken, out ITypeSymbol result)
        {
            Debug.Assert(registration.IsAnyDependencyPropertyRegister(), "Must be a register call");
            result = null;
            var args = (registration.Parent as InvocationExpressionSyntax)?.ArgumentList;
            if (args == null || args.Arguments.Count < 2)
            {
                return false;
            }

            var typeArg = args.Arguments[1];
            if (typeArg == null)
            {
                return false;
            }

            return typeArg.TryGetTypeofType(semanticModel, cancellationToken, out result);
        }

        internal static bool TryGetRegisteredOwnerType(this MemberAccessExpressionSyntax registerCall, SemanticModel semanticModel, CancellationToken cancellationToken, out ArgumentSyntax argument, out ITypeSymbol result)
        {
            Debug.Assert(registerCall.IsAnyDependencyPropertyRegister(), "Must be a register call");
            result = null;
            var args = (registerCall.Parent as InvocationExpressionSyntax)?.ArgumentList;
            if (args == null || args.Arguments.Count < 3)
            {
                argument = null;
                return false;
            }

            argument = args.Arguments[2];
            return argument.TryGetTypeofType(semanticModel, cancellationToken, out result);
        }
    }
}