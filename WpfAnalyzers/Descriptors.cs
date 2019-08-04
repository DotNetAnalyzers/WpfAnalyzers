namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class Descriptors
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

        internal static readonly DiagnosticDescriptor WPF0002BackingFieldShouldMatchRegisteredName = Create(
            id: "WPF0002",
            title: "Backing field for a DependencyPropertyKey should match registered name.",
            messageFormat: "Field '{0}' that is backing field for the DependencyPropertyKey registered as '{1}' must be named '{1}PropertyKey'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A DependencyPropertyKey's backing field must be named with the name it is registered with suffixed by 'PropertyKey'");

        internal static readonly DiagnosticDescriptor WPF0003ClrPropertyShouldMatchRegisteredName = Create(
            id: "WPF0003",
            title: "CLR property for a DependencyProperty should match registered name.",
            messageFormat: "Property '{0}' must be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A CLR property accessor for a DependencyProperty must have the same name as the DependencyProperty is registered with.");

        internal static readonly DiagnosticDescriptor WPF0004ClrMethodShouldMatchRegisteredName = Create(
            id: "WPF0004",
            title: "CLR method for a DependencyProperty must match registered name.",
            messageFormat: "Method '{0}' must be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "CLR methods for accessing a DependencyProperty must have names matching the name the DependencyProperty is registered with.");

        internal static readonly DiagnosticDescriptor WPF0005PropertyChangedCallbackShouldMatchRegisteredName = Create(
            id: "WPF0005",
            title: "Name of PropertyChangedCallback should match registered name.",
            messageFormat: "Method '{0}' should be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of PropertyChangedCallback should match registered name.");

        internal static readonly DiagnosticDescriptor WPF0006CoerceValueCallbackShouldMatchRegisteredName = Create(
            id: "WPF0006",
            title: "Name of CoerceValueCallback should match registered name.",
            messageFormat: "Method '{0}' should be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of CoerceValueCallback should match registered name.");

        internal static readonly DiagnosticDescriptor WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName = Create(
            id: "WPF0007",
            title: "Name of ValidateValueCallback should match registered name.",
            messageFormat: "Method '{0}' should be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of ValidateValueCallback should match registered name.");

        internal static readonly DiagnosticDescriptor WPF0010DefaultValueMustMatchRegisteredType = Create(
            id: "WPF0010",
            title: "Default value type must match registered type.",
            messageFormat: "Default value for '{0}' must be of type {1}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A DependencyProperty is registered with a type and a default value. The type of the default value must be the same as the registered type.");

        internal static readonly DiagnosticDescriptor WPF0011ContainingTypeShouldBeRegisteredOwner = Create(
            id: "WPF0011",
            title: "Containing type should be used as registered owner.",
            messageFormat: "Register containing type: '{0}' as owner.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When registering a DependencyProperty register containing type as owner type.");

        internal static readonly DiagnosticDescriptor WPF0012ClrPropertyShouldMatchRegisteredType = Create(
            id: "WPF0012",
            title: "CLR property type should match registered type.",
            messageFormat: "Property '{0}' must be of type {1}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "CLR property type should match registered type.");

        internal static readonly DiagnosticDescriptor WPF0013ClrMethodMustMatchRegisteredType = Create(
            id: "WPF0013",
            title: "CLR accessor for attached property must match registered type.",
            messageFormat: "{0} must match registered type {1}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "CLR accessor for attached property must match registered type.");

        internal static readonly DiagnosticDescriptor WPF0014SetValueMustUseRegisteredType = Create(
            id: "WPF0014",
            title: "SetValue must use registered type.",
            messageFormat: "{0} must use registered type {1}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Use a type that matches registered type when setting the value of a DependencyProperty");

        internal static readonly DiagnosticDescriptor WPF0015RegisteredOwnerTypeMustBeDependencyObject = Create(
            id: "WPF0015",
            title: "Registered owner type must inherit DependencyObject.",
            messageFormat: "Maybe you intended to use '{0}'?",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "When registering a DependencyProperty owner type must be a subclass of DependencyObject.");

        internal static readonly DiagnosticDescriptor WPF0016DefaultValueIsSharedReferenceType = Create(
            id: "WPF0016",
            title: "Default value is shared reference type.",
            messageFormat: "Default value for '{0}' is a reference type that will be shared among all instances.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When registering a new instance of a reference type as default value the value is shared for all instances of the control.");

        internal static readonly DiagnosticDescriptor WPF0017MetadataMustBeAssignable = Create(
            id: "WPF0017",
            title: "Metadata must be of same type or super type.",
            messageFormat: "Metadata must be of same type or super type.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "When overriding metadata must be of the same type or subtype of the overridden property's metadata.");

        internal static readonly DiagnosticDescriptor WPF0018DefaultStyleKeyPropertyOverrideMetadataArgument = Create(
            id: "WPF0018",
            title: "Use containing type.",
            messageFormat: "Expected new FrameworkPropertyMetadata(typeof({0}))",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Call DefaultStyleKeyProperty.OverrideMetadata with containing type as argument.");

        internal static readonly DiagnosticDescriptor WPF0019CastSenderToCorrectType = Create(
            id: "WPF0019",
            title: "Cast sender to correct type.",
            messageFormat: "Sender is of type {0}.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Cast sender to correct type.");

        internal static readonly DiagnosticDescriptor WPF0020CastValueToCorrectType = Create(
            id: "WPF0020",
            title: "Cast value to correct type.",
            messageFormat: "Value is of type {0}.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Cast value to correct type.");

        internal static readonly DiagnosticDescriptor WPF0021DirectCastSenderToExactType = Create(
            id: "WPF0021",
            title: "Cast sender to containing type.",
            messageFormat: "Sender is of type {0}.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Cast sender to correct type.");

        internal static readonly DiagnosticDescriptor WPF0022DirectCastValueToExactType = Create(
            id: "WPF0022",
            title: "Cast value to correct type.",
            messageFormat: "Value is of type {0}.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Cast value to correct type.");

        internal static readonly DiagnosticDescriptor WPF0023ConvertToLambda = Create(
            id: "WPF0023",
            title: "The callback is trivial, convert to lambda.",
            messageFormat: "Convert to lambda.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "The callback is trivial, convert to lambda for better locality.");

        internal static readonly DiagnosticDescriptor WPF0030BackingFieldShouldBeStaticReadonly = Create(
            id: "WPF0030",
            title: "Backing field for a DependencyProperty should be static and readonly.",
            messageFormat: "Field '{0}' is backing field for a DependencyProperty and should be static and readonly.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Backing field for a DependencyProperty should be static and readonly.");

        internal static readonly DiagnosticDescriptor WPF0031FieldOrder = Create(
            id: "WPF0031",
            title: "DependencyPropertyKey field must come before DependencyProperty field.",
            messageFormat: "Field '{0}' must come before '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "DependencyPropertyKey field must come before DependencyProperty field.");

        internal static readonly DiagnosticDescriptor WPF0032ClrPropertyGetAndSetSameDependencyProperty = Create(
            id: "WPF0032",
            title: "Use same dependency property in get and set.",
            messageFormat: "Property '{0}' must access same dependency property in getter and setter",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Use same dependency property in get and set.");

        internal static readonly DiagnosticDescriptor WPF0033UseAttachedPropertyBrowsableForTypeAttribute = Create(
            id: "WPF0033",
            title: "Add [AttachedPropertyBrowsableForType]",
            messageFormat: "Add [AttachedPropertyBrowsableForType(typeof({0}))]",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Add [AttachedPropertyBrowsableForType]");

        internal static readonly DiagnosticDescriptor WPF0034AttachedPropertyBrowsableForTypeAttributeArgument = Create(
            id: "WPF0034",
            title: "Use correct argument for [AttachedPropertyBrowsableForType]",
            messageFormat: "Use [AttachedPropertyBrowsableForType(typeof({0})]",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Use correct argument for [AttachedPropertyBrowsableForType]");

        internal static readonly DiagnosticDescriptor WPF0035ClrPropertyUseSetValueInSetter = Create(
            id: "WPF0035",
            title: "Use SetValue in setter.",
            messageFormat: "Use SetValue in setter.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use SetValue in setter.");

        internal static readonly DiagnosticDescriptor WPF0036AvoidSideEffectsInClrAccessors = Create(
            id: "WPF0036",
            title: "Avoid side effects in CLR accessors.",
            messageFormat: "Avoid side effects in CLR accessors.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Avoid side effects in CLR accessors.");

        internal static readonly DiagnosticDescriptor WPF0040SetUsingDependencyPropertyKey = Create(
            id: "WPF0040",
            title: "A readonly DependencyProperty must be set with DependencyPropertyKey.",
            messageFormat: "Set '{0}' using '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "A readonly DependencyProperty must be set with DependencyPropertyKey.");

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

        internal static readonly DiagnosticDescriptor WPF0041SetMutableUsingSetCurrentValue = Descriptors.Create(
            id: "WPF0041",
            title: "Set mutable dependency properties using SetCurrentValue.",
            messageFormat: "Use SetCurrentValue({0}, {1})",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer setting mutable dependency properties using SetCurrentValue.");

        internal static readonly DiagnosticDescriptor WPF0042AvoidSideEffectsInClrAccessors = Descriptors.Create(
            id: "WPF0042",
            title: "Avoid side effects in CLR accessors.",
            messageFormat: "Avoid side effects in CLR accessors.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Avoid side effects in CLR accessors.");
    }
}
