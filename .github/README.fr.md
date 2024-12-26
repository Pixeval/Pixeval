<div align="center">

<img src="./src/Pixeval/Assets/Images/logo.svg" alt="logo" width="200">

# Pixeval

Un client desktop tierce de Pixiv, puissant, rapid et vivide, basé sur .NET 8 et WinUI 3

[<img src="https://get.microsoft.com/images/fr-fr%20dark.svg" width="200"/>](https://apps.microsoft.com/detail/Pixeval/9p1rzl9z8454?launch=true&mode=mini)

![](https://img.shields.io/github/stars/Pixeval/Pixeval?color=red&style=for-the-badge&logo=data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAACXBIWXMAAA7EAAAOxAGVKw4bAAAF7GlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS42LWMxNDUgNzkuMTYzNDk5LCAyMDE4LzA4LzEzLTE2OjQwOjIyICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOmRjPSJodHRwOi8vcHVybC5vcmcvZGMvZWxlbWVudHMvMS4xLyIgeG1sbnM6cGhvdG9zaG9wPSJodHRwOi8vbnMuYWRvYmUuY29tL3Bob3Rvc2hvcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RFdnQ9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZUV2ZW50IyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ0MgMjAxOSAoV2luZG93cykiIHhtcDpDcmVhdGVEYXRlPSIyMDIzLTAyLTA1VDE1OjM4OjE5KzA4OjAwIiB4bXA6TW9kaWZ5RGF0ZT0iMjAyMy0wMi0wNVQxNTo0NToyOSswODowMCIgeG1wOk1ldGFkYXRhRGF0ZT0iMjAyMy0wMi0wNVQxNTo0NToyOSswODowMCIgZGM6Zm9ybWF0PSJpbWFnZS9wbmciIHBob3Rvc2hvcDpDb2xvck1vZGU9IjMiIHBob3Rvc2hvcDpJQ0NQcm9maWxlPSJzUkdCIElFQzYxOTY2LTIuMSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDo0NzZjNjhkYS0zNzFmLWYyNGItOTRkZi02ZmVkN2Q1NDM5OGUiIHhtcE1NOkRvY3VtZW50SUQ9InhtcC5kaWQ6Mzc0ODYyNDUtMjQ1OC03YjRmLTg4ZjQtMzQ3NDUzNWZhMDczIiB4bXBNTTpPcmlnaW5hbERvY3VtZW50SUQ9InhtcC5kaWQ6Mzc0ODYyNDUtMjQ1OC03YjRmLTg4ZjQtMzQ3NDUzNWZhMDczIj4gPHhtcE1NOkhpc3Rvcnk+IDxyZGY6U2VxPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0iY3JlYXRlZCIgc3RFdnQ6aW5zdGFuY2VJRD0ieG1wLmlpZDozNzQ4NjI0NS0yNDU4LTdiNGYtODhmNC0zNDc0NTM1ZmEwNzMiIHN0RXZ0OndoZW49IjIwMjMtMDItMDVUMTU6Mzg6MTkrMDg6MDAiIHN0RXZ0OnNvZnR3YXJlQWdlbnQ9IkFkb2JlIFBob3Rvc2hvcCBDQyAyMDE5IChXaW5kb3dzKSIvPiA8cmRmOmxpIHN0RXZ0OmFjdGlvbj0ic2F2ZWQiIHN0RXZ0Omluc3RhbmNlSUQ9InhtcC5paWQ6NDc2YzY4ZGEtMzcxZi1mMjRiLTk0ZGYtNmZlZDdkNTQzOThlIiBzdEV2dDp3aGVuPSIyMDIzLTAyLTA1VDE1OjQ1OjI5KzA4OjAwIiBzdEV2dDpzb2Z0d2FyZUFnZW50PSJBZG9iZSBQaG90b3Nob3AgQ0MgMjAxOSAoV2luZG93cykiIHN0RXZ0OmNoYW5nZWQ9Ii8iLz4gPC9yZGY6U2VxPiA8L3htcE1NOkhpc3Rvcnk+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpSREY+IDwveDp4bXBtZXRhPiA8P3hwYWNrZXQgZW5kPSJyIj8+sj4YggAAAQ5JREFUeNrt20kSwyAMRNH0/Q/d2acSO2CwNXwfAFlvAZIKZPvV+RMAzwF8BhYAjQB+BRUADQDOAgqAwgD/BhMABQFGAwmAQgCzQQRAAYCrAQRAYoBViwuAhACrFxYAiQB2qSoaQKSBonYAVJmY6gyg62hYcvO5OAAAsAe42o4/dDJ8OwbdJfmjOsAdkj8rhFw5cSrBgV7AVZMfaYZcMfnRbtDVkp9ph10p+dl5gKskPwsQGWF4KHJlIuTsyV8FiIQwPSsEAAD2AAAAAAAAACiFAQAAgCcAtGm98ADaCLt9IOIbfs7VAGa7NWcHWHWpyRkBVt/ochaA3Te8HRXg7idvjgLwyIvPVf/D4+nuAG8V/wSNyqWVwwAAAABJRU5ErkJggg==)
![](https://img.shields.io/static/v1?label=contact%20me&message=hotmail&color=green&style=for-the-badge&logo=gmail&logoColor=white)
[![](https://img.shields.io/static/v1?label=chatting&message=qq&color=blue&style=for-the-badge&logo=tencentqq&logoColor=white)](https://jq.qq.com/?_wv=1027&k=5hGmJbQ)
[![](https://img.shields.io/github/license/Pixeval/Pixeval?style=for-the-badge&logo=gnu&logoColor=white)](https://github.com/Pixeval/Pixeval/blob/main/LICENSE)
[![](https://img.shields.io/static/v1?label=feedback&message=issues&color=pink&style=for-the-badge&logo=Github&logoColor=white)](https://github.com/Pixeval/Pixeval/issues/new/choose)
[![](https://img.shields.io/static/v1?label=runtime&message=.NET%208.0&color=yellow&style=for-the-badge&logo=.NET&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/8.0)
![](https://img.shields.io/badge/Platform-Windows10.0.19041-512BD4?&style=for-the-badge&logo=Windows&logoColor=white)

</div>

🌏: [简体中文](README.md), [English](README.en.md), [Русский](README.ru.md), [**Français**](README.fr.md)

---

**Pixeval est actuellement basé sur WinUI 3 avec développement en cours. 
L'ancienne version du WPF a été dépréciée et ne recevra plus de supports depuis les développeurs.**

> La base de code de Pixeval prend seulement en compte Windows 10 (2004, Build Number 19041) ou versions ultérieures.
> Vous pouvez vérifier votre version de Windows sur Paramètres > Système > A propos de > Spécifications de l'appareil

Pour plus d'information, consultez [site du projet](https://sora.ink/pixeval/)

**La version de WinUI 3 donne une meilleure interface utilisateur, une base de code plus structurée et une
expérience de développement plus moderne que la version WPF. Vous pouvez télécharger ce projet et le compiler
par vous-même si vous voulez jeter un oeil sur cette nouvelle version, en suivant les étapes prochaines pour
la compilation et le démarrage:**

## Pré-requis

1. Installer [Visual Studio 2022](https://visualstudio.microsoft.com/vs) (Roslyn 4.x nécessite VS17.x, i.e. VS2022)
2. Dans **Tools - Get Tools and Features**, sous **Workloads**, sélectionner .NET Desktop Development (Dans le panneau Installation Details du dialogue installation, sélectionner le Windows App SDK C# Template en bas de la liste, cependant ceci n'est pas requis.) Vous pouvez vous référer de [Install Tools for Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/set-up-your-development-environment)
3. Sélectionner .NET 8 dans **Tools - Get Tools and Features - Individual components**, ou télécharger la dernière version de [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) en dehors de VS, mais cette approche est non recommendée
4. Chercher le plugin [Single-project MSIX Packaging Tools for VS 2022](https://marketplace.visualstudio.com/items?itemName=ProjectReunion.MicrosoftSingleProjectMSIXPackagingToolsDev17) et installer

## Développement

1. Cloner le projet
2. Si *Pixeval* n'est pas un startup project, configurer-le comme celui-ci
3. Builder and démarrer l'application

* Si cela échoue, vous pouvez essayer de le rebuilder ou redémarrer Visual Studio 2022

## Si vous voulez participer dans le développement, voici quelques critères extras

1. Une connaissance de base de Windows XAML Framework, pour plus d'informations: [XAML Overview](https://docs.microsoft.com/windows/uwp/xaml-platform/xaml-overview)
2. Une connaissance compréhensive de C# et .NET développement
3. Capable de lire les codes sources sans documentations

## Structure du projet

1. Le projet *Pixeval* contient la plupart des codes métiers et des fichiers de packages.
2. Le projet *Pixeval.Controls* contient de nombreux controls légèrement couplés.
3. Le projet *Pixeval.CoreApi* contient des endpoints API requis par ce projet.
4. Le projet *Pixeval.SourceGen* contient des générateurs de codes sources concernant les configurations.
5. Le projet *Pixeval.Utilities* contient des fonctions utilitaires pour ce projet.

## Consignes pour le Version Control

Ce projet est basé sur un modèle de branching simple mais raisonnable: Lorsque vous contribuez, vous créez une nouvelle branche basée sur la branche principale `main` et travaillez sur votre branche. Cette nouvelle branche **DOIT** être nommée de façon `{utilisateur}/{quantificateur}/{description}`, où l'utilisateur est votre nom d'identifiant GitHub.

| Contenu de code                 | Quantificateur | Description                                         |
|---------------------------------|----------------|-----------------------------------------------------|
| Bug fixes                       | fix            | Une description simple de la vulnérabilité          |
| Nouvelles features              | feature        | Une description simple de la nouvelle feature       |
| Refactoring ou qualité de codes | refactor       | Une description simple de la section de refactoring |

Si votre contribution contient plus d'un type spécifié dessus, choisissez un rôle qui ressemble plus à votre contribution, et spécifiez les autres dans le mssage de commit.

Après votre développement, vous devez créer un [Pull Request](https://github.com/Pixeval/Pixeval/pulls) et demander à merger votre branche dans la branche `main`

## En cas de problème... (Par priorité de recommendation)

1. Ouvrir un issue sur [github](https://github.com/dylech30th/Pixeval/issues/new/choose)
2. Envoyer un mail à [decem0730@hotmail.com](mailto:decem0730@hotmail.com)
3. Rejoindre le groupe QQ 815791942 et poser vos questions en face des développeurs

## Crédits (Sans ordre particulière)

[![Toolkit Contributors](https://contrib.rocks/image?repo=Pixeval/Pixeval)](https://github.com/Pixeval/Pixeval/graphs/contributors)

Made with [contrib.rocks](https://contrib.rocks).

## Me sponsoriser

Si ce projet vous plaît parfaitment, vous pouvez acheter un café pour moi dans [afdian](https://afdian.net/@dylech30th). Je le recevrai avec grand plaisir, merci!

## Licence Open Source JetBrains

<div>
  <a href="https://www.jetbrains.com/?from=Pixeval" align="right"><img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg" alt="JetBrains" class="logo-footer" width="130" align="left"></a>
  <br/>
  
  Le Jetbrains™ ReSharper est fortement utilisé lors du développement de ce projet. Merci à JetBrains s.r.o pour avoir fourni la [JetBrains Open Source License](https://www.jetbrains.com/community/opensource/#support), Si vous êtes un des développeurs passionnés qui utilisent souvent les produits JetBrains, vous pouvez essayer d'appliquer la JetBrains Open Source License depuis le [canal officiel](https://www.jetbrains.com/shop/eform/opensource) pour vous aider, vous et vos coéquipiers développeurs, à améliorer significativement les productivités.

</div>
