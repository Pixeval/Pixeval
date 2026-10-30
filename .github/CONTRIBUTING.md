# 贡献指南

## 参与开发的要求

1. 对Avalonia的基本了解，要了解更多相关信息请看 [Avalonia文档](https://docs.avaloniaui.net/)（或拥有WPF/UWP/WinUI 3相关经验）
2. 对C#和.NET开发的一定了解以及开发经验
3. 具有不依赖文档阅读代码的能力

## 环境要求

* 拥有[Git](https://git-scm.com)环境
* 1. 安装[Visual Studio 2026](https://visualstudio.microsoft.com/vs)或更高，并在**工具-获取工具与功能**的**工作负载**中选择 **.NET 桌面开发**
  1. 或最新版[JetBrains Rider](https://www.jetbrains.com/rider/)，并安装[.NET10 SDK](https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0)。

## 运行项目

```sh
git clone --recurse-submodules https://github.com/Pixeval/Pixeval.git
```

1. 用以上Git命令克隆本项目
2. 如果 *Pixeval.Desktop* 不是启动项目，请将
3. 其设置为启动项目
4. 构建并运行

### Material 3 主题依赖

Material3Expressive.Avalonia 当前通过 `src/lib` 下固定提交的子模块接入，
ComposeAnimation、MaterialShapes 和 MaterialSkiaShadow 随其源码提供。
MaterialColorUtilities、MaterialSymbols 也使用源码子模块；`Directory.Build.targets`
将主库的三个 `Shirasagi0012.*` 包引用替换为项目引用，并统一 Avalonia/SkiaSharp 版本。
因此构建 Pixeval 不需要 GitHub Packages 凭证，也不需要修改依赖仓库的项目文件。

现有克隆更新依赖后运行：

```sh
git submodule sync --recursive
git submodule update --init --recursive
dotnet build src/Pixeval.Desktop/Pixeval.Desktop.csproj -c Debug
dotnet test --project src/Pixeval.Tests/Pixeval.Tests.csproj
```

SmoothScroll 使用 Shirasagi0012 的分支，提供 Material 主库编译所需的滚动参与者接口。
普通 ScrollViewer 使用 Material 主题自带的平滑滚动模板，图片查看器继续使用 ScrollView。
不要同时添加 SmoothScroll NuGet 包或再次加载 `ScrollViewerSmoothTheme`。
升级子模块时需验证滚动、缩放和控件卸载，不能只依据编译通过判断手势行为。

主题入口为 `Themes/PixevalTheme.axaml`。Fluent 为尚未实现的控件提供模板，
基础控件采用 Material；Pixeval 的标签窗口、导航、ContentDialog 和图片手势保留现有实现。
标签输入框内部的编辑器局部保留 Fluent 以维持紧凑布局。
Roboto 为可选模块，当前沿用 Pixeval 字体设置和中文字体回退；Material Symbols 仅用于库内置图标。

Material 模板仍包含上游的反射绑定，NativeAOT 发布会报告 IL2026/IL3050。
不要直接屏蔽告警或保留整个程序集来掩盖问题；发布成功和 headless 测试通过不等同于
原生界面交互验收，尤其需检查文本剪贴板菜单、弹出菜单及目标平台的输入行为。

## 项目结构

### src目录

本目录包含和 *Pixeval* 强相关的项目，但耦合度较低的模块被拆分出独立的项目

1. *Pixeval* 项目包含了项目本身的逻辑及布局代码
2. *Pixeval.Desktop* 是桌面平台的打包项目，包括Windows/MacOS/Linux等平台的打包配置
3. *Pixeval.Android* 是Android平台的打包项目
4. *Pixeval.iOS* 是iOS平台的打包项目
5. *Pixeval.I18N* 是项目多语言字符串的功能模块，但不包含字符串资源（字符串资源在*Pixeval*中）
6. *Pixeval.Utilities* 是公用的工具类库
7. *Pixeval.SourceGen* 是 *Pixeval* 的源生成器项目
8. *Pixeval.Filters* 是Pixeval的过滤语句解析器模块
9. *Pixeval.Download* 是Pixeval的下载功能模块
10. *Pixeval.Caching* 是Pixeval的文件缓存模块

### lib目录

本目录大多是控件库或功能库，为了方便随时修改以子模块或直接粘贴形式保存。**一定不会引用src目录的项目**

1. *Mako*/*Mako.SourceGen* Pixeval使用的PixivAPI库
2. *Imouto.BooruParser* Pixeval使用booru系列API库
3. *Tabalonia* 参考 *[Tabalonia](https://github.com/egorozh/Tabalonia)* 修改而成的标签页控件库
4. *AutoSettingsPage*/*AutoSettingsPage.Avalonia* 自动生成设置页的控件库
5. *AnimatedControls.Avalonia* Avalonia的动图控件库

## 项目版本控制

本项目采用一个简单的Git分支模型：当您在进行开发的时候，请基于[`main`](https://github.com/Pixeval/Pixeval/tree/main)创建新的分支，新的分支格式应该遵循`{user}/{qualifier}/{desc}`，其中`{user}`是您的用户名。

| 代码内容 | qualifier | desc |
| - | - | - |
| 漏洞修复 | fix | 漏洞的简要叙述 |
| 新功能 | feature | 新特性的简要叙述 |
| 重构或者代码质量提升 | refactor | 重构部分的简要叙述 |

如果您的贡献包含不止一种上面提到的类型，则应当遵循和您的贡献最为相关的一项，并在commit信息中提及其他类型上的贡献

在开发完成后，请发布 [Pull Request](https://github.com/Pixeval/Pixeval/pulls) 请求合并到`main`分支

## 证书

GPL-3.0 License，详情请参阅 [LICENSE](../LICENSE)
