#pragma warning disable SA1118 // Parameter should not span multiple lines
namespace WpfAnalyzers
{
    using Microsoft.CodeAnalysis;

    internal static class Descriptors
    {
        internal static readonly DiagnosticDescriptor WPF0001BackingFieldShouldMatchRegisteredName = Create(
            id: "WPF0001",
            title: "Backing field for a DependencyProperty should match registered name",
            messageFormat: "Field '{0}' that is backing field for the DependencyProperty registered as '{1}' should be named '{1}Property'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A dependency property's backing field should be named with the name it is registered with suffixed by 'Property'.\r\n" +
                         "This is the convention in the framework.");

        internal static readonly DiagnosticDescriptor WPF0002BackingFieldShouldMatchRegisteredName = Create(
            id: "WPF0002",
            title: "Backing field for a DependencyPropertyKey should match registered name",
            messageFormat: "Field '{0}' that is backing field for the DependencyPropertyKey registered as '{1}' must be named '{1}PropertyKey'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A DependencyPropertyKey's backing field must be named with the name it is registered with suffixed by 'PropertyKey'.");

        internal static readonly DiagnosticDescriptor WPF0003ClrPropertyShouldMatchRegisteredName = Create(
            id: "WPF0003",
            title: "CLR property for a DependencyProperty should match registered name",
            messageFormat: "Property '{0}' must be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A CLR property accessor for a DependencyProperty must have the same name as the DependencyProperty is registered with.");

