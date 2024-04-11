<div align="center">

<img src="./src/Pixeval/Assets/Images/logo.svg" alt="logo" width="200">

# Pixeval

Powerful, fast and beautiful Pixiv third-party desktop program based on .NET 8 and WinUI 3

[<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>](https://apps.microsoft.com/detail/Pixeval/9p1rzl9z8454?launch=true&mode=mini)

![](https://img.shields.io/github/stars/Pixeval/Pixeval?color=red&style=for-the-badge&logo=data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAACXBIWXMAAA7EAAAOxAGVKw4bAAAF7GlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDUgNzkuMTYzNDk5LCAyMDE4LzA4LzEzLTE2OjQwOjIyICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1sbnM6cGhvdG9zaG9wPSJodHRwOi8vbnMuYWRvYmUuY29tL3Bob3Rvc2hvcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RFdnQ9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZUV2ZW50IyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ0MgMjAxOSAoV2luZG93cykiIHhtcDpDcmVhdGVEYXRlPSIyMDIzLTAyLTA1VDE1OjM4OjE5KzA4OjAwIiB4bXA6TW9kaWZ5RGF0ZT0iMjAyMy0wMi0wNVQxNTo0NToyOSswODowMCIgeG1wOk1ldGFkYXRhRGF0ZT0iMjAyMy0wMi0wNVQxNTo0NToyOSswODowMCIgZGM6Zm9ybWF0PSJpbWFnZS9wbmciIHBob3Rvc2hvcDpDb2xvck1vZGU9IjMiIHBob3Rvc2hvcDpJQ0NQcm9maWxlPSJzUkdCIElFQzYxOTY2LTIuMSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDo0NzZjNjhkYS0zNzFmLWYyNGItOTRkZi02ZmVkN2Q1NDM5OGUiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6Mzc0ODYyNDUtMjQ1OC03YjRmLTg4ZjQtMzQ3NDUzNWZhMDczIiB4bXBNTTpPcmlnaW5hbERvY3VtZW50SUQ9InhtcC5kaWQ6Mzc0ODYyNDUtMjQ1OC03YjRmLTg4ZjQtMzQ3NDUzNWZhMDczIj4gPHhtcE1NOkhpc3Rvcnk+IDxyZGY6U2VxPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iY3JlYXRlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDozNzQ4NjI0NS0yNDU4LTdiNGYtODhmNC0zNDc0NTM1ZmEwNzMiIHN0RXZ0OndoZW49IjIwMjMtMDItMDVUMTU6Mzg6MTkrMDg6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCBDQyAyMDE5IChXaW5kb3dzKSIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6NDc2YzY4ZGEtMzcxZi1mMjRiLTk0ZGYtNmZlZDdkNTQzOThlIiBzdEV2dDp3aGVuPSIyMDIzLTAyLTA1VDE1OjQ1OjI5KzA4OjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgMjAxOSAoV2luZG93cykiIHN0RXZ0OmNoYW5nZWQ9Ii8iLz4gPC9yZGY6U2VxPiA8L3htcE1NOkhpc3Rvcnk+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+sj4YggAAAQ5JREFUeNrt20kSwyAMRNH0/Q/d2acSO2CwNXwfAFlvAZIKZPvV+RMAzwF8BhYAjQB+BRUADQDOAgqAwgD/BhMABQFGAwmAQgCzQQRAAYCrAQRAYoBViwuAhACrFxYAiQB2qSoaQKSBonYAVJmY6gyg62hYcvO5OAAAsAe42o4/dDJ8OwbdJfmjOsAdkj8rhFw5cSrBgV7AVZMfaYZcMfnRbtDVkp9ph10p+dl5gKskPwsQGWF4KHJlIuTsyV8FiIQwPSsEAAD2AAAAAAAAACiFAQAAgCcAtGm98ADaCLt9IOIbfs7VAGa7NWcHWHWpyRkBVt/ochaA3Te8HRXg7idvjgLwyIvPVf/D4+nuAG8V/wSNyqWVwwAAAABJRU5ErkJggg==)
![](https://img.shields.io/static/v1?label=contact%20me&message=hotmail&color=green&style=for-the-badge&logo=gmail&logoColor=white)
[![](https://img.shields.io/static/v1?label=chatting&message=qq&color=blue&style=for-the-badge&logo=tencentqq&logoColor=white)](https://jq.qq.com/?_wv=1027&k=5hGmJbQ)
[![](https://img.shields.io/github/license/Pixeval/Pixeval?style=for-the-badge&logo=gnu&logoColor=white)](https://github.com/Pixeval/Pixeval/blob/main/LICENSE)
[![](https://img.shields.io/static/v1?label=feedback&message=issues&color=pink&style=for-the-badge&logo=Github&logoColor=white)](https://github.com/Pixeval/Pixeval/issues/new/choose)
[![](https://img.shields.io/static/v1?label=runtime&message=.NET%208.0&color=yellow&style=for-the-badge&logo=.NET&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/8.0)
![](https://img.shields.io/badge/Platform-Windows10.0.19041-512BD4?&style=for-the-badge&logo=Windows&logoColor=white)

</div>

🌏: [简体中文](README.md)，[**English**](README.en.md)，[Русский](README.ru.md)

---

**The Pixeval that based on WinUI 3 is now WIP, the older version, 
which is the WPF version has been deprecated and expecting no more supports from developers.**

> The WinUI 3 codebase for Pixeval supports only Windows 10 (1809, Build Number 17763) and higher.
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
  <a href="https://www.jetbrains.com/?from=Pixeval" align="right"><img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg" alt="JetBrains" class="logo-footer" width="100" align="left"></a>
  <br/>
  
  The Jetbrains™ ReSharper is heavily used during the development of this project. Thank JetBrains s.r.o for providing the [JetBrains Open Source License]((https://www.jetbrains.com/community/opensource/#support)), If you are one of the passionate developers who often put JetBrains products into use, you can try to apply the JetBrains Open Source License from the [official channel](https://www.jetbrains.com/shop/eform/opensource) to help you and your developer teammates to significantly improve the productivities
</div>
