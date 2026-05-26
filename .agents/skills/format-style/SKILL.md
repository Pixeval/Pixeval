---
name: format-style
description: 工程代码格式化风格指引
---

# Pixeval 格式化风格

若有未详尽覆盖的情况，可参考任意 `src` 下的代码，但不包括 `src/lib` 里的第三方或子模块代码

## 基本要求

- 保留现有文件头。除非所在目录本身就有同类头部，否则不要给原本没有头部的文件新增头部。
- 视图/控件的 `.axaml` 和 `.axaml.cs` 文件要成对维护。
- 默认按现代 C# 假设：主项目目标框架是 `net10.0`，启用了 nullable，`LangVersion` 是 `latest`，并且默认启用编译绑定。

## C# 偏好

- 基础代码风格遵循 `.editorconfig` 中的设置。
- 使用 UTF-8、CRLF、末尾换行、4 空格缩进，不使用 tab。
- `using` 指令不要用空行拆分导入组。
- 使用 file-scoped namespace。
- 尽量使用 `var`，包括内置类型和一眼可见的类型。
- 使用 `nullable`，并在所有地方保证空安全，不要留下 `null` 警告。对于临时为 `null` 的瞬态，可以用 `null!`。
- 优先使用模式匹配：
  - 对常量优先使用 `is` 模式匹配而不是 `==` 或 `!=`：`count is 0`、`count is not 0`、`value is null`、`value is not null`、`value is not A and not B`。
  - 用 `obj is { } value` 绑定非空值。
  - 使用属性/列表模式，例如 `entry is { Width: > 0, Height: > 0 }`。
  - 对形状不匹配的情况，用 `is not { ... }` 做守卫。
- 分支语句优先级： `switch` 表达式 > `switch` 语句 > `if`/`else if` 链，尤其适合枚举、类 union 的状态、命令和类型分发。
- 当成员能在一行内清楚表达时，优先使用表达式主体：属性、访问器、简单方法、局部函数、转换器和运算符都适用。
- 优先使用对象/集合初始化器、集合表达式（`[]`、`[.. items]`）、空条件运算、`??`、`throw` 表达式、范围/索引运算符、元组命名，以及清晰时的匿名成员推断。
- `if-else` 中若主体是一行以内，则可以省略大括号；若分支中有跳转语句（如`return`、`break`、`continue` 等）则优先写在 `if` 中，并且省略 `else` 来减少嵌套层级。
- 强制转换后面保留一个空格：`(Type) value`。
- 二元运算符两侧保持空格，换行时把运算符放在下一行开头。
- 简单场景优先使用 `using var` 或 `await using var`，不要嵌套多层 `using` 块。
- `using var` 或 `await using var` 若有初始化列表则拆开，避免初始化时发生异常导致资源未正确释放。例如：`using var a = new A { X = 1 };` 应该改为 `using var a = new A(); a.X = 1;`。
- 当有意忽略一个非 `void` 返回值时，使用弃元（`_ = ...;`），表达式主体的方法除外。
- 对“理论上穷尽”的 `switch`，使用 `ArgumentOutOfRangeException(nameof(value))` 或等价的默认分支。
- 如果所有构造函数都调用某一个构造函数，或只有一个构造函数，优先把这个构造函数改为主构造函数。
- 对于返回 `Task` 或 `Task<T>` 的方法，若只有返回时用到 `Task`，则不需要将方法声明为异步（`async`）方法，而是直接返回 `Task`，但方法名仍然需要 `Async` 后缀。
- 在非 UI 层的异步方法中，最后一个参数使用 `CancellationToken token = default`，并在其中所有 `await` 的 `Task` 使用 `ConfigureAwait(false)` 避免切换线程上下文。但在 UI 代码中，不要这样做以避免死锁。
- 对于变量名引用，使用 `nameof(...)` 而不是字符串字面量，包括字符串内插中。
- 对可复用的查找/转换状态，使用 `Lazy<T>`、`FrozenDictionary`、生成正则，或者静态只读转换器实例，不要每次调用都重建数据。
- 对 `IDisposable`，若需要则调用 `GC.SuppressFinalize(this)`。
- 如果可以，则将字段/属性/结构体/方法/局部方法设为 `static`/`readonly`/`const`/`getonly`/`initonly` 等以增强约束。
- 不要使用旧的扩展方法，而使用 C# 14 中新的 extension blocks，并将同一类型的扩展聚在一起。
- 尽量使用 `stackalloc`、`Span<T>`、`Memory<T>` 等来增强性能。
- 抛出异常时提供要意义的消息、参数名称。
- 编写 XML 注释时：
  - 对于继承自接口或基类的成员，优先使用 `<inheritdoc />` 来继承文档。
  - 对于调用其他方法所以文档可以直接复用的成员，优先使用 `<inheritdoc cref="..." />` 来复用文档。
  - 对于引用的类型、成员，使用 `<see cref="..." />`，并且不要使用全限定名，而是也用 `using` 引入。
  - 对于成员引用或泛型参数引用，`<paramref name="..." />`、`<typeparamref name="..." />` 来提供链接。
  - 对于其中的关键字，使用 `<see langword="..." />` 来增强高亮。
