# WpfAnalyzers

[![Join the chat at https://gitter.im/DotNetAnalyzers/WpfAnalyzers](https://badges.gitter.im/DotNetAnalyzers/WpfAnalyzers.svg)](https://gitter.im/DotNetAnalyzers/WpfAnalyzers?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/25nvar8j6evtmtg4/branch/master?svg=true)](https://ci.appveyor.com/project/JohanLarsson/wpfanalyzers-twfog/branch/master)
[![Build Status](https://dev.azure.com/DotNetAnalyzers/WpfAnalyzers/_apis/build/status/DotNetAnalyzers.WpfAnalyzers?branchName=master)](https://dev.azure.com/DotNetAnalyzers/WpfAnalyzers/_build/latest?definitionId=2&branchName=master)
[![NuGet](https://img.shields.io/nuget/v/WpfAnalyzers.svg)](https://www.nuget.org/packages/WpfAnalyzers/)

Roslyn analyzers for WPF.
* 1.x versions are for Visual Studio 2015.
* 2.x versions are for Visual Studio 2017.
* 3.x versions are for Visual Studio 2019+.

| Id       | Title
| :--      | :--
| [WPF0001](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0001.md)| Backing field for a DependencyProperty should match registered name.
| [WPF0002](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0002.md)| Backing field for a DependencyPropertyKey should match registered name.
| [WPF0003](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0003.md)| CLR property for a DependencyProperty should match registered name.
| [WPF0004](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0004.md)| CLR method for a DependencyProperty must match registered name.
| [WPF0005](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0005.md)| Name of PropertyChangedCallback should match registered name.
| [WPF0006](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0006.md)| Name of CoerceValueCallback should match registered name.
| [WPF0007](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0007.md)| Name of ValidateValueCallback should match registered name.
| [WPF0008](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0008.md)| [DependsOn(target)] must exist.
| [WPF0010](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0010.md)| Default value type must match registered type.
| [WPF0011](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0011.md)| Containing type should be used as registered owner.
| [WPF0012](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0012.md)| CLR property type should match registered type.
| [WPF0013](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0013.md)| CLR accessor for attached property must match registered type.
| [WPF0014](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0014.md)| SetValue must use registered type.
| [WPF0015](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0015.md)| Registered owner type must inherit DependencyObject.
| [WPF0016](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0016.md)| Default value is shared reference type.
| [WPF0017](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0017.md)| Metadata must be of same type or super type.
| [WPF0018](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0018.md)| Use containing type.
| [WPF0019](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0019.md)| Cast sender to correct type.
| [WPF0020](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0020.md)| Cast value to correct type.
| [WPF0021](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0021.md)| Cast sender to containing type.
| [WPF0022](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0022.md)| Cast value to correct type.
| [WPF0023](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0023.md)| The callback is trivial, convert to lambda.
| [WPF0030](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0030.md)| Backing field for a DependencyProperty should be static and readonly.
| [WPF0031](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0031.md)| DependencyPropertyKey member must be declared before DependencyProperty member.
| [WPF0032](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0032.md)| Use same dependency property in get and set.
| [WPF0033](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0033.md)| Add [AttachedPropertyBrowsableForType]
| [WPF0034](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0034.md)| Use correct argument for [AttachedPropertyBrowsableForType]
| [WPF0035](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0035.md)| Use SetValue in setter.
| [WPF0036](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0036.md)| Avoid side effects in CLR accessors.
| [WPF0040](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0040.md)| A readonly DependencyProperty must be set with DependencyPropertyKey.
| [WPF0041](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0041.md)| Set mutable dependency properties using SetCurrentValue.
| [WPF0042](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0042.md)| Avoid side effects in CLR accessors.
| [WPF0043](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0043.md)| Don't set DataContext using SetCurrentValue.
| [WPF0050](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0050.md)| XmlnsPrefix must map to the same url as XmlnsDefinition.
| [WPF0051](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0051.md)| XmlnsDefinition must map to existing namespace.
| [WPF0052](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0052.md)| XmlnsDefinitions does not map all namespaces with public types.
| [WPF0060](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0060.md)| Backing member for DependencyProperty should have standard documentation text.
| [WPF0061](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0061.md)| Accessor method for attached property should have standard documentation text.
| [WPF0062](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0062.md)| Property changed callback should have standard documentation text.
| [WPF0070](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0070.md)| Add default field to converter.
| [WPF0071](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0071.md)| Add ValueConversion attribute.
| [WPF0072](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0072.md)| ValueConversion must use correct types.
| [WPF0073](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0073.md)| Add ValueConversion attribute (unknown types).
| [WPF0074](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0074.md)| Use containing type.
| [WPF0080](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0080.md)| Add MarkupExtensionReturnType attribute.
| [WPF0081](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0081.md)| MarkupExtensionReturnType must use correct return type.
| [WPF0082](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0082.md)| [ConstructorArgument] must match.
| [WPF0083](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0083.md)| Add [ConstructorArgument].
| [WPF0084](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0084.md)| Target of [XamlSetMarkupExtension] should exist and have correct signature.
| [WPF0085](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0085.md)| Target of [XamlSetTypeConverter] should exist and have correct signature.
| [WPF0090](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0090.md)| Name the invoked method OnEventName.
| [WPF0091](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0091.md)| Name the invoked method OnEventName.
| [WPF0100](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0100.md)| Backing field for a RoutedEvent should match registered name.
| [WPF0101](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0101.md)| Containing type should be used as registered owner.
| [WPF0102](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0102.md)| Name of the event should match registered name.
| [WPF0103](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0103.md)| Use same event in add and remove.
| [WPF0104](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0104.md)| Call AddHandler in add.
| [WPF0105](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0105.md)| Call RemoveHandler in remove.
| [WPF0106](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0106.md)| Use the registered handler type.
| [WPF0107](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0107.md)| Backing member for a RoutedEvent should be static and readonly.
| [WPF0108](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0108.md)| Backing member for RoutedEvent should have standard documentation text.
| [WPF0120](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0120.md)| Register containing member name as name for routed command.
| [WPF0121](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0121.md)| Register containing type as owner for routed command.
| [WPF0122](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0122.md)| Register name and owning type for routed command.
| [WPF0123](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0123.md)| Backing field for a RoutedCommand should be static and readonly.
| [WPF0130](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0130.md)| Add [TemplatePart] to the type.
| [WPF0131](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0131.md)| Use correct [TemplatePart] type.
| [WPF0132](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0132.md)| Use PART prefix.
| [WPF0133](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0133.md)| ContentProperty target does not exist.
| [WPF0140](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0140.md)| Use containing type when creating a ComponentResourceKey.
| [WPF0141](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0141.md)| Use containing member as key when creating a ComponentResourceKey.
| [WPF0150](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0150.md)| Use nameof() instead of literal.
| [WPF0151](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0151.md)| Use nameof() instead of constant.
| [WPF0170](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0170.md)| StyleTypedProperty.Property must exist.
| [WPF0171](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0171.md)| StyleTypedProperty.Property must specify a property of type Style.
| [WPF0172](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0172.md)| StyleTypedProperty.Property must be specified.
| [WPF0173](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0173.md)| StyleTypedProperty.StyleTargetType must be assignable to a type that has a Style property.
| [WPF0174](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0174.md)| StyleTypedProperty.StyleTargetType must be specified.
| [WPF0175](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0175.md)| StyleTypedProperty.Property must be specified only once.
| [WPF0176](https://github.com/DotNetAnalyzers/WpfAnalyzers/tree/master/documentation/WPF0176.md)| StyleTypedProperty is missing.

## Using WpfAnalyzers

The preferable way to use the analyzers is to add the nuget package [WpfAnalyzers](https://www.nuget.org/packages/WpfAnalyzers/)
to the project(s).

The severity of individual rules may be configured using [rule set files](https://msdn.microsoft.com/en-us/library/dd264996.aspx)
in Visual Studio 2015.

## Installation

WpfAnalyzers can be installed using [Paket](https://fsprojects.github.io/Paket/) or the NuGet command line or the NuGet Package Manager in Visual Studio 2015.


**Install using the command line:**
```bash
Install-Package WpfAnalyzers
```

## Updating

The ruleset editor does not handle changes IDs well, if things get out of sync you can try:

1) Close visual studio.
2) Edit the ProjectName.rulset file and remove the WpfAnalyzers element.
3) Start visual studio and add back the desired configuration.

Above is not ideal, sorry about this. Not sure this is our bug.


## Current status

Early alpha, names and IDs may change.
