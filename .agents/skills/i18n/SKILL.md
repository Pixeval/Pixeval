---
name: i18n
description: Pixeval 国际化资源维护规范，涵盖用户可见字符串、JSON/Markdown 资源、XAML/C# 引用、BlockedContent 共享文案、非默认语言增删规则以及 Language.tt 生成验证。Use when adding, moving, removing, or changing localized text in Pixeval.
---

# Pixeval i18n

维护 Pixeval 的用户可见文本时，遵循资源、引用、生成和验证的完整流程。

## Rules

- 将所有需要翻译的用户可见文本放入 `src/Pixeval/i18n`，只有不需要翻译的专有名词等可以保留为字面量。
- 按语义选择资源文件，避免在页面资源中重复定义跨页面使用的文案。
- 优先复用已有资源键。移动文案时删除旧键，不保留页面资源中的重复键或兼容别名。
- 以 `zh-Hans` 作为默认语言和资源键来源。新增或迁移字符串时只在 `zh-Hans` 添加中文值。
- 不要为 `en-US`、`fr-FR`、`ru-RU` 或其他非默认语言新增字符串、补翻译或创建缺失的资源文件。迁移或删除资源时，非默认语言只删除对应的旧条目；保留无关的既有内容。
- 不要手动编辑生成的 `src/Pixeval/i18n/Language.cs`。

## References

在 C# 中通过生成的资源类获取文本：

```csharp
I18NManager.GetResource(BlockedContentResources.BlockTag)
```

在 XAML 中通过 `I18N` 标记扩展引用资源：

```xml
{I18N {x:Static pixeval:BlockedContentResources.BlockTag}}
```

不要把资源路径写成字符串，也不要使用未生成的裸资源名。

## Workflow

1. 使用 `rg` 搜索待修改的旧资源键、硬编码文本和所有语言文件中的对应条目。
2. 将通用文案放入默认语言下合适的共享资源文件，只修改 `zh-Hans` 的新增值，并在所有存在旧条目的语言文件中删除旧条目。
3. 更新 XAML 和 C# 引用，使用生成的 `*Resources` 常量。
4. 运行 T4 重新生成语言资源类：

   ```powershell
   t4 src/Pixeval/i18n/Language.tt
   ```

5. 校验所有修改过的 JSON 可以解析，并确认旧资源键和不应保留的硬编码文本不再被引用。运行最小相关验证：

   ```powershell
   dotnet build src/Pixeval/Pixeval.csproj --no-restore -v:minimal
   dotnet test src/Pixeval.Tests/Pixeval.Tests.csproj --no-restore -v:minimal
   ```

6. 检查 `git diff --check`，只保留本次 i18n 变更和必要的生成文件变更。
