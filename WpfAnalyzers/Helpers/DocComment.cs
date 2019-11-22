namespace WpfAnalyzers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
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
                    e.SyntaxTree.GetLocation(TextSpan.FromBounds(e.Content.First().SpanStart, e.Content.Last().Span.End)),
                    Format(format, p1, p2, p3));
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
                            throw new FormatException($"Expected {{ before [i].");
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

        internal static (Location, string)? VerifyCref(XmlEmptyElementSyntax e, string nameMember)
        {
            if (e.IsCref(out var attribute))
            {
                if (attribute.Cref is NameMemberCrefSyntax { Name: IdentifierNameSyntax name })
                {
                    if (name.Identifier.ValueText == nameMember)
                    {
                        return null;
                    }

                    return (name.GetLocation(), nameMember);
                }
            }

            return (e.GetLocation(), $"<see cref=\"{nameMember}\"/>");
        }

        internal static bool IsCref(this XmlEmptyElementSyntax e, [NotNullWhen(true)] out XmlCrefAttributeSyntax? attribute)
        {
            if (e.Name?.LocalName.ValueText == "see")
            {
                foreach (var candidate in e.Attributes)
                {
                    if (candidate is XmlCrefAttributeSyntax cref)
                    {
                        attribute = cref;
                        return true;
                    }
                }
            }

            attribute = null;
            return false;
        }

        internal static bool IsMatch(this XmlTextSyntax xmlText, string text)
        {
            return xmlText.TextTokens.TrySingle(x => x.IsKind(SyntaxKind.XmlTextLiteralToken) && !string.IsNullOrWhiteSpace(x.ValueText), out var token) &&
                   Matches(token.ValueText);

            bool Matches(string valueText)
            {
                var index = valueText.IndexOf(text);
                if (index < 0)
                {
                    return false;
                }

                for (var i = 0; i < index; i++)
                {
                    if (!char.IsWhiteSpace(valueText[i]) &&
                        valueText[i] != '\\')
                    {
                        return false;
                    }
                }

                for (var i = index + text.Length + 1; i < valueText.Length; i++)
                {
                    if (!char.IsWhiteSpace(valueText[i]) &&
                        valueText[i] != '\\')
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        internal static bool TryMatch<T1, T2, T3>(this XmlElementSyntax e, [NotNullWhen(true)] out T1? n1, [NotNullWhen(true)] out T2? n2, [NotNullWhen(true)] out T3? n3)
            where T1 : XmlNodeSyntax
            where T2 : XmlNodeSyntax
            where T3 : XmlNodeSyntax
        {
            n1 = default;
            n2 = default;
            n3 = default;
            return e is { Content: { Count: 3 } content } &&
                   Element(0, out n1) &&
                   Element(1, out n2) &&
                   Element(2, out n3);

            bool Element<T>(int index, out T? result)
                 where T : class
            {
                return (result = content[index] as T) is { };
            }
        }
    }
}
