---
title: Pixeval Code Style
scope: src excluding src/lib
---

# Pixeval Code Style

Use this guide for Pixeval main-project code in `src`, but exclude vendored/submodule code under `src/lib`. Prefer nearby file style over global rules when they differ.

## Baseline

- Use UTF-8, CRLF, final newline, 4-space indentation, no tabs.
- Keep C# `using` directives outside namespaces, sort `System` first, and do not split import groups with blank lines.
- Use file-scoped namespaces in normal C# files.
- Preserve existing file headers. Do not add a header to files that do not already use one unless the surrounding folder does.
- Keep changes narrow and in the existing architecture: view code in `.axaml.cs`, view models in `ViewModels`, reusable Avalonia controls in `Controls`, app state in `AppManagement`, helpers in `Utilities`.
- Pair `.axaml` and `.axaml.cs` files for views/controls. Constructors that only initialize XAML are commonly `public Foo() => InitializeComponent();`.
- Assume modern C#: the main Avalonia project targets `net10.0`, nullable is enabled, `LangVersion` is preview, and compiled bindings are enabled by default.
- Do not introduce broad dependency or project-file churn. Reuse existing packages and helper projects unless the requested change truly requires a new dependency.

## C# Preferences

- Prefer `var`, including built-in types and apparent types.
- Prefer pattern matching:
  - `count is 0`, `count is not 0`, `value is null`, `value is not null`.
  - `obj is { } value` to bind non-null values.
  - Property/list patterns such as `entry is { Width: > 0, Height: > 0 }`.
  - `is not { ... }` guard clauses for failed shape checks.
- Prefer `switch` and switch expressions over long `if`/`else if` ladders, especially for enum, union-like, state, command, and type dispatch.
- Prefer expression-bodied members when they fit on one readable line: properties, accessors, simple methods, local functions, converters, and operators.
- Prefer object/collection initializers, collection expressions (`[]`, `[.. items]`), null propagation, `??`, throw expressions, range/index operators, tuple names, and inferred anonymous member names where clear.
- Prefer direct guard clauses. Braces may be omitted for very short single-line bodies when nearby code does so.
- Use `and`, `or`, `not` patterns when they make conditions clearer, e.g. `UpdateState is not A and not B`.
- Cast with a space after the cast: `(Type) value`.
- Keep binary operators spaced and wrap operators at the beginning of continued lines.
- Prefer `static` local functions when they do not capture.
- Prefer simple `using var` declarations over nested `using` blocks.
- Use discard variables for intentionally unused values.
- Use `ArgumentOutOfRangeException(nameof(value))` or equivalent default arms for supposedly exhaustive switches.
- Use records/record structs for immutable data carriers, tokens, AST nodes, small result types, and value-like layout state.
- Use primary constructors when the surrounding type already follows that style, especially for small wrappers, settings entries, factories, and persistent managers.
- Use `Try...` names for non-throwing attempts (`TryValidate`, `TryReset`, `TryResume`, `TryDelete`) and return nullable/out values consistently with nearby code.
- In low-level async utilities, pass `CancellationToken token = default` and preserve existing `ConfigureAwait(false)` usage. In UI/view-model code, stay on the existing await style.
- Keep computed UI state as expression-bodied properties where possible, then raise `OnPropertyChanged(nameof(...))` for dependent properties when the source state changes.
- Prefer centralized helpers and extensions already present in `Pixeval.Utilities`, such as `SelectNotNull`, `PickMax`/`PickClosest`, `ForEach`, `FileHelper`, `MakoHelper`, and cache helpers.
- Use `Lazy<T>`, `FrozenDictionary`, generated regex, or static readonly converter instances for reusable lookup/conversion state instead of rebuilding data per call.
- Use `I18NManager.GetResource(...)` for user-visible strings in C# and keep resource keys near their feature area.
- Use service resolution through `App.AppViewModel.AppServiceProvider.GetRequiredService<T>()` when nearby code does so, rather than new global singletons.
- In disposable types, call `GC.SuppressFinalize(this)` and dispose owned token sources/streams; avoid adding finalizers unless necessary.
- Prefer event hooks for task lifecycle extension points (`Started`, `Stopped`, `Error`, `After...`) when matching download/task code.

## C# Naming

- Public/internal types, properties, events, methods, visible fields, and constants use PascalCase.
- Interfaces start with `I`.
- Private instance fields use `_camelCase`; private static and static readonly fields use `_PascalCase`.
- Required DTO/model properties commonly use `required ... { get; init; }`.
- Avalonia styled/direct properties follow the usual `NameProperty` field plus `Name` CLR wrapper pattern.
- View models commonly use `partial` classes with CommunityToolkit attributes like `[ObservableProperty]` and `[RelayCommand(CanExecute = nameof(...))]`.
- Split command-heavy view models into `.Commands.cs` partial files when the type already has that pattern.
- Source-generated JSON contexts use partial `JsonSerializerContext` classes with `[JsonSerializable]` attributes near related DTOs.
- Factory-style view models usually implement `IFactory<T, TSelf>` with `public static TSelf CreateInstance(T input) => new(input);`.
- Extension helpers may use C# extension blocks when the surrounding file uses them; keep them grouped by receiver type.

