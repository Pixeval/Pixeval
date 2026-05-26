---
name: code-style
description: 工程代码编写风格指引（不含简单的格式化风格）
---

# Pixeval 代码风格

- 在重构代码时，不要被既有模式束缚住，直接重构为更合理、更高效的实现；不要作打补丁式的临时修改，并且不要留下兼容代码或过渡方案，避免新旧逻辑混杂。
- 在关键处留下注释以表达意图。
- 优先复用现有包和辅助项目、工具类，避免重复造轮子，优先复用 `Pixeval.Utilities` 或本项目里已经有的辅助函数和扩展，例如 `SelectNotNull`、`PickMax`/`PickClosest`、`FileHelper`、`MakoHelper` 和缓存辅助方法。
- 保持代码可 AOT 编译，避免使用反射、动态代码，但是若可以用 `[DynamicallyAccessedMembers]` 避免反射失败则可以使用。
- 对于相近功能的模型，通过继承、接口、组合等方式复用代码，避免重复代码。
- 可观测属性优先使用 CommunityToolkit 的 `[ObservableProperty]` 生成器，并附在部分属性上而不是字段上来生成。
- 对于难以处理的异常，可以使用 `App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()` 记录日志，但若频繁抛出的异常（如 `OperationCanceledException`、`TaskCanceledException`等）则不记录。

## 文件夹结构

- `AppManagement`：应用状态和生命周期管理。
- `Controls`：模板控件或控件样式，模板控件继承 `TemplatedControl`，成对的文件命名也使用 `.axaml` 和 `.axaml.cs`；资源字典要在 `Themes/Controls.axaml` 里引用。
- `Utilities`：辅助函数和扩展方法。
- `Views`：普通的用户控件，包括页面等。
- `ViewModels`：视图模型。
- `Models`：数据模型。
- `i18n`：国际化字符串资源。

## 其他

## Avalonia XAML

- 打开浏览器、复制文本使用 `AvaloniaHelper` 里封装好的静态属性，参数通过 `Tag` 属性指定。
- 通过现有 `{I18N ...}` 标记扩展指定本地化字符串。
- 复用主题资源，例如 `ControlCornerRadius`、`TextFillColorSecondaryBrush`、`SystemAccentColor` 等。不要在资源字典外硬编码颜色。
- 对来自 URL 或可缓存源的图片加载，优先使用已有附加属性，例如 `controls:Source.Cache`，而不是直接设置 `Image.Source`。
- 优先复用现有的 `Views.Converters`、`Views.Markup`、`Utilities` 里已有的转换器和标记扩展，不要临时在 code-behind 里手写格式化。

## 国际化字符串管理

- 主项目 UI 中禁止出现硬编码的字符串（除非不需要翻译的名字、专有名词），所有用户可见的字符串都必须通过资源管理。
- 字符串资源在 `i18n` 文件夹里以语言划分，包含 JSON 文件和 Markdown 文件两种形式。
- 编辑完字符串资源后，运行 `i18n/Language.tt` 来生成对应的 `Language.cs`，不要手动编辑这个文件。
- C# 文件中，使用 `I18NManager.GetResource(XXXResources.XXX)` 来获取字符串。禁止直接使用常量字符串： `I18NManager.GetResource("XXXResources.XXX")`。
- XAML 文件中，使用 `{I18N {x:Static xxx:XXXResources.XXX}}` 标记扩展来获取字符串。禁止直接使用常量字符串： `{I18N XXXResources.XXX}`。
- 对于自定义枚举，若要给每个枚举值指定一个对应的字符串资源（来给ComboBox等控件使用），则遵循以下步骤：
  - 先在 `i18n/xx-XX/Enum.json` 中定义枚举值对应的字符串资源，并生成 `Language.cs`。
  - 若是项目内定义的枚举，在枚举上提供 `[LocalizationMetadata]`，并在每个枚举值上提供 `[LocalizedResource(EnumResources.XXX)]` 来指定资源键，也可以通过 `[LocalizedResource(Resource = "XXX")]` 直接指定本地化字符串的值，在使用不需要翻译的专有英文名词时会用到（如 `"http"`）。
  - 若是外部库的枚举，则在 `Models/Settings/LocalSettingsEntryHelper.cs` 的静态构造函数里指定映射，参考已有示例即可。

## 添加设置项

- 设置页应优先使用现有的设置生成器，而不是直接手写。大致流程是：
  - 先在 `AppManagement/AppSettings.cs` 中添加一个新的可读可写属性，若有必要可指定默认值。
  - 在 `i18n/xx-XX/AppSettings.json` 中添加这个设置项的名称和描述资源，并生成 `Language.cs`。
  - 给这个属性添加一个 `[SettingsEntry]`，指定设置图标（`Icon`）、标题（`Header`）资源和描述（`Description`）资源，还可以指定占位符（`Placeholder`）资源。
  - 在 `ViewModels/SettingsPageViewModel.cs` 的 `LocalGroups` 属性中选择合适的设置组，注册设置项；若有必要可以指定 `ValueChanged` 回调或更多复杂的逻辑。
  - 若要实现特殊的设置项控件，需要在 `Views/Settings` 下实现一个 `XXXSettingsCard` 或 `XXXSettingsExpander`，继承于 `IEntryControl<XXXSettingsEntry>`；并在 `Models/Settings/LocalSettingsEntryHelper.cs` 静态构造函数里注册设置项对应的控件类型。
  - 如果要实现出了基元和数组类型外的设置项，需要在 `Models\Settings\Entries` 下实现一个 `XXXSettingsEntry` 设置类，再注册对应的设置控件。

## 完成前

- 对当前改动的文件，运行最小且相关的格式化 / 构建 / 测试命令。
