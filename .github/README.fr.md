<div align="center">

<img src="../src/Pixeval/Assets/Images/logo.svg" alt="logo" width="200">

# Pixeval

Un client desktop tierce de Pixiv, puissant, rapid et vivide, basé sur .NET 8 et WinUI 3

[<img src="https://get.microsoft.com/images/fr%20dark.svg" width="200"/>](https://apps.microsoft.com/detail/Pixeval/9p1rzl9z8454?launch=true\&mode=mini)

![](https://img.shields.io/github/stars/Pixeval/Pixeval?color=red\&style=for-the-badge\&logo=data:image/svg+xml;charset=utf-8;base64,PHN2ZyB3aWR0aD0iNDgiIGhlaWdodD0iNDgiIHZpZXdCb3g9IjAgMCA0OCA0OCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHBhdGggZD0iTTIxLjgwMyA2LjA4NTQ0QzIyLjcwMTcgNC4yNjQ0OSAyNS4yOTgzIDQuMjY0NDggMjYuMTk3IDYuMDg1NDRMMzEuMDQ5MyAxNS45MTc0TDQxLjg5OTYgMTcuNDk0QzQzLjkwOTEgMTcuNzg2IDQ0LjcxMTUgMjAuMjU1NiA0My4yNTc0IDIxLjY3M0wzNS40MDYxIDI5LjMyNjFMMzcuMjU5NSA0MC4xMzI1QzM3LjYwMjggNDIuMTMzOSAzNS41MDIxIDQzLjY2MDIgMzMuNzA0NyA0Mi43MTUyTDI0IDM3LjYxMzJMMTQuMjk1MiA0Mi43MTUyQzEyLjQ5NzggNDMuNjYwMiAxMC4zOTcxIDQyLjEzMzkgMTAuNzQwNCA0MC4xMzI1TDEyLjU5MzggMjkuMzI2MUw0Ljc0MjU1IDIxLjY3M0MzLjI4ODQzIDIwLjI1NTYgNC4wOTA4MyAxNy43ODYgNi4xMDAzNyAxNy40OTRMMTYuOTUwNiAxNS45MTc0TDIxLjgwMyA2LjA4NTQ0WiIgZmlsbD0iI2ZmZmZmZiIvPgo8L3N2Zz4K)
![](https://img.shields.io/static/v1?label=contact%20me\&message=hotmail\&color=green\&style=for-the-badge\&logo=gmail\&logoColor=white)
[![](https://img.shields.io/static/v1?label=chatting\&message=qq\&color=blue\&style=for-the-badge\&logo=qq\&logoColor=white)](https://jq.qq.com/?_wv=1027\&k=5hGmJbQ)
[![](https://img.shields.io/github/license/Pixeval/Pixeval?style=for-the-badge\&logo=gnu\&logoColor=white)](https://github.com/Pixeval/Pixeval/blob/main/LICENSE)
[![](https://img.shields.io/static/v1?label=feedback\&message=issues\&color=pink\&style=for-the-badge\&logo=Github\&logoColor=white)](https://github.com/Pixeval/Pixeval/issues/new/choose)
[![](https://img.shields.io/static/v1?label=runtime\&message=.NET%208.0\&color=yellow\&style=for-the-badge\&logo=.NET\&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/8.0)
![](https://img.shields.io/badge/Platform-Windows10.0.19041-512BD4?\&style=for-the-badge\&logo=data:image/svg+xml;charset=utf-8;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI0OCIgaGVpZ2h0PSI0OCI+CjxwYXRoIGQ9Ik00LjggMy44NGEuOTYuOTYgMCAwIDAtLjk2Ljk2djE4LjI0aDE5LjJWMy44NFptMjAuMTYgMHYxOS4yaDE5LjJWNC44YS45Ni45NiAwIDAgMC0uOTYtLjk2Wk0zLjg0IDI0Ljk2VjQzLjJjMCAuNTMuNDMuOTYuOTYuOTZoMTguMjR2LTE5LjJabTIxLjEyIDB2MTkuMkg0My4yYS45Ni45NiAwIDAgMCAuOTYtLjk2VjI0Ljk2Wm0wIDAiIGZpbGw9IiNmZmZmZmYiLz4KPC9zdmc+)

</div>

🌏: [简体中文](README.md), [English](README.en.md), [Русский](README.ru.md), [**Français**](README.fr.md)

---

**Pixeval est actuellement basé sur WinUI 3 avec développement en cours.
L'ancienne version du WPF a été dépréciée et ne recevra plus de supports depuis les développeurs.**

> La base de code de Pixeval prend seulement en compte Windows 10 (2004, Build Number 19041) ou versions ultérieures.
> 可以通过以下步骤查看。右键点击“开始”按钮，选择然后选择系统；或者在“设置”中，依次选择“系统”>“系统信息”，此时页面中的Windows规格下可以看到相关信息。

Pour plus d'information, consultez [site du projet](https://sora.ink/pixeval/)

**La version de WinUI 3 donne une meilleure interface utilisateur, une base de code plus structurée et une
expérience de développement plus moderne que la version WPF. Vous pouvez télécharger ce projet et le compiler
par vous-même si vous voulez jeter un oeil sur cette nouvelle version, en suivant les étapes prochaines pour
la compilation et le démarrage:**

## Pré-requis

1. 拥有[git](https://git-scm.com)环境
2. Installer [Visual Studio 2022](https://visualstudio.microsoft.com/vs) (Roslyn 4.x nécessite VS17.x, i.e. VS2022)
   如果已安装请确认是VS2022的最新版本，因为负载里.NET SDK的版本和VS的版本有关，低版本可能不包含.NET8 SDK。
3. Dans **Tools - Get Tools and Features**, sous **Workloads**, sélectionner .NET Desktop Development (Dans le panneau Installation Details du dialogue installation, sélectionner le Windows App SDK C# Template en bas de la liste, cependant ceci n'est pas requis.)Vous pouvez vous référer de [Install Tools for Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/set-up-your-development-environment)

## Développement

1. Cloner le projet
2. Si _Pixeval_ n'est pas un startup project, configurer-le comme celui-ci
3. Builder and démarrer l'application

- Si cela échoue, vous pouvez essayer de le rebuilder ou redémarrer Visual Studio 2022

## Si vous voulez participer dans le développement, voici quelques critères extras

1. Une connaissance de base de Windows XAML Framework, pour plus d'informations: [XAML Overview](https://docs.microsoft.com/windows/uwp/xaml-platform/xaml-overview)
2. Une connaissance compréhensive de C# et .NET développement
3. Capable de lire les codes sources sans documentations

## Consignes pour le Version Control

Ce projet est basé sur un modèle de branching simple mais raisonnable: Lorsque vous contribuez, vous créez une nouvelle branche basée sur la branche principale `main` et travaillez sur votre branche. Cette nouvelle branche **DOIT** être nommée de façon `{utilisateur}/{quantificateur}/{description}`, où l'utilisateur est votre nom d'identifiant GitHub.

| Contenu de code                 | Quantificateur | Description                                         |
| ------------------------------- | -------------- | --------------------------------------------------- |
| Bug fixes                       | fix            | Une description simple de la vulnérabilité          |
| Nouvelles features              | feature        | Une description simple de la nouvelle feature       |
| Refactoring ou qualité de codes | refactor       | Une description simple de la section de refactoring |

Si votre contribution contient plus d'un type spécifié dessus, choisissez un rôle qui ressemble plus à votre contribution, et spécifiez les autres dans le mssage de commit.

Après votre développement, vous devez créer un [Pull Request](https://github.com/Pixeval/Pixeval/pulls) et demander à merger votre branche dans la branche `main`

## Structure du projet

1. Le projet _Pixeval_ contient la plupart des codes métiers et des fichiers de packages.
2. Le projet _Pixeval.Controls_ contient de nombreux controls légèrement couplés.
3. Le projet _Pixeval.CoreApi_ contient des endpoints API requis par ce projet.
4. Le projet _Pixeval.SourceGen_ contient des générateurs de codes sources concernant les configurations.
5. Le projet _Pixeval.Utilities_ contient des fonctions utilitaires pour ce projet.

## En cas de problème... (Par priorité de recommendation)

1. Ouvrir un issue sur [github](https://github.com/dylech30th/Pixeval/issues/new/choose)
2. Envoyer un mail à [decem0730@hotmail.com](mailto:decem0730@hotmail.com)
3. Rejoindre le groupe QQ 815791942 et poser vos questions en face des développeurs

## Crédits (Sans ordre particulière)

[![Toolkit Contributors](https://contrib.rocks/image?repo=Pixeval/Pixeval)](https://github.com/Pixeval/Pixeval/graphs/contributors)

Made with [contrib.rocks](https://contrib.rocks).

## Me sponsoriser

Si ce projet vous plaît parfaitment, vous pouvez acheter un café pour moi dans [afdian](https://afdian.net/@dylech30th). Je le recevrai avec grand plaisir, merci!

<div>
  <a href="https://www.jetbrains.com/?from=Pixeval" align="right"><img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg" alt="JetBrains" class="logo-footer" width="130" align="left"></a>
  <br/>

Le Jetbrains™ ReSharper est fortement utilisé lors du développement de ce projet. Merci à JetBrains s.r.o pour avoir fourni la [JetBrains Open Source License](https://www.jetbrains.com/community/opensource/#support), Si vous êtes un des développeurs passionnés qui utilisent souvent les produits JetBrains, vous pouvez essayer d'appliquer la JetBrains Open Source License depuis le [canal officiel](https://www.jetbrains.com/shop/eform/opensource) pour vous aider, vous et vos coéquipiers développeurs, à améliorer significativement les productivités.

</div>