## XAML/Avalonia

- Use typed XAML wherever possible:
  - `x:DataType="{x:Type vm:SomeViewModel}"`
  - `TargetType="{x:Type Button}"`
  - `x:Key="{x:Type controls:SomeControl}"`
  - `BasedOn="{StaticResource {x:Type ListBox}}"`
- Avoid bare type strings such as `TargetType="Button"` unless the existing file specifically uses that older style.
- Use one attribute per line for multi-attribute elements. Follow `Settings.XamlStyler` ordering: class/xmlns first, then `x:*`/key/name/title, layout attached props, size, margin/padding/alignment, then remaining props.
- Use self-closing tags for empty elements and keep a space before `/>`.
- Prefer `StaticResource` for stable styles/templates and `DynamicResource` for theme-dependent brushes/colors.
- Prefer compiled/typed bindings with `x:DataType`; use `$parent[...]`, `#Name`, `TemplateBinding`, and `x:Static` in the established Avalonia style.
- Use converters and markup extensions already present in `Views.Converters`, `Views.Markup`, `Utilities`, and I18N resources rather than ad hoc code-behind formatting.
- For control themes/templates, use `ControlTheme`, `ControlTemplate`, named parts like `PART_ContentPresenter`, and `TemplateBinding` for templated properties.
- Keep view text localized through existing I18N resources when the surrounding UI is localized.
- Prefer `Command` bindings for user actions. Keep code-behind for view-only event glue, static helpers such as copy/open-browser handlers, and interactions that need the control instance.
- Prefer Fluent icon markup already used in the project, such as `markup:SymbolIcon` or `fluent:SymbolIcon`, instead of new icon mechanisms.
- Reuse theme resources such as `ControlCornerRadius`, `TextFillColorSecondaryBrush`, `SystemAccentColor`, and existing text block themes. Avoid hard-coded colors outside resource dictionaries.
- Add `Design.PreviewWith` for reusable controls/templates when nearby controls provide previews.
- Keep repeated item UI in `DataTemplate` and repeated panels in `ItemsPanelTemplate`; use typed templates when the item type is known.
- For image loading from URLs/cacheable sources, prefer existing attached properties such as `controls:Source.Cache` over setting `Image.Source` directly.
- Use Avalonia boolean binding idioms already present in the project, e.g. `IsVisible="{Binding !!Items.Count}"` and negated bindings such as `{Binding !IsFollowed}`.
- Prefer `TextTrimming`, `MaxLines`, `TextWrapping`, and resource text themes over custom measurement code for text layout.
- Keep page-level XAML as the primary layout surface. Use code-behind for `TopLevel`, `StorageProvider`, clipboard, navigation, and event handlers that need sender/control context.
- For command bars, prefer `CommandBarButton` with Fluent `Icon` and localized `Label`; use secondary commands for less common actions.
- Use `d:*` design attributes and `mc:Ignorable="d"` in pages/controls that already maintain design-time metadata.
- Register Avalonia `StyledProperty` for standard bindable control state, `DirectProperty` when storing/validating through backing fields and `SetAndRaise`.

## Feature Patterns

- Settings UI should use the existing AutoSettingsPage builder DSL, `SettingsEntryAttribute`, `LocalSettingsEntryHelper`, `SymbolComboBoxItem`, and custom `IEntryControl` implementations instead of hand-built settings pages.
- Search/form view models expose `TryValidate(out title, out content)` and `BuildArguments(...)`; validation messages come from I18N resources.
- Download/task state uses `DownloadState` switch expressions for labels, icons, brushes, and enabled state; keep transitions guarded by current state patterns.
- Generated code and source generators live in `Pixeval.SourceGen`; prefer attributes/source-gen conventions already present instead of reflection-heavy runtime wiring for repeated model boilerplate.

## Quick Examples

Prefer:

```csharp
if (TopLevel.GetTopLevel(this) is not { ViewContainer: { } viewContainer })
    return;

var state = count switch
{
    0 => Empty,
    > 0 => Ready,
    _ => Unknown
};
```

Avoid:

```csharp
if (count == 0)
{
    state = Empty;
}
else if (count > 0)
{
    state = Ready;
}
```

Prefer:

```xml
<ControlTheme x:Key="{x:Type controls:AvatarImage}" TargetType="{x:Type controls:AvatarImage}">
    <Setter Property="Template">
        <ControlTemplate TargetType="{x:Type controls:AvatarImage}">
            <Image Source="{TemplateBinding Source}" />
        </ControlTemplate>
    </Setter>
</ControlTheme>
```

## Before Finishing

- Re-read nearby `src` files and align with their local idioms.
- Do not copy style from `src/lib`.
- Run the smallest relevant format/build/test command available for the files touched.
