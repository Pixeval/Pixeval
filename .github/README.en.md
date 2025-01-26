<div align="center">

<img src="../src/Pixeval/Assets/Images/logo.svg" alt="logo" width="200">

# Pixeval

Powerful, fast and beautiful Pixiv third-party desktop program based on .NET 8 and WinUI 3

[<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>](https://apps.microsoft.com/detail/Pixeval/9p1rzl9z8454?launch=true&mode=mini)

![](https://img.shields.io/github/stars/Pixeval/Pixeval?color=red&style=for-the-badge&logo=data:image/svg+xml;charset=utf-8;base64,PHN2ZyB3aWR0aD0iNDgiIGhlaWdodD0iNDgiIHZpZXdCb3g9IjAgMCA0OCA0OCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHBhdGggZD0iTTIxLjgwMyA2LjA4NTQ0QzIyLjcwMTcgNC4yNjQ0OSAyNS4yOTgzIDQuMjY0NDggMjYuMTk3IDYuMDg1NDRMMzEuMDQ5MyAxNS45MTc0TDQxLjg5OTYgMTcuNDk0QzQzLjkwOTEgMTcuNzg2IDQ0LjcxMTUgMjAuMjU1NiA0My4yNTc0IDIxLjY3M0wzNS40MDYxIDI5LjMyNjFMMzcuMjU5NSA0MC4xMzI1QzM3LjYwMjggNDIuMTMzOSAzNS41MDIxIDQzLjY2MDIgMzMuNzA0NyA0Mi43MTUyTDI0IDM3LjYxMzJMMTQuMjk1MiA0Mi43MTUyQzEyLjQ5NzggNDMuNjYwMiAxMC4zOTcxIDQyLjEzMzkgMTAuNzQwNCA0MC4xMzI1TDEyLjU5MzggMjkuMzI2MUw0Ljc0MjU1IDIxLjY3M0MzLjI4ODQzIDIwLjI1NTYgNC4wOTA4MyAxNy43ODYgNi4xMDAzNyAxNy40OTRMMTYuOTUwNiAxNS45MTc0TDIxLjgwMyA2LjA4NTQ0WiIgZmlsbD0iI2ZmZmZmZiIvPgo8L3N2Zz4K)
![](https://img.shields.io/static/v1?label=contact%20me&message=hotmail&color=green&style=for-the-badge&logo=gmail&logoColor=white)
[![](https://img.shields.io/static/v1?label=chatting&message=qq&color=blue&style=for-the-badge&logo=qq&logoColor=white)](https://jq.qq.com/?_wv=1027&k=5hGmJbQ)
[![](https://img.shields.io/github/license/Pixeval/Pixeval?style=for-the-badge&logo=gnu&logoColor=white)](https://github.com/Pixeval/Pixeval/blob/main/LICENSE)
[![](https://img.shields.io/static/v1?label=feedback&message=issues&color=pink&style=for-the-badge&logo=Github&logoColor=white)](https://github.com/Pixeval/Pixeval/issues/new/choose)
[![](https://img.shields.io/static/v1?label=runtime&message=.NET%208.0&color=yellow&style=for-the-badge&logo=.NET&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/8.0)
![](https://img.shields.io/badge/Platform-Windows10.0.19041-512BD4?&style=for-the-badge&logo=data:image/svg+xml;charset=utf-8;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI0OCIgaGVpZ2h0PSI0OCI+CjxwYXRoIGQ9Ik00LjggMy44NGEuOTYuOTYgMCAwIDAtLjk2Ljk2djE4LjI0aDE5LjJWMy44NFptMjAuMTYgMHYxOS4yaDE5LjJWNC44YS45Ni45NiAwIDAgMC0uOTYtLjk2Wk0zLjg0IDI0Ljk2VjQzLjJjMCAuNTMuNDMuOTYuOTYuOTZoMTguMjR2LTE5LjJabTIxLjEyIDB2MTkuMkg0My4yYS45Ni45NiAwIDAgMCAuOTYtLjk2VjI0Ljk2Wm0wIDAiIGZpbGw9IiNmZmZmZmYiLz4KPC9zdmc+)

</div>

🌏: [简体中文](README.md), [**English**](README.en.md), [Русский](README.ru.md), [Français](README.fr.md)

---

**The Pixeval that based on WinUI 3 is now WIP, the older version, 
which is the WPF version has been deprecated and expecting no more supports from developers.**

> The WinUI 3 codebase for Pixeval supports only Windows 10 (2004, Build Number 19041) and higher.
> You can check this at Settings | System | About | Windows specifications

For more information, see [main page](https://sora.ink/pixeval/)

**The WinUI 3 version provides a better user interface,
a more structural codebase and a modern development experience compared to the WPF version.
You can download and compile it yourself if you want to take a glance at the new version,
follow the following steps to compile and run:**

## Prerequisites

1. Install [Visual Studio 2022](https://visualstudio.microsoft.com/vs) (Roslyn 4.x requires VS17.x, i.e. VS2022)
2. In **Tools - Get Tools and Features**, under **Workloads**, select .NET Desktop Development (In the Installation Details pane of the installation dialog, select the Windows App SDK C# Template at the bottom of the list, but it is not required.) You can refer to [Install Tools for Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/set-up-your-development-environment)
3. Select .NET 8 in **Tools - Get Tools and Features - Individual components**, or download the latest version of the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) outside of VS, but this is not recommended
4. Search for plugin [Single-project MSIX Packaging Tools for VS 2022](https://marketplace.visualstudio.com/items?itemName=ProjectReunion.MicrosoftSingleProjectMSIXPackagingToolsDev17) and install

## Development

1. Clone the project
2. If *Pixeval* is not a startup project, set it as a startup project
3. Build and run

* If that fails, you can try rebuilding the solution or restarting Visual Studio 2022

## If you want to take part into the development, there are some extra requirements

1. The basic knowledge about Windows XAML Framework, for more information, see [XAML Overview](https://docs.microsoft.com/windows/uwp/xaml-platform/xaml-overview)
2. A comprehensive understanding of C# and .NET development
3. Read the source code without documentation

## Project Structure

1. The *Pixeval* project contains the most relevant codes and the packaging files.
2. The *Pixeval.Controls* project contains includes a number of less coupled controls
3. The *Pixeval.CoreApi* project contains the API endpoints that are required by the project.
4. The *Pixeval.SourceGen* project contains code generators about settings.
5. The *Pixeval.Utilities* project contains the codes for universal util functions.

## The Guidelines for Version Control

This project follows a simple yet reasonable branching model: When you are willing to contribute your code, please create a new branch based on `main` and proceed with it. The new branch **MUST** follows `{user}/{qualifier}/{desc}`, where the `{user}` is your GitHub user name.

| Code content | qualifier | desc |
| - | - | - |
| Bug fixes | fix | A brief description of the vulnerability |
| New features | feature | A brief description of the new feature |
| Refactoring or code quality | refactor | A brief description of the refactoring section |

If your contribution contains more than one kind specified above, choose the rule that is most relevant to your contribution, and specify others in the commit message.

After your development, Please create a [Pull Request](https://github.com/Pixeval/Pixeval/pulls) and request to merge your branch into `main`

## In case that you are having problems... (Ordered by recommend priority)

1. Open an issue at [github](https://github.com/dylech30th/Pixeval/issues/new/choose)
2. Send an email to [decem0730@hotmail.com](mailto:decem0730@hotmail.com)
3. Join the QQ group 815791942 and ask developers face-to-face

## Acknowledgements (In no particular order)

[![Toolkit Contributors](https://contrib.rocks/image?repo=Pixeval/Pixeval)](https://github.com/Pixeval/Pixeval/graphs/contributors)

Made with [contrib.rocks](https://contrib.rocks).

## Support me

If this project meets your requirements perfectly, welcome the buy me a coffee at [afdian](https://afdian.net/@dylech30th). It's my pleasure to having your rewards. Thanks!

## JetBrains Open Source License

<div>
  <a href="https://www.jetbrains.com/?from=Pixeval" align="right"><img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg" alt="JetBrains" class="logo-footer" width="130" align="left"></a>
  <br/>
  
  The Jetbrains™ ReSharper is heavily used during the development of this project. Thank JetBrains s.r.o for providing the [JetBrains Open Source License](https://www.jetbrains.com/community/opensource/#support), If you are one of the passionate developers who often put JetBrains products into use, you can try to apply the JetBrains Open Source License from the [official channel](https://www.jetbrains.com/shop/eform/opensource) to help you and your developer teammates to significantly improve the productivities
</div>