        internal static readonly DiagnosticDescriptor WPF0004ClrMethodShouldMatchRegisteredName = Create(
            id: "WPF0004",
            title: "CLR method for a DependencyProperty must match registered name",
            messageFormat: "Method '{0}' must be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "CLR methods for accessing a DependencyProperty must have names matching the name the DependencyProperty is registered with.");

        internal static readonly DiagnosticDescriptor WPF0005PropertyChangedCallbackShouldMatchRegisteredName = Create(
            id: "WPF0005",
            title: "Name of PropertyChangedCallback should match registered name",
            messageFormat: "Method '{0}' should be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of PropertyChangedCallback should match registered name.");

        internal static readonly DiagnosticDescriptor WPF0006CoerceValueCallbackShouldMatchRegisteredName = Create(
            id: "WPF0006",
            title: "Name of CoerceValueCallback should match registered name",
            messageFormat: "Method '{0}' should be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of CoerceValueCallback should match registered name.");

        internal static readonly DiagnosticDescriptor WPF0007ValidateValueCallbackCallbackShouldMatchRegisteredName = Create(
            id: "WPF0007",
            title: "Name of ValidateValueCallback should match registered name",
            messageFormat: "Method '{0}' should be named '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of ValidateValueCallback should match registered name.");

        internal static readonly DiagnosticDescriptor WPF0008DependsOnTarget = Create(
            id: "WPF0008",
            title: "[DependsOn(target)] must exist",
            messageFormat: "[DependsOn(target)] must exist",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "[DependsOn(target)] must exist.");

        internal static readonly DiagnosticDescriptor WPF0010DefaultValueMustMatchRegisteredType = Create(
            id: "WPF0010",
            title: "Default value type must match registered type",
            messageFormat: "Default value for '{0}' must be of type {1}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A DependencyProperty is registered with a type and a default value. The type of the default value must be the same as the registered type.");

        internal static readonly DiagnosticDescriptor WPF0011ContainingTypeShouldBeRegisteredOwner = Create(
            id: "WPF0011",
            title: "Containing type should be used as registered owner",
            messageFormat: "Register containing type: '{0}' as owner",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When registering a DependencyProperty register containing type as owner type.");

        internal static readonly DiagnosticDescriptor WPF0012ClrPropertyShouldMatchRegisteredType = Create(
            id: "WPF0012",
            title: "CLR property type should match registered type",
            messageFormat: "Property '{0}' must be of type {1}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "CLR property type should match registered type.");

        internal static readonly DiagnosticDescriptor WPF0013ClrMethodMustMatchRegisteredType = Create(
            id: "WPF0013",
            title: "CLR accessor for attached property must match registered type",
            messageFormat: "{0} must match registered type {1}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "CLR accessor for attached property must match registered type.");

        internal static readonly DiagnosticDescriptor WPF0014SetValueMustUseRegisteredType = Create(
            id: "WPF0014",
            title: "SetValue must use registered type",
            messageFormat: "{0} must use registered type {1}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use a type that matches registered type when setting the value of a DependencyProperty.");

        internal static readonly DiagnosticDescriptor WPF0015RegisteredOwnerTypeMustBeDependencyObject = Create(
            id: "WPF0015",
            title: "Registered owner type must inherit DependencyObject",
            messageFormat: "Maybe you intended to use '{0}'?",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When registering a DependencyProperty owner type must be a subclass of DependencyObject.");

        internal static readonly DiagnosticDescriptor WPF0016DefaultValueIsSharedReferenceType = Create(
            id: "WPF0016",
            title: "Default value is shared reference type",
            messageFormat: "Default value for '{0}' is a reference type that will be shared among all instances",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When registering a new instance of a reference type as default value the value is shared for all instances of the control.");

        internal static readonly DiagnosticDescriptor WPF0017MetadataMustBeAssignable = Create(
            id: "WPF0017",
            title: "Metadata must be of same type or super type",
            messageFormat: "Metadata must be of same type or super type",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When overriding metadata must be of the same type or subtype of the overridden property's metadata.");

        internal static readonly DiagnosticDescriptor WPF0018DefaultStyleKeyPropertyOverrideMetadataArgument = Create(
            id: "WPF0018",
            title: "Use containing type",
            messageFormat: "Expected new FrameworkPropertyMetadata(typeof({0}))",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Call DefaultStyleKeyProperty.OverrideMetadata with containing type as argument.");

        internal static readonly DiagnosticDescriptor WPF0019CastSenderToCorrectType = Create(
            id: "WPF0019",
            title: "Cast sender to correct type",
            messageFormat: "Sender is of type {0}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Cast sender to correct type.");

        internal static readonly DiagnosticDescriptor WPF0020CastValueToCorrectType = Create(
            id: "WPF0020",
            title: "Cast value to correct type",
            messageFormat: "Value is of type {0}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Cast value to correct type.");

        internal static readonly DiagnosticDescriptor WPF0021DirectCastSenderToExactType = Create(
            id: "WPF0021",
            title: "Cast sender to containing type",
            messageFormat: "Sender is of type {0}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Cast sender to correct type.");

        internal static readonly DiagnosticDescriptor WPF0022DirectCastValueToExactType = Create(
            id: "WPF0022",
            title: "Cast value to correct type",
            messageFormat: "Value is of type {0}",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Cast value to correct type.");

        internal static readonly DiagnosticDescriptor WPF0023ConvertToLambda = Create(
            id: "WPF0023",
            title: "The callback is trivial, convert to lambda",
            messageFormat: "Convert to lambda",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "The callback is trivial, convert to lambda for better locality.");

        internal static readonly DiagnosticDescriptor WPF0024ParameterShouldBeNullable = Create(
            id: "WPF0024",
            title: "Parameter type should be nullable",
            messageFormat: "Parameter type should be nullable",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Parameter type should be nullable.");

        internal static readonly DiagnosticDescriptor WPF0030BackingFieldShouldBeStaticReadonly = Create(
            id: "WPF0030",
            title: "Backing field for a DependencyProperty should be static and readonly",
            messageFormat: "Field '{0}' is backing field for a DependencyProperty and should be static and readonly",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Backing field for a DependencyProperty should be static and readonly.");

        internal static readonly DiagnosticDescriptor WPF0031FieldOrder = Create(
            id: "WPF0031",
            title: "DependencyPropertyKey member must be declared before DependencyProperty member",
            messageFormat: "'{0}' must be declared before '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "DependencyPropertyKey member must be declared before DependencyProperty member.");

        internal static readonly DiagnosticDescriptor WPF0032ClrPropertyGetAndSetSameDependencyProperty = Create(
            id: "WPF0032",
            title: "Use same dependency property in get and set",
            messageFormat: "Property '{0}' must access same dependency property in getter and setter",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use same dependency property in get and set.");

        internal static readonly DiagnosticDescriptor WPF0033UseAttachedPropertyBrowsableForTypeAttribute = Create(
            id: "WPF0033",
            title: "Add [AttachedPropertyBrowsableForType]",
            messageFormat: "Add [AttachedPropertyBrowsableForType(typeof({0}))]",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Add [AttachedPropertyBrowsableForType].");

        internal static readonly DiagnosticDescriptor WPF0034AttachedPropertyBrowsableForTypeAttributeArgument = Create(
            id: "WPF0034",
            title: "Use correct argument for [AttachedPropertyBrowsableForType]",
            messageFormat: "Use [AttachedPropertyBrowsableForType(typeof({0})]",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Use correct argument for [AttachedPropertyBrowsableForType].");

        internal static readonly DiagnosticDescriptor WPF0035ClrPropertyUseSetValueInSetter = Create(
            id: "WPF0035",
            title: "Use SetValue in setter",
            messageFormat: "Use SetValue in setter",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use SetValue in setter.");

        internal static readonly DiagnosticDescriptor WPF0036AvoidSideEffectsInClrAccessors = Create(
            id: "WPF0036",
            title: "Avoid side effects in CLR accessors",
            messageFormat: "Avoid side effects in CLR accessors",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Avoid side effects in CLR accessors.");

        internal static readonly DiagnosticDescriptor WPF0040SetUsingDependencyPropertyKey = Create(
            id: "WPF0040",
            title: "A readonly DependencyProperty must be set with DependencyPropertyKey",
            messageFormat: "Set '{0}' using '{1}'",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A readonly DependencyProperty must be set with DependencyPropertyKey.");

        internal static readonly DiagnosticDescriptor WPF0041SetMutableUsingSetCurrentValue = Create(
            id: "WPF0041",
            title: "Set mutable dependency properties using SetCurrentValue",
            messageFormat: "Use SetCurrentValue({0}, {1})",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Prefer setting mutable dependency properties using SetCurrentValue.");

        internal static readonly DiagnosticDescriptor WPF0042AvoidSideEffectsInClrAccessors = Create(
            id: "WPF0042",
            title: "Avoid side effects in CLR accessors",
            messageFormat: "Avoid side effects in CLR accessors",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Avoid side effects in CLR accessors.");

        internal static readonly DiagnosticDescriptor WPF0043DoNotUseSetCurrentValue = Create(
            id: "WPF0043",
            title: "Don't set DataContext and Style using SetCurrentValue",
            messageFormat: "Use SetValue({0}, {1})",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Set DataContext and Style using SetValue.");

        internal static readonly DiagnosticDescriptor WPF0050XmlnsPrefixMustMatchXmlnsDefinition = Create(
            id: "WPF0050",
            title: "XmlnsPrefix must map to the same url as XmlnsDefinition",
            messageFormat: "There is no [{0}] mapping to '{1}'",
            category: AnalyzerCategory.XmlnsDefinition,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "[XmlnsPrefix] must have a corresponding [XmlnsDefinition] mapping to the same url.");

        internal static readonly DiagnosticDescriptor WPF0051XmlnsDefinitionMustMapExistingNamespace = Create(
            id: "WPF0051",
            title: "XmlnsDefinition must map to existing namespace",
            messageFormat: "[XmlnsDefinition] maps to '{0}' that does not exist",
            category: AnalyzerCategory.XmlnsDefinition,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "XmlnsDefinition must map to existing namespace.");

        internal static readonly DiagnosticDescriptor WPF0052XmlnsDefinitionsDoesNotMapAllNamespaces = Create(
            id: "WPF0052",
            title: "XmlnsDefinitions does not map all namespaces with public types",
            messageFormat: "The following namespaces are not mapped: {0}",
            category: AnalyzerCategory.XmlnsDefinition,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "XmlnsDefinitions does not map all namespaces with public types.");

        internal static readonly DiagnosticDescriptor WPF0060DocumentDependencyPropertyBackingMember = Create(
            id: "WPF0060",
            title: "Backing member for DependencyProperty should have standard documentation text",
            messageFormat: "Backing member for DependencyProperty should have standard documentation text",
            category: AnalyzerCategory.Documentation,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Backing member for DependencyProperty should have standard documentation text.");

        internal static readonly DiagnosticDescriptor WPF0061DocumentClrMethod = Create(
            id: "WPF0061",
            title: "Accessor method for attached property should have standard documentation text",
            messageFormat: "Accessor method for attached property should have standard documentation text",
            category: AnalyzerCategory.Documentation,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Accessor method for attached property should have standard documentation text.");

        internal static readonly DiagnosticDescriptor WPF0062DocumentPropertyChangedCallback = Create(
            id: "WPF0062",
            title: "Property changed callback should have standard documentation text",
            messageFormat: "Property changed callback should have standard documentation text",
            category: AnalyzerCategory.Documentation,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Property changed callback should have standard documentation text.");

        internal static readonly DiagnosticDescriptor WPF0070ConverterDoesNotHaveDefaultField = Create(
            id: "WPF0070",
            title: "Add default field to converter",
            messageFormat: "Add default field to converter",
            category: AnalyzerCategory.IValueConverter,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Add default field to converter.");

        internal static readonly DiagnosticDescriptor WPF0071ConverterDoesNotHaveAttribute = Create(
            id: "WPF0071",
            title: "Add ValueConversion attribute",
            messageFormat: "Add ValueConversion attribute",
            category: AnalyzerCategory.IValueConverter,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Add ValueConversion attribute.");

        internal static readonly DiagnosticDescriptor WPF0072ValueConversionMustUseCorrectTypes = Create(
            id: "WPF0072",
            title: "ValueConversion must use correct types",
            messageFormat: "ValueConversion must use correct types Expected: {0}",
            category: AnalyzerCategory.IValueConverter,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "ValueConversion must use correct types.");

        internal static readonly DiagnosticDescriptor WPF0073ConverterDoesNotHaveAttributeUnknownTypes = Create(
            id: "WPF0073",
            title: "Add ValueConversion attribute (unknown types)",
            messageFormat: "Add ValueConversion attribute (unknown types)",
            category: AnalyzerCategory.IValueConverter,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Add ValueConversion attribute (unknown types).");

        internal static readonly DiagnosticDescriptor WPF0074DefaultMemberOfWrongType = Create(
            id: "WPF0074",
            title: "Use containing type",
            messageFormat: "Use containing type",
            category: AnalyzerCategory.IValueConverter,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use containing type.");

        internal static readonly DiagnosticDescriptor WPF0080MarkupExtensionDoesNotHaveAttribute = Create(
            id: "WPF0080",
            title: "Add MarkupExtensionReturnType attribute",
            messageFormat: "Add MarkupExtensionReturnType attribute",
            category: AnalyzerCategory.MarkupExtension,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Add MarkupExtensionReturnType attribute.");

        internal static readonly DiagnosticDescriptor WPF0081MarkupExtensionReturnTypeMustUseCorrectType = Create(
            id: "WPF0081",
            title: "MarkupExtensionReturnType must use correct return type",
            messageFormat: "MarkupExtensionReturnType must use correct return type Expected: {0}",
            category: AnalyzerCategory.MarkupExtension,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "MarkupExtensionReturnType must use correct return type.");

        internal static readonly DiagnosticDescriptor WPF0082ConstructorArgument = Create(
            id: "WPF0082",
            title: "[ConstructorArgument] must match",
            messageFormat: "[ConstructorArgument] must match Expected: {0}",
            category: AnalyzerCategory.MarkupExtension,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "[ConstructorArgument] must match the name of the constructor parameter.");

        internal static readonly DiagnosticDescriptor WPF0083UseConstructorArgumentAttribute = Create(
            id: "WPF0083",
            title: "Add [ConstructorArgument]",
            messageFormat: "Add [ConstructorArgument(\"{0}\"]",
            category: AnalyzerCategory.MarkupExtension,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Add [ConstructorArgument] for the property.");

        internal static readonly DiagnosticDescriptor WPF0084XamlSetMarkupExtensionAttributeTarget = Create(
            id: "WPF0084",
            title: "Target of [XamlSetMarkupExtension] should exist and have correct signature",
            messageFormat: "Expected a method with signature void ReceiveMarkupExtension(object, XamlSetMarkupExtensionEventArgs)",
            category: AnalyzerCategory.MarkupExtension,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Target of [XamlSetMarkupExtension] should exist and have correct signature.");

        internal static readonly DiagnosticDescriptor WPF0085XamlSetTypeConverterTarget = Create(
            id: "WPF0085",
            title: "Target of [XamlSetTypeConverter] should exist and have correct signature",
            messageFormat: "Expected a method with signature void ReceiveTypeConverter(object, XamlSetTypeConverterEventArgs)",
            category: AnalyzerCategory.MarkupExtension,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Target of [XamlSetTypeConverter] should exist and have correct signature.");

        internal static readonly DiagnosticDescriptor WPF0090RegisterClassHandlerCallbackNameShouldMatchEvent = Create(
            id: "WPF0090",
            title: "Name the invoked method OnEventName",
            messageFormat: "Rename to {0} to match the event",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Name the invoked method OnEventName.");

        internal static readonly DiagnosticDescriptor WPF0091AddAndRemoveHandlerCallbackNameShouldMatchEvent = Create(
            id: "WPF0091",
            title: "Name the invoked method OnEventName",
            messageFormat: "Rename to {0} to match the event",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Name the invoked method OnEventName.");

        internal static readonly DiagnosticDescriptor WPF0100BackingFieldShouldMatchRegisteredName = Create(
            id: "WPF0100",
            title: "Backing field for a RoutedEvent should match registered name.",
            messageFormat: "Field '{0}' that is backing field for the RoutedEvent registered as '{1}' must be named '{1}Event'",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "A routed event's backing field should be named with the name it is registered with suffixed by 'Event'");

        internal static readonly DiagnosticDescriptor WPF0101RegisterContainingTypeAsOwner = Create(
            id: "WPF0101",
            title: "Containing type should be used as registered owner",
            messageFormat: "Register containing type: '{0}' as owner.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "When registering a RoutedEvent register containing type as owner type.");

        internal static readonly DiagnosticDescriptor WPF0102EventDeclarationName = Create(
            id: "WPF0102",
            title: "Name of the event should match registered name.",
            messageFormat: "Rename to: '{0}'.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Name of the event should match registered name.");

        internal static readonly DiagnosticDescriptor WPF0103EventDeclarationAddRemove = Create(
            id: "WPF0103",
            title: "Use same event in add and remove.",
            messageFormat: "Add uses: '{0}', remove uses: '{1}'.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use same event in add and remove.");

        internal static readonly DiagnosticDescriptor WPF0104EventDeclarationAddHandlerInAdd = Create(
            id: "WPF0104",
            title: "Call AddHandler in add.",
            messageFormat: "Call AddHandler in add.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Call AddHandler in add.");

        internal static readonly DiagnosticDescriptor WPF0105EventDeclarationRemoveHandlerInRemove = Create(
            id: "WPF0105",
            title: "Call RemoveHandler in remove.",
            messageFormat: "Call RemoveHandler in remove.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Call RemoveHandler in remove.");

        internal static readonly DiagnosticDescriptor WPF0106EventDeclarationUseRegisteredHandlerType = Create(
            id: "WPF0106",
            title: "Use the registered handler type.",
            messageFormat: "Use the registered handler type {0}.",
            category: AnalyzerCategory.RoutedEvent,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use the registered handler type.");

        internal static readonly DiagnosticDescriptor WPF0107BackingMemberShouldBeStaticReadonly = Create(
            id: "WPF0107",
            title: "Backing member for a RoutedEvent should be static and readonly.",
            messageFormat: "Backing member for a RoutedEvent and should be static and readonly.",
            category: AnalyzerCategory.DependencyProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Backing member for a RoutedEvent should be static and readonly.");

        internal static readonly DiagnosticDescriptor WPF0108DocumentRoutedEventBackingMember = Create(
            id: "WPF0108",
            title: "Backing member for RoutedEvent should have standard documentation text.",
            messageFormat: "Backing member for RoutedEvent should have standard documentation text.",
            category: AnalyzerCategory.Documentation,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "Backing member for RoutedEvent should have standard documentation text.");

        internal static readonly DiagnosticDescriptor WPF0120RegisterContainingMemberAsNameForRoutedCommand = Create(
            id: "WPF0120",
            title: "Register containing member name as name for routed command.",
            messageFormat: "Register {0} as name.",
            category: AnalyzerCategory.RoutedCommand,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Register containing member name as name for routed command.");

        internal static readonly DiagnosticDescriptor WPF0121RegisterContainingTypeAsOwnerForRoutedCommand = Create(
            id: "WPF0121",
            title: "Register containing type as owner for routed command.",
            messageFormat: "Register {0} as owner.",
            category: AnalyzerCategory.RoutedCommand,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Register containing type as owner for routed command.");

        internal static readonly DiagnosticDescriptor WPF0122RegisterRoutedCommand = Create(
            id: "WPF0122",
            title: "Register name and owning type for routed command.",
            messageFormat: "Register name and owning type for routed command.",
            category: AnalyzerCategory.RoutedCommand,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Register containing type as owner for routed command.");

        internal static readonly DiagnosticDescriptor WPF0123BackingMemberShouldBeStaticReadonly = Create(
            id: "WPF0123",
            title: "Backing field for a RoutedCommand should be static and readonly.",
            messageFormat: "Backing member for a RoutedCommand and should be static and readonly.",
            category: AnalyzerCategory.RoutedCommand,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Backing field for a RoutedCommand should be static and readonly.");

        internal static readonly DiagnosticDescriptor WPF0130UseTemplatePartAttribute = Create(
            id: "WPF0130",
            title: "Add [TemplatePart] to the type.",
            messageFormat: "Add {0} to the type.",
            category: AnalyzerCategory.TemplatePart,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Add [TemplatePart] to the type.");

        internal static readonly DiagnosticDescriptor WPF0131TemplatePartType = Create(
            id: "WPF0131",
            title: "Use correct [TemplatePart] type.",
            messageFormat: "Use correct [TemplatePart] type.",
            category: AnalyzerCategory.TemplatePart,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use correct [TemplatePart] type.");

        internal static readonly DiagnosticDescriptor WPF0132UsePartPrefix = Create(
            id: "WPF0132",
            title: "Use PART prefix.",
            messageFormat: "Use PART prefix.",
            category: AnalyzerCategory.TemplatePart,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use PART prefix.");

        internal static readonly DiagnosticDescriptor WPF0133ContentPropertyTarget = Create(
            id: "WPF0133",
            title: "ContentProperty target does not exist.",
            messageFormat: "ContentProperty target does not exist.",
            category: AnalyzerCategory.Correctness,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "ContentProperty target does not exist.");

        internal static readonly DiagnosticDescriptor WPF0140UseContainingTypeComponentResourceKey = Create(
            id: "WPF0140",
            title: "Use containing type when creating a ComponentResourceKey.",
            messageFormat: "Use containing type: {0}.",
            category: AnalyzerCategory.ComponentResourceKey,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use containing type when creating a ComponentResourceKey.");

        internal static readonly DiagnosticDescriptor WPF0141UseContainingMemberComponentResourceKey = Create(
            id: "WPF0141",
            title: "Use containing member as key when creating a ComponentResourceKey.",
            messageFormat: "Use containing member: {0}.",
            category: AnalyzerCategory.ComponentResourceKey,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use containing member as key when creating a ComponentResourceKey.");

        internal static readonly DiagnosticDescriptor WPF0150UseNameofInsteadOfLiteral = Create(
            id: "WPF0150",
            title: "Use nameof() instead of literal.",
            messageFormat: "Use nameof({0}).",
            category: AnalyzerCategory.Correctness,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use nameof() as it is less fragile than string literal.");

        internal static readonly DiagnosticDescriptor WPF0151UseNameofInsteadOfConstant = Create(
            id: "WPF0151",
            title: "Use nameof() instead of constant.",
            messageFormat: "Use nameof({0}).",
            category: AnalyzerCategory.Correctness,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Use nameof() as it is less fragile than constant.");

        internal static readonly DiagnosticDescriptor WPF0170StyleTypedPropertyPropertyTarget = Create(
            id: "WPF0170",
            title: "StyleTypedProperty.Property must exist.",
            messageFormat: "StyleTypedProperty.Property must exist.",
            category: AnalyzerCategory.StyleTypedProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "StyleTypedProperty.Property must exist.");

        internal static readonly DiagnosticDescriptor WPF0171StyleTypedPropertyPropertyType = Create(
            id: "WPF0171",
            title: "StyleTypedProperty.Property must specify a property of type Style.",
            messageFormat: "StyleTypedProperty.Property must specify a property of type Style.",
            category: AnalyzerCategory.StyleTypedProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "StyleTypedProperty.Property must specify a property of type Style.");

        internal static readonly DiagnosticDescriptor WPF0172StyleTypedPropertyPropertySpecified = Create(
            id: "WPF0172",
            title: "StyleTypedProperty.Property must be specified.",
            messageFormat: "StyleTypedProperty.Property must be specified.",
            category: AnalyzerCategory.StyleTypedProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "StyleTypedProperty.Property must be specified.");

        internal static readonly DiagnosticDescriptor WPF0173StyleTypedPropertyStyleTargetType = Create(
            id: "WPF0173",
            title: "StyleTypedProperty.StyleTargetType must be assignable to a type that has a Style property.",
            messageFormat: "StyleTypedProperty.StyleTargetType must be assignable to a type that has a Style property.",
            category: AnalyzerCategory.StyleTypedProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "StyleTypedProperty.StyleTargetType must be assignable to a type that has a Style property.");

        internal static readonly DiagnosticDescriptor WPF0174StyleTypedPropertyStyleSpecified = Create(
            id: "WPF0174",
            title: "StyleTypedProperty.StyleTargetType must be specified.",
            messageFormat: "StyleTypedProperty.StyleTargetType must be specified.",
            category: AnalyzerCategory.StyleTypedProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "StyleTypedProperty.StyleTargetType must be specified.");

        internal static readonly DiagnosticDescriptor WPF0175StyleTypedPropertyPropertyUnique = Create(
            id: "WPF0175",
            title: "StyleTypedProperty.Property must be specified only once.",
            messageFormat: "StyleTypedProperty.Property must be specified only once.",
            category: AnalyzerCategory.StyleTypedProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "StyleTypedProperty.Property must be specified only once.");

        internal static readonly DiagnosticDescriptor WPF0176StyleTypedPropertyMissing = Create(
            id: "WPF0176",
            title: "StyleTypedProperty is missing.",
            messageFormat: "StyleTypedProperty should be specified for {0}.",
            category: AnalyzerCategory.StyleTypedProperty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "StyleTypedProperty is missing.");

        /// <summary>
        /// Create a DiagnosticDescriptor, which provides description about a <see cref="Diagnostic" />.
        /// NOTE: For localizable <paramref name="title" />, <paramref name="description" /> and/or <paramref name="messageFormat" />,
        /// </summary>
        /// <param name="id">A unique identifier for the diagnostic. For example, code analysis diagnostic ID "CA1001".</param>
        /// <param name="title">A short title describing the diagnostic. For example, for CA1001: "Types that own disposable fields should be disposable".</param>
        /// <param name="messageFormat">A format message string, which can be passed as the first argument to <see cref="string.Format(string,object[])" /> when creating the diagnostic message with this descriptor.
        /// For example, for CA1001: "Implement IDisposable on '{0}' because it creates members of the following IDisposable types: '{1}'.</param>
        /// <param name="category">The category of the diagnostic (like Design, Naming etc.). For example, for CA1001: "Microsoft.Design".</param>
        /// <param name="defaultSeverity">Default severity of the diagnostic.</param>
        /// <param name="isEnabledByDefault">True if the diagnostic is enabled by default.</param>
        /// <param name="description">An optional longer description of the diagnostic.</param>
        /// <param name="customTags">Optional custom tags for the diagnostic. See <see cref="WellKnownDiagnosticTags" /> for some well known tags.</param>
        private static DiagnosticDescriptor Create(
          string id,
          string title,
          string messageFormat,
          string category,
          DiagnosticSeverity defaultSeverity,
          bool isEnabledByDefault,
          string description,
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
