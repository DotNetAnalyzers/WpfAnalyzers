namespace WpfAnalyzers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Gu.Roslyn.AnalyzerExtensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class DocComment
    {
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

        internal static (Location, string)? VerifyParamRef(XmlEmptyElementSyntax e, ParameterSyntax parameter)
        {
            if (e.IsParamRef(out var attribute))
            {
                if (attribute.Identifier is IdentifierNameSyntax name)
                {
                    if (name.Identifier.ValueText == parameter.Identifier.ValueText)
                    {
                        return null;
                    }

                    return (name.GetLocation(), parameter.Identifier.ValueText);
                }
            }

            return (e.GetLocation(), $"<paramref name=\"{parameter.Identifier.ValueText}\"/>");
        }

        internal static bool IsParamRef(this XmlEmptyElementSyntax e, [NotNullWhen(true)] out XmlNameAttributeSyntax? attribute)
        {
            if (e.Name?.LocalName.ValueText == "paramref")
            {
                foreach (var candidate in e.Attributes)
                {
                    if (candidate is XmlNameAttributeSyntax name)
                    {
                        attribute = name;
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

        internal static bool TryMatch<T1>(this XmlElementSyntax e, [NotNullWhen(true)] out T1? n1)
            where T1 : XmlNodeSyntax
        {
            n1 = default;
            return e is { Content: { Count: 1 } content } &&
                   Element(0, out n1);

            bool Element<T>(int index, out T? result)
                 where T : class
            {
                return (result = content[index] as T) is { };
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

        internal static bool TryMatch<T1, T2, T3, T4>(this XmlElementSyntax e, [NotNullWhen(true)] out T1? n1, [NotNullWhen(true)] out T2? n2, [NotNullWhen(true)] out T3? n3, [NotNullWhen(true)] out T4? n4)
            where T1 : XmlNodeSyntax
            where T2 : XmlNodeSyntax
            where T3 : XmlNodeSyntax
            where T4 : XmlNodeSyntax
        {
            n1 = default;
            n2 = default;
            n3 = default;
            n4 = default;
            return e is { Content: { Count: 4 } content } &&
                   Element(0, out n1) &&
                   Element(1, out n2) &&
                   Element(2, out n3) &&
                   Element(3, out n4);

            bool Element<T>(int index, out T? result)
                 where T : class
            {
                return (result = content[index] as T) is { };
            }
        }

        internal static bool TryMatch<T1, T2, T3, T4, T5>(this XmlElementSyntax e, [NotNullWhen(true)] out T1? n1, [NotNullWhen(true)] out T2? n2, [NotNullWhen(true)] out T3? n3, [NotNullWhen(true)] out T4? n4, [NotNullWhen(true)] out T5? n5)
            where T1 : XmlNodeSyntax
            where T2 : XmlNodeSyntax
            where T3 : XmlNodeSyntax
            where T4 : XmlNodeSyntax
            where T5 : XmlNodeSyntax
        {
            n1 = default;
            n2 = default;
            n3 = default;
            n4 = default;
            n5 = default;
            return e is { Content: { Count: 5 } content } &&
                   Element(0, out n1) &&
                   Element(1, out n2) &&
                   Element(2, out n3) &&
                   Element(3, out n4) &&
                   Element(4, out n5);

            bool Element<T>(int index, out T? result)
                 where T : class
            {
                return (result = content[index] as T) is { };
            }
        }

        internal static (Location Location, string Text)? VerifyTextCrefText(XmlElementSyntax e, string prefix, string name, string suffix)
        {
            if (e.TryMatch<XmlTextSyntax, XmlEmptyElementSyntax, XmlTextSyntax>(out var prefixElement, out var crefElement, out var suffixElement) &&
                prefixElement.IsMatch(prefix) &&
                suffixElement.IsMatch(suffix))
            {
                if (VerifyCref(crefElement, name) is { } error)
                {
                    return error;
                }

                return null;
            }
            else
            {
                return (e.GetLocation(), $"{prefix}<see cref=\"{name}\"/>{suffix}");
            }
        }
    }
}