- 接口中的泛型若可以用协变/逆变，则使用。
- 对于返回值，若无必要（如迭代器方法则除外），则优先使用 `IReadOnlyCollection<T>` 或更严格的 `IReadOnlyList<T>` 而不是 `IEnumerable<T>`，以表达集合内元素有限。
- 优先使用 `IReadOnlyList<T>`或`ReadOnlySpan<T>` 而不是 `T[]` 来表示数组，以避免潜在的数组协变。
- 若一个类内容太多，可以拆成形如 `.Commands.cs` 的 partial 文件。
- 使用 Linq 时，优先使用方法链式调用而不是查询表达式。
- 命名时：
  - 对非抛出式尝试使用 `Try*` 命名（例如 `TryReset`、`TryDelete`）。
  - 类型、属性、事件、方法、可见字段和常量使用 PascalCase。
  - 接口以 `I` 开头。
  - 私有实例字段使用 `_camelCase`；私有静态字段和 `static readonly` 字段使用 `_PascalCase`。

## Avalonia XAML

- 基础代码风格遵循 `Settings.XamlStyler` 中的设置。
- 尽量使用有类型信息的 XAML：
  - `x:DataType="{x:Type vm:SomeViewModel}"`
  - `TargetType="{x:Type Button}"`
  - `x:Key="{x:Type controls:SomeControl}"`
  - `BasedOn="{StaticResource {x:Type ListBox}}"`
- 对于 XAML 内的命名空间引用，使用 `xmlns:prefix="using:..."` 而不是 `xmlns:prefix="clr-namespace:...;assembly=..."`。对于根元素类所在的命名空间，使用 `local` 作为命名空间 `xmlns:local="using:..."`。
- 空元素使用自闭合标签。
- 稳定的样式/模板优先用 `StaticResource`，和主题相关的画刷/颜色优先用 `DynamicResource`。
- 优先使用编译/有类型绑定配合 `x:DataType`。
- 对控件主题/模板，使用 `ControlTheme`、`ControlTemplate`、诸如 `PART_ContentPresenter` 的命名部件，以及 `TemplateBinding` 处理模板属性。
- 优先使用标记扩展以减少代码行数，例如使用 `{markup:SymbolIcon ...}` 而不是 `<fluent:SymbolIcon Symbol="..." />`
- 文本布局优先使用 `TextTrimming`、`MaxLines`、`TextWrapping` 和资源文本主题，而不是自己写测量逻辑。
- 标准可绑定控件状态使用 Avalonia `StyledProperty`，非样式属性或用户控件中定义的需要绑定的属性使用 `DirectProperty` 和 `SetAndRaise`。

- 绑定风格：
  - 优先使用 Avalonia 语法，如 `$parent[...]`、`#Name`等。
  - 优先使用 Avalonia 语法而不是转换器，例如 `IsVisible="{Binding !!Items.Count}"`、`{Binding !IsFollowed}`。

### 简单示例

```xml
<ControlTheme x:Key="{x:Type controls:AvatarImage}" TargetType="{x:Type controls:AvatarImage}">
    <Setter Property="Template">
        <ControlTemplate TargetType="{x:Type controls:AvatarImage}">
            <Image Source="{TemplateBinding Source}" />
        </ControlTemplate>
    </Setter>
</ControlTheme>
```
