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
