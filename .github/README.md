<div align="center">

<img src="../src/Pixeval/Assets/Images/logo.svg" alt="logo" width="200">

# Pixeval

基于.NET 8 和 WinUI 3 的强大、快速、漂亮的Pixiv第三方桌面程序

[<img src="https://get.microsoft.com/images/zh-cn%20dark.svg" width="200"/>](https://apps.microsoft.com/detail/Pixeval/9p1rzl9z8454?launch=true&mode=mini)

![](https://img.shields.io/github/stars/Pixeval/Pixeval?color=red&style=for-the-badge&logo=data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAACXBIWXMAAA7EAAAOxAGVKw4bAAAF7GlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDUgNzkuMTYzNDk5LCAyMDE4LzA4LzEzLTE2OjQwOjIyICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1sbnM6cGhvdG9zaG9wPSJodHRwOi8vbnMuYWRvYmUuY29tL3Bob3Rvc2hvcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RFdnQ9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZUV2ZW50IyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ0MgMjAxOSAoV2luZG93cykiIHhtcDpDcmVhdGVEYXRlPSIyMDIzLTAyLTA1VDE1OjM4OjE5KzA4OjAwIiB4bXA6TW9kaWZ5RGF0ZT0iMjAyMy0wMi0wNVQxNTo0NToyOSswODowMCIgeG1wOk1ldGFkYXRhRGF0ZT0iMjAyMy0wMi0wNVQxNTo0NToyOSswODowMCIgZGM6Zm9ybWF0PSJpbWFnZS9wbmciIHBob3Rvc2hvcDpDb2xvck1vZGU9IjMiIHBob3Rvc2hvcDpJQ0NQcm9maWxlPSJzUkdCIElFQzYxOTY2LTIuMSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDo0NzZjNjhkYS0zNzFmLWYyNGItOTRkZi02ZmVkN2Q1NDM5OGUiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6Mzc0ODYyNDUtMjQ1OC03YjRmLTg4ZjQtMzQ3NDUzNWZhMDczIiB4bXBNTTpPcmlnaW5hbERvY3VtZW50SUQ9InhtcC5kaWQ6Mzc0ODYyNDUtMjQ1OC03YjRmLTg4ZjQtMzQ3NDUzNWZhMDczIj4gPHhtcE1NOkhpc3Rvcnk+IDxyZGY6U2VxPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iY3JlYXRlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDozNzQ4NjI0NS0yNDU4LTdiNGYtODhmNC0zNDc0NTM1ZmEwNzMiIHN0RXZ0OndoZW49IjIwMjMtMDItMDVUMTU6Mzg6MTkrMDg6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCBDQyAyMDE5IChXaW5kb3dzKSIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6NDc2YzY4ZGEtMzcxZi1mMjRiLTk0ZGYtNmZlZDdkNTQzOThlIiBzdEV2dDp3aGVuPSIyMDIzLTAyLTA1VDE1OjQ1OjI5KzA4OjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgMjAxOSAoV2luZG93cykiIHN0RXZ0OmNoYW5nZWQ9Ii8iLz4gPC9yZGY6U2VxPiA8L3htcE1NOkhpc3Rvcnk+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+sj4YggAAAQ5JREFUeNrt20kSwyAMRNH0/Q/d2acSO2CwNXwfAFlvAZIKZPvV+RMAzwF8BhYAjQB+BRUADQDOAgqAwgD/BhMABQFGAwmAQgCzQQRAAYCrAQRAYoBViwuAhACrFxYAiQB2qSoaQKSBonYAVJmY6gyg62hYcvO5OAAAsAe42o4/dDJ8OwbdJfmjOsAdkj8rhFw5cSrBgV7AVZMfaYZcMfnRbtDVkp9ph10p+dl5gKskPwsQGWF4KHJlIuTsyV8FiIQwPSsEAAD2AAAAAAAAACiFAQAAgCcAtGm98ADaCLt9IOIbfs7VAGa7NWcHWHWpyRkBVt/ochaA3Te8HRXg7idvjgLwyIvPVf/D4+nuAG8V/wSNyqWVwwAAAABJRU5ErkJggg==)
![](https://img.shields.io/static/v1?label=contact%20me&message=hotmail&color=green&style=for-the-badge&logo=gmail&logoColor=white)
[![](https://img.shields.io/static/v1?label=chatting&message=qq&color=blue&style=for-the-badge&logo=tencentqq&logoColor=white)](https://jq.qq.com/?_wv=1027&k=5hGmJbQ)
[![](https://img.shields.io/github/license/Pixeval/Pixeval?style=for-the-badge&logo=gnu&logoColor=white)](https://github.com/Pixeval/Pixeval/blob/main/LICENSE)
[![](https://img.shields.io/static/v1?label=feedback&message=issues&color=pink&style=for-the-badge&logo=Github&logoColor=white)](https://github.com/Pixeval/Pixeval/issues/new/choose)
[![](https://img.shields.io/static/v1?label=runtime&message=.NET%208.0&color=yellow&style=for-the-badge&logo=.NET&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/8.0)
![](https://img.shields.io/badge/Platform-Windows10.0.19041-512BD4?&style=for-the-badge&logo=Windows&logoColor=white)

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
