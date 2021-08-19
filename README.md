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

**基于WinUI3的Pixeval已经正在开发中，而作为旧的WPF版本将会停止维护并不再收到任何支持，WinUI3版本提供了更好的UI，更好的项目结构已经更好的开发体验，如果你想要了解目前的开发进度，可以通过以下方法来下载并编译该项目**

更多详细信息请前往[项目主页](https://sora.ink/pixeval/)查看

## 准备
1. 带有WinUI 3工作负载的Visual Studio 2019，你可以在[创建你的第一个WinUI 3 app](https://docs.microsoft.com/zh-cn/windows/apps/winui/winui3/)的"先决条件"部分查看更多信息
2. 安装.NET 5，你可以在[这里](https://dotnet.microsoft.com/download/dotnet/5.0)下载并安装.NET 5

### 如果你想要参与进开发进程中，则需要...
3. 对Windows XAML Framework的基本了解，要了解更多相关信息请看[XAML概述](https://docs.microsoft.com/zh-cn/windows/uwp/xaml-platform/xaml-overview)
4. 对C# .NET开发的一定了解以及开发经验
5. 具有不依赖文档阅读代码的能力


## 开发
1. 克隆本项目
2. 将*Pixeval (Project)*设置为启动项目
3. 构建并运行

### 项目结构
1. *Pixeval*项目包含了项目本身的逻辑以及布局代码
2. *Pixeval (Packaged)*将原有项目包装进一个安装包
3. *Pixeval.LoginProxy*包含了Pixiv登录以及IPC相关的代码
4. *Pixeval.SourceGen*包含了从本地化文件自动生成对应C#类的代码

### 如果遇到任何问题(按照推荐程度优先级排序)
1. 在[github](https://github.com/dylech30th/Pixeval/issues/new)提交新的Issue
2. 给[decem0730@hotmail.com](mailto:decem0730@hotmail.com)发送邮件
3. 加入QQ群815791942来面对面的和开发者反馈问题

**鸣谢：(排名不分先后)**
* [@tobiichiamane](https://github.com/tobiichiamane)
* [@Notsfsssf](https://github.com/Notsfsssf)
* [@ControlNet](https://github.com/ControlNet)
* [@wulunshijian](https://github.com/wulunshijian)
* [@duiweiya](https://github.com/duiweiya)
* [@Lasm_Gratel](https://github.com/NanamiArihara)
* [@TheRealKamisama](https://github.com/TheRealKamisama)
* [@Summpot](https://github.com/Summpot)
* 使用并给予积极意见的各位

**支持作者：**
如果你感觉该项目帮助到了你，欢迎前往[爱发电](https://afdian.net/@dylech30th)赞助我，你的支持是我维护项目的动力，谢谢！
