namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static partial class Descriptors
    {
        internal static readonly DiagnosticDescriptor WPF0001BackingFieldShouldMatchRegisteredName = Create(
            id: "WPF0001",
            title: "Backing field for a DependencyProperty should match registered name.",
            messageFormat: "Field '{0}' that is backing field for the DependencyProperty registered as '{1}' should be named '{1}Property'.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A dependency property's backing field should be named with the name it is registered with suffixed by 'Property'.\r\n" +
                         "This is the convention in the framework.");

        internal static readonly DiagnosticDescriptor WPF0002BackingFieldShouldMatchRegisteredName = Descriptors.Create(
            id: "WPF0002",
            title: "Backing field for a DependencyPropertyKey should match registered name.",
            messageFormat: "Field '{0}' that is backing field for the DependencyPropertyKey registered as '{1}' must be named '{1}PropertyKey'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A DependencyPropertyKey's backing field must be named with the name it is registered with suffixed by 'PropertyKey'");

        internal static readonly DiagnosticDescriptor WPF0003ClrPropertyShouldMatchRegisteredName = Descriptors.Create(
            id: "WPF0003",
            title: "CLR property for a DependencyProperty should match registered name.",
            messageFormat: "Property '{0}' must be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A CLR property accessor for a DependencyProperty must have the same name as the DependencyProperty is registered with.");

        internal static readonly DiagnosticDescriptor WPF0004ClrMethodShouldMatchRegisteredName = Descriptors.Create(
            id: "WPF0004",
            title: "CLR method for a DependencyProperty must match registered name.",
            messageFormat: "Method '{0}' must be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "CLR methods for accessing a DependencyProperty must have names matching the name the DependencyProperty is registered with.");

        internal static readonly DiagnosticDescriptor WPF0005PropertyChangedCallbackShouldMatchRegisteredName = Descriptors.Create(
            id: "WPF0005",
            title: "Name of PropertyChangedCallback should match registered name.",
            messageFormat: "Method '{0}' should be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of PropertyChangedCallback should match registered name.");

        internal static readonly DiagnosticDescriptor WPF0006CoerceValueCallbackShouldMatchRegisteredName = Descriptors.Create(
            id: "WPF0006",
            title: "Name of CoerceValueCallback should match registered name.",
            messageFormat: "Method '{0}' should be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of CoerceValueCallback should match registered name.");

        internal static readonly DiagnosticDescriptor WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName = Descriptors.Create(
            id: "WPF0007",
            title: "Name of ValidateValueCallback should match registered name.",
            messageFormat: "Method '{0}' should be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of ValidateValueCallback should match registered name.");

        internal static readonly DiagnosticDescriptor WPF0010DefaultValueMustMatchRegisteredType = Descriptors.Create(
            id: "WPF0010",
            title: "Default value type must match registered type.",
            messageFormat: "Default value for '{0}' must be of type {1}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A DependencyProperty is registered with a type and a default value. The type of the default value must be the same as the registered type.");

        internal static readonly DiagnosticDescriptor WPF0011ContainingTypeShouldBeRegisteredOwner = Descriptors.Create(
            id: "WPF0011",
            title: "Containing type should be used as registered owner.",
            messageFormat: "Register containing type: '{0}' as owner.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When registering a DependencyProperty register containing type as owner type.");

        /// <summary>
        /// Create a DiagnosticDescriptor, which provides description about a <see cref="T:Microsoft.CodeAnalysis.Diagnostic" />.
        /// NOTE: For localizable <paramref name="title" />, <paramref name="description" /> and/or <paramref name="messageFormat" />,
        /// use constructor overload <see cref="M:Microsoft.CodeAnalysis.DiagnosticDescriptor.#ctor(System.String,Microsoft.CodeAnalysis.LocalizableString,Microsoft.CodeAnalysis.LocalizableString,System.String,Microsoft.CodeAnalysis.DiagnosticSeverity,System.Boolean,Microsoft.CodeAnalysis.LocalizableString,System.String,System.String[])" />.
        /// </summary>
        /// <param name="id">A unique identifier for the diagnostic. For example, code analysis diagnostic ID "CA1001".</param>
        /// <param name="title">A short title describing the diagnostic. For example, for CA1001: "Types that own disposable fields should be disposable".</param>
        /// <param name="messageFormat">A format message string, which can be passed as the first argument to <see cref="M:System.String.Format(System.String,System.Object[])" /> when creating the diagnostic message with this descriptor.
        /// For example, for CA1001: "Implement IDisposable on '{0}' because it creates members of the following IDisposable types: '{1}'.</param>
        /// <param name="category">The category of the diagnostic (like Design, Naming etc.). For example, for CA1001: "Microsoft.Design".</param>
        /// <param name="defaultSeverity">Default severity of the diagnostic.</param>
        /// <param name="isEnabledByDefault">True if the diagnostic is enabled by default.</param>
        /// <param name="description">An optional longer description of the diagnostic.</param>
        /// <param name="customTags">Optional custom tags for the diagnostic. See <see cref="T:Microsoft.CodeAnalysis.WellKnownDiagnosticTags" /> for some well known tags.</param>
        internal static DiagnosticDescriptor Create(
          string id,
          string title,
          string messageFormat,
          string category,
          DiagnosticSeverity defaultSeverity,
          bool isEnabledByDefault,
          string description = null,
          params string[] customTags)
        {
            return new DiagnosticDescriptor(
                id: id,
                title: title,
                messageFormat: messageFormat,
                category: category,
                defaultSeverity: defaultSeverity,
                isEnabledByDefault: isEnabledByDefault,
                description: description,
                helpLinkUri: $"https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/{id}.md",
                customTags: customTags);
        }
    }
}
