<div align="center">
    <img align="center" src="https://s1.ax1x.com/2020/04/03/GUMZjS.png" alt="logo" width="200">
    <h1 align="center">Pixeval</h1>
    <p align="center">基于.NET 7 和 Windows UI 3的强大、快速、漂亮的Pixiv桌面程序</p>
    <p align="center">
        <img src="https://img.shields.io/github/stars/Pixeval/Pixeval?color=red&style=for-the-badge"/>
        <a href="mailto:decem0730@hotmail.com">
            <img src="https://img.shields.io/static/v1?label=contact%20me&message=hotmail&color=green&style=for-the-badge&logo=gmail&logoColor=white"/>
        </a>
        <a href="https://jq.qq.com/?_wv=1027&k=5hGmJbQ" target="_blank">
            <img src="https://img.shields.io/static/v1?label=chatting&message=qq&color=blue&style=for-the-badge&logo=tencentqq&logoColor=white"/>
        </a>
        <a href="https://github.com/Pixeval/Pixeval/blob/master/LICENSE" target="_blank">
            <img src="https://img.shields.io/github/license/Pixeval/Pixeval?style=for-the-badge&logo=gnu&logoColor=white"/>
        </a>
        <a href="https://github.com/Pixeval/Pixeval/issues/new/choose" target="_blank">
            <img src="https://img.shields.io/static/v1?label=feedback&message=issues&color=pink&style=for-the-badge&logo=Github&logoColor=white"/>
        </a>
        <a href="https://dotnet.microsoft.com/en-us/download/dotnet/6.0" target="_blank">
            <img src="https://img.shields.io/static/v1?label=runtime&message=.NET%207.0&color=yellow&style=for-the-badge&logo=.NET&logoColor=white"/>
        </a>
        <a href="https://dotnet.microsoft.com/en-us/download/dotnet/6.0" target="_blank">
            <img src="https://img.shields.io/badge/Platform-Windows10.0.19041-512BD4?&style=for-the-badge&logo=Windows&logoColor=white"/>
        </a>
    </p>
    </br>
</div>

🌏: [**简体中文**](https://github.com/Pixeval/Pixeval/blob/master/README.md)
, [English](https://github.com/Pixeval/Pixeval/blob/master/README.en.md)

---

**基于WinUI3的Pixeval已经正在开发中，而作为旧的WPF版本除严重问题以外不再进行大量维护，请适时切换到新版Pixeval。**

> 仅支持 Windows 10（版本 1809 - 内部版本 17763）及更高版本。
> 可以通过以下步骤查看。选择“开始”按钮 ，然后选择“设置”。在“设置”中，依次选择“系统”>“关于” Windows规格。

更多详细信息请前往 [项目主页](https://sora.ink/pixeval) 查看

**WinUI3版本提供了更好的UI，更好的项目结构以及更好的开发体验，如果你想要了解目前的开发进度，可以通过以下方法来下载并编译该项目**

## 环境要求

1. 安装[Visual Studio
   2022](https://visualstudio.microsoft.com/vs)
2. 在**工具-获取工具与功能**的**工作负载**中选择 **.NET 桌面开发**（建议在安装对话框的“安装详细信息”窗格中，选择位于列表底部的“Windows 应用 SDK C# 模板”，但不是必需的。可以参考[安装适用于 Windows 应用 SDK 的工具](https://learn.microsoft.com/windows/apps/windows-app-sdk/set-up-your-development-environment)）
3. 在**工具-获取工具与功能**的**单个组件**中选择 **.NET 7**，也可以在VS外部下载[.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)最新版本，但不推荐这么做

## 运行项目

1. 用Git克隆本项目
2. 如果 Pixeval 不是启动项目，请将其设置为启动项目
3. 构建并运行（如果失败可以尝试重新生成解决方案）

## 参与开发的要求

1. 对Windows XAML
   Framework的基本了解，要了解更多相关信息请看 [XAML概述](https://docs.microsoft.com/windows/uwp/xaml-platform/xaml-overview)
2. 对C#和.NET开发的一定了解以及开发经验
3. 具有不依赖文档阅读代码的能力

## 项目版本控制

本项目采用一个简单的Git分支模型：当您在进行开发的时候，请基于`main`创建新的分支，新的分支格式**必须**遵循`{user}/{qualifier}/{desc}`，其中`{user}`是您的用户名。

| 代码内容 | qualifier | desc |
| - | - | - |
| BUG修复 | fix | BUG的简要叙述 |
| 新功能 | feature | 新特性的简要叙述 |
| 重构或者代码质量提升 | refactor | 重构部分的简要叙述 |

如果您的贡献包含不止一种上面提到的类型，则应当遵循和您的贡献最为相关的一项，并在commit信息中提及其他类型上的贡献

在开发完成后，请发布 [Pull Request](https://github.com/Pixeval/Pixeval/pulls) 请求合并到`main`分支

## 项目结构

1. Pixeval 项目包含了项目本身的逻辑及布局代码
2. Pixeval.CoreApi 包含了项目需要的Pixiv API
3. Pixeval.Utilities 包含了通用的工具代码

## 如果遇到任何问题（按照推荐程度优先级排序）

1. 在 [github](https://github.com/Pixeval/Pixeval/issues/new) 提交新的Issue
2. 给 [decem0730@hotmail.com](mailto:decem0730@hotmail.com) 发送邮件
3. 加入QQ群815791942来面对面的和开发者反馈问题

## 鸣谢（排名不分先后）

* [sovetskyfish](https://github.com/sovetskyfish)
* [Notsfsssf](https://github.com/Notsfsssf)
* [ControlNet](https://github.com/ControlNet)
* [wulunshijian](https://github.com/wulunshijian)
* [duiweiya](https://github.com/duiweiya)
* [LasmGratel](https://github.com/LasmGratel)
* [TheRealKamisama](https://github.com/TheRealKamisama)
* [Summpot](https://github.com/Summpot)
* [Poker](https://github.com/Poker-sang)

## 支持作者

如果你感觉该项目帮助到了你，欢迎前往[爱发电](https://afdian.net/@dylech30th)赞助我，你的支持是我维护项目的动力，谢谢！

## JetBrains开源许可

本项目重度依赖于JetBrains™ ReSharper，感谢JetBrains s.r.o为本项目提供[开源许可证](https://www.jetbrains.com/community/opensource/#support)，如果你同样对开发充满热情并且经常使用JetBrains s.r.o的产品，你也可以尝试通过JetBrains官方渠道[申请](https://www.jetbrains.com/shop/eform/opensource)开源许可证以供核心开发者使用

<figure style="width: min-content">
    <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/ReSharper_icon.png" width="200" height="200"/>
    <figcaption>Copyright © 2021 JetBrains s.r.o. </br>ReSharper and the ReSharper logo are registered trademarks of JetBrains s.r.o.</figcaption>
</figure>
