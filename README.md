<div align="center">
    <img align="center" src="https://s1.ax1x.com/2020/04/03/GUMZjS.png" alt="logo" width="200">
    <h1 align="center">Pixeval</h1>
    <p align="center">基于.NET 6 和 Windows UI 3的强大、快速、漂亮的Pixiv桌面程序</p>
    <p align="center">
        <img src="https://img.shields.io/github/stars/Pixeval/Pixeval?color=red&style=flat-square">
        <a href="mailto:decem0730@hotmail.com">
            <img src="https://img.shields.io/static/v1?label=contact%20me&message=hotmail&color=green&style=flat-square">
        </a>
        <a href="https://jq.qq.com/?_wv=1027&k=5hGmJbQ" target="_blank">
            <img src="https://img.shields.io/static/v1?label=chatting&message=qq&color=blue&style=flat-square">
        </a>
        <a href="https://github.com/Pixeval/Pixeval/blob/master/LICENSE" target="_blank">
            <img src="https://img.shields.io/github/license/Pixeval/Pixeval?style=flat-square">
        </a>
        <a href="https://github.com/Pixeval/Pixeval/issues/new/choose" target="_blank">
            <img src="https://img.shields.io/static/v1?label=feedback&message=issues&color=pink&style=flat-square">
        </a>
        <a href="https://dotnet.microsoft.com/en-us/download/dotnet/6.0" target="_blank">
            <img src="https://img.shields.io/static/v1?label=runtime&message=.NET%206.0&color=yellow&style=flat-square">
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
## 准备
1. 带有WinUI 3工作负载的Visual Studio
   2022，你可以在 [创建你的第一个WinUI 3 app](https://docs.microsoft.com/zh-cn/windows/apps/winui/winui3/create-your-first-winui3-app?tabs=desktop-csharp) 的"先决条件"部分查看更多信息
2. 安装VS插件[Single-project MSIX Packaging Tools for VS 20200](https://aka.ms/windowsappsdk/stable-vsix-2022-cs)
3. 安装.NET 6，你可以在 [这里](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) 下载并安装.NET 6

## 如果你想要参与开发，则需要如下几点额外需求

4. 对Windows XAML
   Framework的基本了解，要了解更多相关信息请看 [XAML概述](https://docs.microsoft.com/zh-cn/windows/uwp/xaml-platform/xaml-overview)
5. 对C# .NET开发的一定了解以及开发经验
6. 具有不依赖文档阅读代码的能力

## 开发 
1. 克隆本项目
2. 将 Pixeval 设置为启动项目
3. 打开`Build` | `ConfigurationManager`，勾选Pixeval一项后面的`Deploy`单选框
4. 构建并运行

## 项目版本控制须知
本项目采用一个简单的Git分支模型：当您在进行开发的时候，请基于`dev/main`创建新的分支，**切勿**直接基于`master`或者`dev/main`分支进行开发，新的分支格式**必须**遵循`dev/{user}/{qualifier}-{name}`，`{user}`**必须**是您的用户名。

1. 如果新的代码包含的是*BUG修复*，则`{qualifier}`**必须**为`fix`，`{name}`**应当**为BUG的简要叙述
2. 如果新的代码包含的是*新功能*，则`{qualifier}`**必须**为`feat`或者`feature`，`{name}`**应当**为新特性的简要叙述
3. 如果新的代码是*重构或者代码质量提升*，则`{qualifier}`**必须**为`refactor`，`{name}`**应当**为重构部分的简要叙述
4. 如果您的贡献包含不止一种上面提到的类型，则应当遵循和您的贡献最为相关的一项，并在commit信息中提及其他类型上的贡献
5. `master`分支**必须**当且仅当在新版本将要被发布的时候更新

在开发完成后，请在[这里](https://github.com/Pixeval/Pixeval/pulls)发布Pull Request请求合并到`dev/main`分支

## 项目结构

1. Pixeval 项目包含了项目本身的逻辑，布局代码，以及打包相关的文件
2. Pixeval.CoreApi 包含了项目需要的Pixiv API
3. Pixeval.LoginProxy 包含了Pixiv登录以及IPC相关的代码
4. Pixeval.SourceGen 包含了从本地化文件自动生成对应C#类的代码
5. Pixeval.Utilities 包含了通用的工具代码

## 如果遇到任何问题(按照推荐程度优先级排序)

1. 在 [github](https://github.com/Pixeval/Pixeval/issues/new) 提交新的Issue
2. 给 [decem0730@hotmail.com](mailto:decem0730@hotmail.com) 发送邮件
3. 加入QQ群815791942来面对面的和开发者反馈问题

## 鸣谢：(排名不分先后)

* [@sovetskyfish](https://github.com/sovetskyfish)
* [@Notsfsssf](https://github.com/Notsfsssf)
* [@ControlNet](https://github.com/ControlNet)
* [@wulunshijian](https://github.com/wulunshijian)
* [@duiweiya](https://github.com/duiweiya)
* [@Lasm_Gratel](https://github.com/LasmGratel)
* [@TheRealKamisama](https://github.com/TheRealKamisama)
* [@Summpot](https://github.com/Summpot)
* [@Poker](https://github.com/Poker-sang)

## 支持作者:

如果你感觉该项目帮助到了你，欢迎前往[爱发电](https://afdian.net/@dylech30th)赞助我，你的支持是我维护项目的动力，谢谢！

## JetBrains开源许可
本项目重度依赖于JetBrains™ ReSharper，感谢JetBrains s.r.o为本项目提供[开源许可证](https://www.jetbrains.com/community/opensource/#support)，如果你同样对开发充满热情并且经常使用JetBrains s.r.o的产品，你也可以尝试通过JetBrains官方渠道[申请](https://www.jetbrains.com/shop/eform/opensource)开源许可证以供核心开发者使用


<figure style="width: min-content">
    <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/ReSharper_icon.png" width="200" height="200">
    <figcaption>Copyright © 2021 JetBrains s.r.o. </br>ReSharper and the ReSharper logo are registered trademarks of JetBrains s.r.o.</figcaption>
</figure>
