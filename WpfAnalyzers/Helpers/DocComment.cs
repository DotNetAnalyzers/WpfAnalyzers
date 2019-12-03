namespace WpfAnalyzers
{
    using System;
    using System.Linq;
    using System.Text;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    internal static class DocComment
    {
        internal static (Location Location, string Text)? Verify(XmlElementSyntax e, string format, string? p1, string? p2, string? p3)
        {
            var sourceText = e.SyntaxTree.GetText();
            var formatPos = 0;
            var docPos = e.Span.Start;
            string? parameter = null;
            while (docPos < e.Span.End &&
                   formatPos < format.Length)
            {
                if (sourceText[docPos] == format[formatPos])
                {
                    formatPos++;
                    docPos++;
                    continue;
                }

                if (MoveToContent())
                {
                    continue;
                }

                if (format[formatPos] == '{')
                {
                    parameter = NextParameter();
                    if (IsMatch())
                    {
                        formatPos = format.IndexOf('}', formatPos) + 1;
                    }
                    else
                    {
                        var token = e.FindToken(docPos, findInsideTrivia: true);
                        if (token.IsKind(SyntaxKind.XmlTextLiteralToken))
                        {
                            return ContentError();
                        }

                        return (token.GetLocation(), parameter);
                    }

                    string NextParameter()
                    {
                        if (parameter == null)
                        {
                            return p1 ?? throw new FormatException("Too few parameters provided p1 is null.");
                        }

                        if (parameter == p1)
                        {
                            return p2 ?? throw new FormatException("Too few parameters provided p2 is null.");
                        }

                        if (parameter == p2)
                        {
                            return p3 ?? throw new FormatException("Too few parameters provided p3 is null.");
                        }

                        throw new FormatException("Too few parameters provided.");
                    }

                    bool IsMatch()
                    {
                        var paramPos = 0;
                        while (paramPos < parameter.Length &&
                               docPos < e.Span.End)
                        {
                            if (sourceText[docPos] == parameter[paramPos])
                            {
                                paramPos++;
                                docPos++;
                            }
                            else
                            {
                                return false;
                            }
                        }

                        return paramPos == parameter.Length;
                    }
                }
                else
                {
                    return ContentError();
                }

                bool MoveToContent()
                {
                    var token = e.FindToken(docPos, findInsideTrivia: true);
                    switch (token.Kind())
                    {
                        case SyntaxKind.XmlTextLiteralNewLineToken:
                            docPos += token.ValueText.Length;
                            return true;
                        case SyntaxKind.XmlTextLiteralToken
                            when token.HasLeadingTrivia &&
                                 token.LeadingTrivia.Span.Contains(docPos):
                            docPos += token.LeadingTrivia.Span.Length;
                            return true;
                        case SyntaxKind.XmlTextLiteralToken
                            when sourceText[docPos] == ' ' &&
                                 token.SpanStart == docPos:
                            docPos++;
                            return true;
                        default:
                            return false;
                    }
                }
            }

            if (docPos == e.Span.End &&
                formatPos == format.Length)
            {
                return null;
            }

            return ContentError();

            (Location, string) ContentError()
            {
                return (
                    e.SyntaxTree.GetLocation(TextSpan.FromBounds(Start(), End())),
                    Format(format, p1, p2, p3));

                int Start()
                {
                    foreach (var node in e.Content)
                    {
                        foreach (var token in node.DescendantTokens(descendIntoTrivia: true))
                        {
                            switch (token.Kind())
                            {
                                case SyntaxKind.XmlTextLiteralNewLineToken:
                                    continue;
                                case SyntaxKind.XmlTextLiteralToken
                                    when token.ValueText.StartsWith(" ", StringComparison.Ordinal):
                                    return token.SpanStart + 1;
                                default:
                                    return token.SpanStart;
                            }
                        }
                    }

                    return e.GetFirstToken().SpanStart;
                }

                int End()
                {
                    for (var i = e.Content.Count - 1; i >= 0; i--)
                    {
                        foreach (var token in e.Content[i].DescendantTokens(descendIntoTrivia: true).Reverse())
                        {
                            switch (token.Kind())
                            {
                                case SyntaxKind.XmlTextLiteralNewLineToken:
                                    continue;
                                default:
                                    return token.Span.End;
                            }
                        }
                    }

                    return e.GetLastToken().Span.End;
                }
            }
        }

        internal static (Location Location, string Text)? VerifySummary(this DocumentationCommentTriviaSyntax doc, string format, string? p1 = null, string? p2 = null, string? p3 = null)
        {
            if (doc.TryGetSummary(out var summary))
            {
                return Verify(summary, format, p1, p2, p3);
            }

            return (doc.GetLocation(), Format(format, p1, p2, p3));
        }

        internal static (Location Location, string Text)? VerifyParameter(this DocumentationCommentTriviaSyntax doc, string format, IParameterSymbol parameter, string? p1 = null, string? p2 = null)
        {
            if (doc.TryGetParam(parameter.Name, out var param))
            {
                return Verify(param, format, parameter.Name, p1, p2);
            }

            return (parameter.Locations[0], Format(format, parameter.Name, p1, p2));
        }

        internal static (Location Location, string Text)? VerifyReturns(this DocumentationCommentTriviaSyntax doc, string format, string? p1 = null, string? p2 = null, string? p3 = null)
        {
            if (doc.TryGetReturns(out var returns))
            {
                return Verify(returns, format, p1, p2, p3);
            }

            if (doc.TryFirstAncestor(out MethodDeclarationSyntax? method))
            {
                return (method.ReturnType.GetLocation(), Format(format, p1, p2, p3));
            }

            return (doc.GetLocation(), Format(format, p1, p2, p3));
        }

        internal static string Format(string format, params string?[] args)
        {
            var builder = new StringBuilder(format);
            foreach (var arg in args)
            {
                if (arg == null)
                {
                    break;
                }

                if (FindParameter() is { } parameter)
                {
                    _ = builder.Replace(parameter, arg);
                }
                else
                {
                    throw new FormatException("Too many format parameters in the format string.");
                }
            }

            if (FindParameter() is { })
            {
                throw new FormatException("Too many format parameters in the format string.");
            }

            return builder.ToString();

            string? FindParameter()
            {
                var start = -1;
                for (var i = 0; i < builder.Length; i++)
                {
                    if (builder[i] == '{')
                    {
                        start = i;
                    }

                    if (builder[i] == '}')
                    {
                        if (start == -1)
                        {
                            throw new FormatException($"Expected {{ before [{i}].");
                        }

                        return builder.ToString(start, i - start + 1);
                    }
                }

                return null;
            }
        }

        internal static string ToCrefType(this ITypeSymbol type)
        {
            return type switch
            {
                INamedTypeSymbol { IsGenericType: false } simple => simple.Name,
                INamedTypeSymbol { IsGenericType: true } generic => $"{generic.Name}{{{string.Join(",", generic.TypeArguments.Select(x => x.Name))}}}",
                _ => type.Name,
            };
        }
    }
}
