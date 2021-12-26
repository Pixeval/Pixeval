<div align="center">
    <img align="center" src="https://s1.ax1x.com/2020/04/03/GUMZjS.png" alt="logo" width="200">
    <h1 align="center">Pixeval</h1>
    <p align="center">A Strong, Fast, and Nice-looking Pixiv desktop client based on .NET 6 and Windows UI 3</p>
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

🌏: [**简体中文**](https://github.com/Pixeval/Pixeval/blob/master/README.en.md), [English](https://github.com/Pixeval/Pixeval/blob/master/README.en.md)

---

**The Pixeval that based on WinUI 3 is now WIP, the older version, which is the WPF version has been deprecated and expecting no more supports from developers, the WinUI 3 version provides a better user interface, a more structural codebase and a modern development experience compared to the WPF version. You can download and compile it yourself if you want to take a glance at the new version, follow the following steps to compile and run:**

> The WinUI 3 codebase for Pixeval supports only Windows 10 (1809, Build Number 17763) and higher.
> You can check this at Settings | System | About | Windows specifications

For more information, see [main page](https://sora.ink/pixeval/)

## Prerequisites
1. Visual Studio 2022 with WinUI 3 workload, for more informations, see the "prerequisites" section of [Create your first WinUI 3 app](https://docs.microsoft.com/en-us/windows/apps/winui/winui3/create-your-first-winui3-app?tabs=desktop-csharp)
2. Install Visual Studio extension [Single-project MSIX Packaging Tools for VS 2022](https://aka.ms/windowsappsdk/stable-vsix-2022-cs)
3. .NET 6 installed, you can download and install .NET 6 at [here](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

## If you want to take part into the development, there are some extra requirements:
4. The basic knowledge about Windows XAML Framework, for more information, see [XAML Overview](https://docs.microsoft.com/en-us/windows/uwp/xaml-platform/xaml-overview)
5. A comprehensive understanding of C# and .NET development
6. Read the source code without documentation

## Development
1. Clone the project
2. Set the *Pixeval* as startup project
3. Open `Build` | `ConfigurationManager` and select the check box labelled with `Deploy` at Pixeval's row
4. Build and run

## Project Structure
1. The *Pixeval* project contains the most relevant codes and the packaging files.
2. The *Pixeval.CoreApi* contains the API endpoints that are required by the project.
3. The *Pixeval.LoginProxy* contains the codes for login and the IPC
4. The *Pixeval.SourceGen* contains the codes that automatically generates classes from the localization resource files
5. The *Pixeval.Utilities* contains the codes for universal util functions

## The Guidelines for Version Control
This project follows a simple yet reasonable branching model: When you are willing to contribute your code, please create a new branch based on `dev/main` and proceed with it, **DO NOT** write codes directly based on `master` or `dev/main`, the new branch **MUST** follows `dev/{user}/{qualifier}-{name}`, where the `{name}` **MUST BE** your GitHub user name.

1. If your contribution mainly involves *BUG FIX*, then the `{qualifier}` **MUST BE** `fix`, and the `{name}` **SHOULD BE** the simple description of the bug you've just take care of.
2. If your contribution mainly involves *NEW FEATURES*, then `{qualifier}` **MUST BE** `feat` or `feature`, and `{name}` **SHOULD BE** the simple description of the new feature you've just created.
3. If your contribution mainly involves *REFACTOR or CODE QUALITY IMPROVEMENTS*, then `{qualifier}` **MUST BE** `refactor`, and `{name}` **SHOULD BE** the simple description of your refactoring.
4. If your contribution contains more than one kind specified above, choose the rule that is most relevant to your contribution, and specify others in the commit message.
5. The `master` branch **MUST BE** updated if and only if a new release is about to be published.


After your development, Please create a [Pull Request](https://github.com/Pixeval/Pixeval/pulls) and request to merge your branch into `dev/main`

## In case that you are having problems...(Ordered by recommend priority)
1. Open an issue at [github](https://github.com/dylech30th/Pixeval/issues/new)
2. Send an email to [decem0730@hotmail.com](mailto:decem0730@hotmail.com) 
3. Join the QQ group 815791942 and ask developers face-to-face

## Acknowledgements (In no particular order)
* [@sovetskyfish](https://github.com/sovetskyfish)
* [@Notsfsssf](https://github.com/Notsfsssf)
* [@ControlNet](https://github.com/ControlNet)
* [@wulunshijian](https://github.com/wulunshijian)
* [@duiweiya](https://github.com/duiweiya)
* [@Lasm_Gratel](https://github.com/LasmGratel)
* [@TheRealKamisama](https://github.com/TheRealKamisama)
* [@Summpot](https://github.com/Summpot)
* [@Poker-sang](https://github.com/Poker-sang)

## Support me
If this project meets your requirements perfectly, welcome the buy me a coffee at [afdian](https://afdian.net/@dylech30th). It's my pleasure to having your rewards. Thanks!

## JetBrains Open Source License
The Jetbrains™ ReSharper is heavily used during the development of this project. Thank JetBrains s.r.o for providing the [JetBrains Open Source License]((https://www.jetbrains.com/community/opensource/#support)), If you are one of the passionate developers who often put JetBrains products into use, you can try to apply the JetBrains Open Source License from the [official channel](https://www.jetbrains.com/shop/eform/opensource) to help you and your developer teammates to significantly improve the productivities

<figure style="width: min-content">
    <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/ReSharper_icon.png" width="200" height="200">
    <figcaption>Copyright © 2021 JetBrains s.r.o. </br>ReSharper and the ReSharper logo are registered trademarks of JetBrains s.r.o.</figcaption>
</figure>
