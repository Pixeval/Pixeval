<div align="center">

<img src="../src/Pixeval/Assets/Images/logo.svg" alt="logo" width="200">

# Pixeval

基于.NET 8 和 WinUI 3 的强大、快速、漂亮的Pixiv第三方桌面程序

[<img src="https://get.microsoft.com/images/zh-cn%20dark.svg" width="200"/>](https://apps.microsoft.com/detail/Pixeval/9p1rzl9z8454?launch=true&mode=mini)

![](https://img.shields.io/github/stars/Pixeval/Pixeval?color=red&style=for-the-badge&logo=data:image/svg+xml;charset=utf-8;base64,PHN2ZyB3aWR0aD0iNDgiIGhlaWdodD0iNDgiIHZpZXdCb3g9IjAgMCA0OCA0OCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHBhdGggZD0iTTIxLjgwMyA2LjA4NTQ0QzIyLjcwMTcgNC4yNjQ0OSAyNS4yOTgzIDQuMjY0NDggMjYuMTk3IDYuMDg1NDRMMzEuMDQ5MyAxNS45MTc0TDQxLjg5OTYgMTcuNDk0QzQzLjkwOTEgMTcuNzg2IDQ0LjcxMTUgMjAuMjU1NiA0My4yNTc0IDIxLjY3M0wzNS40MDYxIDI5LjMyNjFMMzcuMjU5NSA0MC4xMzI1QzM3LjYwMjggNDIuMTMzOSAzNS41MDIxIDQzLjY2MDIgMzMuNzA0NyA0Mi43MTUyTDI0IDM3LjYxMzJMMTQuMjk1MiA0Mi43MTUyQzEyLjQ5NzggNDMuNjYwMiAxMC4zOTcxIDQyLjEzMzkgMTAuNzQwNCA0MC4xMzI1TDEyLjU5MzggMjkuMzI2MUw0Ljc0MjU1IDIxLjY3M0MzLjI4ODQzIDIwLjI1NTYgNC4wOTA4MyAxNy43ODYgNi4xMDAzNyAxNy40OTRMMTYuOTUwNiAxNS45MTc0TDIxLjgwMyA2LjA4NTQ0WiIgZmlsbD0iI2ZmZmZmZiIvPgo8L3N2Zz4K)
![](https://img.shields.io/static/v1?label=contact%20me&message=hotmail&color=green&style=for-the-badge&logo=gmail&logoColor=white)
[![](https://img.shields.io/static/v1?label=chatting&message=qq&color=blue&style=for-the-badge&logo=qq&logoColor=white)](https://jq.qq.com/?_wv=1027&k=5hGmJbQ)
[![](https://img.shields.io/github/license/Pixeval/Pixeval?style=for-the-badge&logo=gnu&logoColor=white)](https://github.com/Pixeval/Pixeval/blob/main/LICENSE)
[![](https://img.shields.io/static/v1?label=feedback&message=issues&color=pink&style=for-the-badge&logo=Github&logoColor=white)](https://github.com/Pixeval/Pixeval/issues/new/choose)
[![](https://img.shields.io/static/v1?label=runtime&message=.NET%208.0&color=yellow&style=for-the-badge&logo=.NET&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/8.0)
![](https://img.shields.io/badge/Platform-Windows10.0.19041-512BD4?&style=for-the-badge&logo=data:image/svg+xml;charset=utf-8;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI0OCIgaGVpZ2h0PSI0OCI+CjxwYXRoIGQ9Ik00LjggMy44NGEuOTYuOTYgMCAwIDAtLjk2Ljk2djE4LjI0aDE5LjJWMy44NFptMjAuMTYgMHYxOS4yaDE5LjJWNC44YS45Ni45NiAwIDAgMC0uOTYtLjk2Wk0zLjg0IDI0Ljk2VjQzLjJjMCAuNTMuNDMuOTYuOTYuOTZoMTguMjR2LTE5LjJabTIxLjEyIDB2MTkuMkg0My4yYS45Ni45NiAwIDAgMCAuOTYtLjk2VjI0Ljk2Wm0wIDAiIGZpbGw9IiNmZmZmZmYiLz4KPC9zdmc+)

</div>

🌏: [**简体中文**](README.md)，[English](README.en.md)，[Русский](README.ru.md)，[Français](README.fr.md)

---

**基于WinUI3的Pixeval已经正在开发中，而作为旧的WPF版本除严重问题以外不再进行大量维护，请适时切换到新版Pixeval。**

> 仅支持 Windows 10（版本 2004 - 内部版本 19041）及更高版本。
> 可以通过以下步骤查看。右键点击“开始”按钮，选择然后选择系统；或者在“设置”中，依次选择“系统”>“系统信息”，此时页面中的Windows规格下可以看到相关信息。

更多详细信息请前往 [项目主页](https://sora.ink/pixeval) 查看

**WinUI3版本提供了更好的UI，更好的项目结构以及更好的开发体验，如果你想要了解目前的开发进度，可以通过以下方法来下载并编译该项目：**

## 环境要求

1. 拥有[git](https://git-scm.com)环境
2. 安装[Visual Studio 2022](https://visualstudio.microsoft.com/vs)（Roslyn 4.x要求必须是VS17.x，即VS2022）。
如果已安装请确认是VS2022的最新版本，因为负载里.NET SDK的版本和VS的版本有关，低版本可能不包含.NET8 SDK。
3. 在**工具-获取工具与功能**的**工作负载**中选择 **.NET 桌面开发**，并在安装对话框的“安装详细信息”窗格中，选择位于列表底部的“Windows 应用 SDK C# 模板”。（可以参考[安装适用于 Windows 应用 SDK 的工具](https://learn.microsoft.com/windows/apps/windows-app-sdk/set-up-your-development-environment)）

## 运行项目

1. 用Git克隆本项目
2. 如果 Pixeval 不是启动项目，请将其设置为启动项目
3. 构建并运行

* 如果失败可以尝试重新生成解决方案或者重启Visual Studio 2022

## 参与开发的要求

1. 对Windows XAML Framework的基本了解，要了解更多相关信息请看 [XAML概述](https://docs.microsoft.com/windows/uwp/xaml-platform/xaml-overview)
2. 对C#和.NET开发的一定了解以及开发经验
3. 具有不依赖文档阅读代码的能力

## 项目版本控制

本项目采用一个简单的Git分支模型：当您在进行开发的时候，请基于`main`创建新的分支，新的分支格式**必须**遵循`{user}/{qualifier}/{desc}`，其中`{user}`是您的用户名。

| 代码内容 | qualifier | desc |
| - | - | - |
| 漏洞修复 | fix | 漏洞的简要叙述 |
| 新功能 | feature | 新特性的简要叙述 |
| 重构或者代码质量提升 | refactor | 重构部分的简要叙述 |

如果您的贡献包含不止一种上面提到的类型，则应当遵循和您的贡献最为相关的一项，并在commit信息中提及其他类型上的贡献

在开发完成后，请发布 [Pull Request](https://github.com/Pixeval/Pixeval/pulls) 请求合并到`main`分支

## 项目结构

1. Pixeval 项目包含了项目本身的逻辑及布局代码
2. Pixeval.Controls 包含了一些耦合度较低的控件
3. Pixeval.CoreApi 包含了项目需要的Pixiv API
4. Pixeval.SourceGen 包含了设置相关的代码生成器
5. Pixeval.Utilities 包含了通用的工具代码

## 反馈问题（按照推荐程度优先级排序）

1. 在 [github](https://github.com/Pixeval/Pixeval/issues/new/choose) 提交新的Issue
2. 给 [decem0730@hotmail.com](mailto:decem0730@hotmail.com) 发送邮件
3. 加入QQ群815791942来面对面的和开发者反馈问题

## 鸣谢（排名不分先后）

[![Toolkit Contributors](https://contrib.rocks/image?repo=Pixeval/Pixeval)](https://github.com/Pixeval/Pixeval/graphs/contributors)

Made with [contrib.rocks](https://contrib.rocks).

## 支持作者

如果你感觉该项目帮助到了你，欢迎前往[爱发电](https://afdian.net/@dylech30th)赞助我，你的支持是我维护项目的动力，谢谢！

<div>
  <a href="https://www.jetbrains.com/?from=Pixeval" align="right"><img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg" alt="JetBrains" class="logo-footer" width="100" align="left"></a>
  <br/>
  
  本项目重度依赖于 [JetBrains](https://www.jetbrains.com/?from=ImageSharp) ReSharper，感谢JetBrains s.r.o为本项目提供 [开源许可证](https://www.jetbrains.com/community/opensource/#support)，如果你同样对开发充满热情并且经常使用JetBrains s.r.o的产品，你也可以尝试通过JetBrains官方渠道 [申请](https://www.jetbrains.com/shop/eform/opensource) 开源许可证以供核心开发者使用
</div>
