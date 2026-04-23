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
