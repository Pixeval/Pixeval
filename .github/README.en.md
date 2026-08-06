<div align="center">

<img src="../src/Pixeval/Assets/logo.svg" alt="logo" width="200">

# Pixeval

Powerful, fast and beautiful Pixiv third-party desktop program based on .NET 10 and Avalonia

[<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>](https://apps.microsoft.com/detail/Pixeval/9p1rzl9z8454?launch=true&mode=mini)

![](https://img.shields.io/github/stars/Pixeval/Pixeval?color=red&style=for-the-badge&logo=data:image/svg+xml;charset=utf-8;base64,PHN2ZyB3aWR0aD0iNDgiIGhlaWdodD0iNDgiIHZpZXdCb3g9IjAgMCA0OCA0OCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHBhdGggZD0iTTIxLjgwMyA2LjA4NTQ0QzIyLjcwMTcgNC4yNjQ0OSAyNS4yOTgzIDQuMjY0NDggMjYuMTk3IDYuMDg1NDRMMzEuMDQ5MyAxNS45MTc0TDQxLjg5OTYgMTcuNDk0QzQzLjkwOTEgMTcuNzg2IDQ0LjcxMTUgMjAuMjU1NiA0My4yNTc0IDIxLjY3M0wzNS40MDYxIDI5LjMyNjFMMzcuMjU5NSA0MC4xMzI1QzM3LjYwMjggNDIuMTMzOSAzNS41MDIxIDQzLjY2MDIgMzMuNzA0NyA0Mi43MTUyTDI0IDM3LjYxMzJMMTQuMjk1MiA0Mi43MTUyQzEyLjQ5NzggNDMuNjYwMiAxMC4zOTcxIDQyLjEzMzkgMTAuNzQwNCA0MC4xMzI1TDEyLjU5MzggMjkuMzI2MUw0Ljc0MjU1IDIxLjY3M0MzLjI4ODQzIDIwLjI1NTYgNC4wOTA4MyAxNy43ODYgNi4xMDAzNyAxNy40OTRMMTYuOTUwNiAxNS45MTc0TDIxLjgwMyA2LjA4NTQ0WiIgZmlsbD0iI2ZmZmZmZiIvPgo8L3N2Zz4K)
![](https://img.shields.io/static/v1?label=contact%20me&message=hotmail&color=green&style=for-the-badge&logo=gmail&logoColor=white)
[![](https://img.shields.io/static/v1?label=chatting&message=qq&color=blue&style=for-the-badge&logo=qq&logoColor=white)](https://jq.qq.com/?_wv=1027\&k=5hGmJbQ)
[![](https://img.shields.io/github/license/Pixeval/Pixeval?style=for-the-badge&logo=gnu&logoColor=white)](https://github.com/Pixeval/Pixeval/blob/main/LICENSE)
[![](https://img.shields.io/static/v1?label=feedback&message=issues&color=pink&style=for-the-badge&logo=Github&logoColor=white)](https://github.com/Pixeval/Pixeval/issues/new/choose)
[![](https://img.shields.io/static/v1?label=runtime&message=.NET%2010.0&color=yellow&style=for-the-badge&logo=.NET&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/8.0)
![](https://img.shields.io/badge/Framework-avalonia-512BD4?&style=for-the-badge&logo=avaloniaui)

</div>

[**Chinese Simplified**](README.md)

---

**Pixeval, based on Avalonia, is already under development, and the old WPF/WinUI3 version is no longer heavily maintained. Please switch to the newer Pixeval anytime.**

For more information, go to the [Project Homepage](https://pixeval.github.io/)

**Avalonia provides a better UI, a better project structure and development experience. If you want to know the current development progress, you can reference the [Contributing Guide](CONTRIBUTING.md) to download and compile the project.**

## Supported Platforms

- Windows 8 and higher
- MacOS
- Linux
- Android 16 (API 36) and higher
- iOS 13 and higher

<!-- * 浏览器 -->

### Install Pixeval via Homebrew

[![brew test-bot](https://github.com/Pixeval/homebrew-tap/actions/workflows/tests.yml/badge.svg)](https://github.com/Pixeval/homebrew-tap/actions/workflows/tests.yml)

Pixeval provides a self-hosted [Homebrew Tap](https://github.com/Pixeval/homebrew-tap) to install Pixeval on Mac and Linux

**Installation**

```bash
# Add the tap
brew tap Pixeval/tap

# Trust the tap
brew trust Pixeval/tap

# Install Pixeval (Mac Only)
brew install --cask pixeval

# Install Pixeval via Formula (Mac & Linux)
brew install --formula pixeval
```

**Uninstall**

```bash
brew uninstall --cask --zap pixeval

# Only uninstall the app, but keep user data
brew uninstall --cask pixeval

# Download Pixeval (Formula)
brew uninstall pixeval

# Remove the tap itself
brew untap Pixeval/tap
```

> [!NOTE]
> If the default tap origin is slow, try manually downloading the [Cask File](https://raw.githubusercontent.com/Pixeval/homebrew-tap/refs/heads/main/Casks/pixeval.rb) or the [Formula File](https://raw.githubusercontent.com/Pixeval/homebrew-tap/refs/heads/main/Formula/pixeval.rb), run `brew install --cask /path/to/pixeval.rb` or `brew install --formula /path/to/pixeval.rb` after switching to the mirror domain.

See the [Tap Repository](https://github.com/Pixeval/homebrew-tap) for more information

## In case that you are having problems... (Ordered by recommend priority)

1. Open an issue at [github](https://github.com/dylech30th/Pixeval/issues/new/choose)
2. Send an email to [decem0730@hotmail.com](mailto:decem0730@hotmail.com)
3. Join the QQ group 815791942 and ask developers face-to-face

## Acknowledgments (in no particular order)

[![Toolkit Contributors](https://contrib.rocks/image?repo=Pixeval/Pixeval)](https://github.com/Pixeval/Pixeval/graphs/contributors)

Made with [contrib.rocks](https://contrib.rocks).
