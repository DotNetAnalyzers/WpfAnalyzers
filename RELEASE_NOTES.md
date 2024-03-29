#### 4.1.1
* BUGFIX: WPF0092 allow RoutedEventHandler

#### 4.1.0
* BUGFIX: WPF0023 when missing explicit : UserControl
* FEATURE: Check delegate types
* FEATURE: Refactor event to routed event

#### 4.0.2
* BUGFIX: WPF0012 Allow accessor property to be nullable
* BUGFIX: WPF0073 don't warn when generic
* BUGFIX: Handle GetAsFrozen

#### 4.0.1
* BUGFIX: WPF0090 no warning when used by more than one registration.

#### 4.0.0
* BREAKING: For VS2022+ now.
* BUGFIX: AD0001 Could not load file or assembly

#### 3.5.4
* BUFIX: IsRepresentationPreservingConversion when cast reference type.

#### 3.5.3
* BUGFIX: Don't use SymbolEquaityComparer

#### 3.5.2
* BUGFIX: Handle empty bodies #291

#### 3.5.1
* BUGFIX: WPF0041 should not nag about StyleProperty

#### 3.5.0
* BREAKING: Change all DiagnosticSeverity.Error to DiagnosticSeverity.Warning

#### 3.4.0
* BUGFIX: Suppress SA1202
* BUGFIX: WPF0023  don't warn about virtual methods
* FEATURE: WPF0024 require nullable coerce callback parameter

#### 3.3.0
* BREAKING: Roslyn 3.5.0, requires a recent VisualStudio, not sure about exact version.
* FEATURE: Initial support for nullable types.

#### 3.2.0
* FEATURE: Change to dependency property refactorings.
* BUGFIX: switch expression WPF0072
* BUGFIX: Only change SetValue to SetCurrentValue when in lambda

#### 3.1.1
* Handle regressions in Roslyn 3.7

#### 3.1.0
* BUGFIX: Handle using C = C
* BUGFIX WPF014 when null coalesce

#### 2.3
* NEW RULES: WPF0171-176
* FEATURE: Use nameof() fix.
* BUGFIX: WPF005 warn only on virtual.
* BUGFIX WPF0070 when list.

#### 2.2.0.3
* BUGFIX: WPF0080 when generic type

#### 2.2.0
* Fix for WPF0012.
* WPF0012 when AddOwner.
* WPF001 require field name to be same as AddOwner source.
* WPF0003 handle AddOwner when source is not in AST.

#### 2.1.7
* FEATURE: New analyzers WPF0130, WPF0131, WPF0132, WPF0140, WPF0141

#### 2.1.6
* FEATURE: New analyzers WPF0107, WPF0120, WPF0121, WPF0122, WPF0123

#### 2.1.5.3
* BUGFIX: WPF007 don't nag when validation method is not in containing class.
* BUGFIX: WPF070 don't nag when constructor has parameters.

#### 2.1.5
* BREAKING: Move docs analyzers to category WpfAnalyzers.Documentation.
* Feature: Check and generate standard docs.
 
#### 2.1.4
* Feature: new analyzer convert to lambda.
* Feature: WPF0020 and WPF0022 for OverrideMetadata
* Feature: WPF0020 and WPF0022 for AddOwner
* Feature: WPF0019 and WPF0021 check lambdas

#### 2.1.2.1
* Feature: tweak perf.

#### 2.1.2
* BUGFIX: better check of casts
* Feature: new analyzers for direct casts.

#### 2.1.1
* BUGFIX: allow as casts of interfaces.

#### 2.1.0
* WPF0050 default warning.
* BUGFIX: handle as casts.
* FEATURE: Handle is pattern matching
* FEATURE: Handle switch pattern matching.

#### 1.0.0
* New analyzers

#### 0.4.0
* New analyzers

#### 0.3.3
* BUGFIX: WPF0070 handle internal
* BUGFIX: WPF0081 allow more specific types when object
* Feature: Stub code gen for IValueConverter & IMultiValueConverter

#### 0.3.1
* BUGFIX: Tweak code for determining representation preserving conversion.

#### 0.3.0
* BREAKING CHANGE: move property changed analyzers to https://github.com/DotNetAnalyzers/PropertyChangedAnalyzers.

#### 0.2.0
* BREAKING CHANGE: renumbered & renamed ids and titles.

#### 0.1.0
* Initial with ony one analyzer & fix.
