<div align="center">
    <img align="center" src="https://s1.ax1x.com/2020/04/03/GUMZjS.png" alt="logo" width="200">
    <h1 align="center">Pixeval</h1>
    <p align="center">A strong, fast, and nice-looking Pixiv desktop app based on .NET 5 and Windows UI 3</p>
    <p align="center">
        <img src="https://img.shields.io/github/stars/Rinacm/Pixeval?color=red&style=flat-square">
        <a href="mailto:decem0730@hotmail.com">
            <img src="https://img.shields.io/static/v1?label=contact%20me&message=hotmail&color=green&style=flat-square">
        </a>
        <a href="https://jq.qq.com/?_wv=1027&k=5hGmJbQ" target="_blank">
            <img src="https://img.shields.io/static/v1?label=chatting&message=qq&color=blue&style=flat-square"
        </a>
        <a href="http://47.95.218.243/index.html" target="_blank">
            <img src="https://img.shields.io/static/v1?label=homepage&message=pixeval&color=blueviolet&style=flat-square">
        </a>
        <a href="https://github.com/Rinacm/Pixeval/blob/master/LICENSE" target="_blank">
            <img src="https://img.shields.io/github/license/Rinacm/Pixeval?style=flat-square">
        </a>
        <a href="https://github.com/Rinacm/Pixeval/issues/new/choose" target="_blank">
            <img src="https://img.shields.io/static/v1?label=feedback&message=issues&color=pink&style=flat-square">
        </a>
        <a href="https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-3.1.3-windows-x64-installer" target="_blank">
            <img src="https://img.shields.io/static/v1?label=runtime&message=.NET%20Core%203.1&color=yellow&style=flat-square">
        </a>
    </p>
</div>

🌏: [**简体中文**](https://github.com/Pixeval/Pixeval/blob/master/README.en.md), [English](https://github.com/Pixeval/Pixeval/blob/master/README.en.md)

---

**The Pixeval that based on WinUI 3 is now WIP, the older version, which is the WPF version has been deprecated and expecting no more supports from developers, the WinUI 3 version provides a better user interface, a more structural codebase and a modern development experience compared to the WPF version. You can download and compile it yourself if you want to take a glance at the new version, follow the following steps to compile and run:**

For more information, see [main page](https://sora.ink/pixeval/)

## Prerequisites
1. Visual Studio 2019 with WinUI 3 workload, for more informations, see the "prerequisites" section of [Create your first WinUI 3 app](https://docs.microsoft.com/en-us/windows/apps/winui/winui3/create-your-first-winui3-app?tabs=desktop-csharp)
2. Install Visual Studio extension [Single-project MSIX Packaging Tools for VS 2019](https://marketplace.visualstudio.com/items?itemName=ProjectReunion.MicrosoftSingleProjectMSIXPackagingTools)(note if you're using VS 2022, use the corresponding version instead)
3. .NET 5 installed, you can download and install .NET 5 at [here](https://dotnet.microsoft.com/download/dotnet/5.0)

### If you want to take part into the development, there are some extra requirements:
3. The basic knowledge about Windows XAML Framework, for more information, see [XAML Overview](https://docs.microsoft.com/en-us/windows/uwp/xaml-platform/xaml-overview)
4. The comprehension and the experience of C# .NET development
5. Read the source code without documentation

## Development
1. Clone the project
2. Set the *Pixeval (Packaged)* as startup project
3. Open `Build` | `ConfigurationManager` and select the check box labelled with `Deploy` at Pixeval's row
3. Build and run

### Project Structure
1. The *Pixeval* project contains the most relevant codes and the packaging files.
2. The *Pixeval.CoreApi* contains the API endpoints that are required by the project.
3. The *Pixeval.LoginProxy* contains the codes for login and the IPC
4. The *Pixeval.SourceGen* contains the codes that automatically generates classes from the localization resource files

### In case that you are having problems...(Ordered by recommend priority)
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

## Support me
If this project meets your requirements perfectly, welcome the buy me a coffee at [afdian](https://afdian.net/@dylech30th). It's my pleasure to having your rewards. Thanks!

## JetBrains Open Source License
The Jetbrains™ ReSharper is heavily used during the development of this project. Thank JetBrains s.r.o for providing the [JetBrains Open Source License]((https://www.jetbrains.com/community/opensource/#support)), If you are one of the passionate developers who often put JetBrains products into use, you can try to apply the JetBrains Open Source License from the [official channel](https://www.jetbrains.com/shop/eform/opensource) to help you and your developer teammates to significantly improve the productivities

<figure style="width: min-content">
    <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/ReSharper_icon.png" width="200" height="200">
    <figcaption>Copyright © 2021 JetBrains s.r.o. </br>ReSharper and the ReSharper logo are registered trademarks of JetBrains s.r.o.</figcaption>
</figure>
