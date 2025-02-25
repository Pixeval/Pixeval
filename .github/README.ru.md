<div align="center">

<img src="../src/Pixeval/Assets/Images/logo.svg" alt="logo" width="200">

# Pixeval

Мощное, быстрое и красивое стороннее настольное приложение Pixiv на базе .NET 8 и WinUI 3

[<img src="https://get.microsoft.com/images/ru%20dark.svg" width="200"/>](https://apps.microsoft.com/detail/Pixeval/9p1rzl9z8454?launch=true\&mode=mini)

![](https://img.shields.io/github/stars/Pixeval/Pixeval?color=red\&style=for-the-badge\&logo=data:image/svg+xml;charset=utf-8;base64,PHN2ZyB3aWR0aD0iNDgiIGhlaWdodD0iNDgiIHZpZXdCb3g9IjAgMCA0OCA0OCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHBhdGggZD0iTTIxLjgwMyA2LjA4NTQ0QzIyLjcwMTcgNC4yNjQ0OSAyNS4yOTgzIDQuMjY0NDggMjYuMTk3IDYuMDg1NDRMMzEuMDQ5MyAxNS45MTc0TDQxLjg5OTYgMTcuNDk0QzQzLjkwOTEgMTcuNzg2IDQ0LjcxMTUgMjAuMjU1NiA0My4yNTc0IDIxLjY3M0wzNS40MDYxIDI5LjMyNjFMMzcuMjU5NSA0MC4xMzI1QzM3LjYwMjggNDIuMTMzOSAzNS41MDIxIDQzLjY2MDIgMzMuNzA0NyA0Mi43MTUyTDI0IDM3LjYxMzJMMTQuMjk1MiA0Mi43MTUyQzEyLjQ5NzggNDMuNjYwMiAxMC4zOTcxIDQyLjEzMzkgMTAuNzQwNCA0MC4xMzI1TDEyLjU5MzggMjkuMzI2MUw0Ljc0MjU1IDIxLjY3M0MzLjI4ODQzIDIwLjI1NTYgNC4wOTA4MyAxNy43ODYgNi4xMDAzNyAxNy40OTRMMTYuOTUwNiAxNS45MTc0TDIxLjgwMyA2LjA4NTQ0WiIgZmlsbD0iI2ZmZmZmZiIvPgo8L3N2Zz4K)
![](https://img.shields.io/static/v1?label=contact%20me\&message=hotmail\&color=green\&style=for-the-badge\&logo=gmail\&logoColor=white)
[![](https://img.shields.io/static/v1?label=chatting\&message=qq\&color=blue\&style=for-the-badge\&logo=qq\&logoColor=white)](https://jq.qq.com/?_wv=1027\&k=5hGmJbQ)
[![](https://img.shields.io/github/license/Pixeval/Pixeval?style=for-the-badge\&logo=gnu\&logoColor=white)](https://github.com/Pixeval/Pixeval/blob/main/LICENSE)
[![](https://img.shields.io/static/v1?label=feedback\&message=issues\&color=pink\&style=for-the-badge\&logo=Github\&logoColor=white)](https://github.com/Pixeval/Pixeval/issues/new/choose)
[![](https://img.shields.io/static/v1?label=runtime\&message=.NET%208.0\&color=yellow\&style=for-the-badge\&logo=.NET\&logoColor=white)](https://dotnet.microsoft.com/download/dotnet/8.0)
![](https://img.shields.io/badge/Platform-Windows10.0.19041-512BD4?\&style=for-the-badge\&logo=data:image/svg+xml;charset=utf-8;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI0OCIgaGVpZ2h0PSI0OCI+CjxwYXRoIGQ9Ik00LjggMy44NGEuOTYuOTYgMCAwIDAtLjk2Ljk2djE4LjI0aDE5LjJWMy44NFptMjAuMTYgMHYxOS4yaDE5LjJWNC44YS45Ni45NiAwIDAgMC0uOTYtLjk2Wk0zLjg0IDI0Ljk2VjQzLjJjMCAuNTMuNDMuOTYuOTYuOTZoMTguMjR2LTE5LjJabTIxLjEyIDB2MTkuMkg0My4yYS45Ni45NiAwIDAgMCAuOTYtLjk2VjI0Ljk2Wm0wIDAiIGZpbGw9IiNmZmZmZmYiLz4KPC9zdmc+)

</div>

🌏: [简体中文](README.md)，[English](README.en.md)，[**Русский**](README.ru.md)，[Français](README.fr.md)

---

**Pixeval на базе WinUI 3 сейчас находится в разработке. Старая версия на базе WPF устарела и больше не получает поддержку от разработчиков.**\*\*

> 仅支持 Windows 10（版本 2004 - 内部版本 19041）及更高版本。
> 可以通过以下步骤查看。右键点击“开始”按钮，选择然后选择系统；或者在“设置”中，依次选择“系统”>“系统信息”，此时页面中的Windows规格下可以看到相关信息。

Для получения дополнительной информации см. [главную страницу](https://sora.ink/pixeval/)

**Версия на базе WinUI 3 предлагает лучший пользовательский интерфейс,
более структурированную кодовую базу и современный опыт разработки по сравнению с версией на базе WPF.
Вы можете скачать и скомпилировать её самостоятельно, если хотите взглянуть на новую версию,
следуйте следующим шагам для компиляции и запуска:**

## Предварительные требования

1. 拥有[git](https://git-scm.com)环境
2. Установите [Visual Studio 2022](https://visualstudio.microsoft.com/vs) (Roslyn 4.x требует VS17.x, то есть VS2022).
   如果已安装请确认是VS2022的最新版本，因为负载里.NET SDK的版本和VS的版本有关，低版本可能不包含.NET8 SDK。
3. В **Инструменты → Получить инструменты и функции**, в разделе **Рабочие нагрузки**, выберите разработку для .NET Desktop (В панели подробностей установки диалогового окна установки выберите шаблон Windows App SDK C# в нижней части списка, хотя это и не обязательно). Смотрите [Установка инструментов для Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/set-up-your-development-environment).（可以参考[安装适用于 Windows 应用 SDK 的工具](https://learn.microsoft.com/windows/apps/windows-app-sdk/set-up-your-development-environment)）

## 运行项目

1. 用Git克隆本项目
2. Если _Pixeval_ не установлен как стартовый проект, сделайте его таковым.
3. 构建并运行

- Если возникают проблемы, попробуйте пересобрать решение или перезапустить Visual Studio 2022.

## 参与开发的要求

1. Базовые знания о Windows XAML Framework, для получения дополнительной информации см. [Обзор XAML](https://docs.microsoft.com/windows/uwp/xaml-platform/xaml-overview)
2. Глубокое понимание C# и разработки на .NET
3. Чтение исходного кода без документации

## Руководство по контролю версий

Этот проект следует простой, но разумной модели ветвления: когда вы хотите внести свой вклад в код, пожалуйста, создайте новую ветку на основе `main` и работайте с ней. Новая ветка **ДОЛЖНА** следовать формату `{user}/{qualifier}/{desc}`, где `{user}` — ваше имя пользователя на GitHub.

| Содержание кода               | qualifier | desc                                 |
| ----------------------------- | --------- | ------------------------------------ |
| Исправления ошибок            | fix       | Краткое описание уязвимости          |
| Новые функции                 | feature   | Краткое описание новой функции       |
| Рефакторинг или качество кода | refactor  | Краткое описание секции рефакторинга |

Если ваш вклад содержит более одного типа, указанного выше, выберите правило, которое наиболее релевантно вашему вкладу, и укажите остальные в сообщении коммита.

После завершения разработки, пожалуйста, создайте [Pull Request](https://github.com/Pixeval/Pixeval/pulls) и запросите слияние вашей ветки в `main`

## Структура проекта

1. Pixeval 项目包含了项目本身的逻辑及布局代码
2. Проект _Pixeval.Controls_ включает в себя ряд слабо связанных контролов.
3. Проект _Pixeval.CoreApi_ содержит точки API, необходимые для проекта.
4. Проект _Pixeval.SourceGen_ содержит генераторы кода для настроек.
5. Проект _Pixeval.Utilities_ содержит коды для универсальных функций утилит.

## 反馈问题（按照推荐程度优先级排序）

1. Откройте issue на [GitHub](https://github.com/dylech30th/Pixeval/issues/new/choose)
2. Отправьте email на [decem0730@hotmail.com](mailto:decem0730@hotmail.com)
3. Присоединитесь к группе QQ 815791942 и задайте вопрос разработчикам напрямую

## 鸣谢（排名不分先后）

[![Toolkit Contributors](https://contrib.rocks/image?repo=Pixeval/Pixeval)](https://github.com/Pixeval/Pixeval/graphs/contributors)

Made with [contrib.rocks](https://contrib.rocks).

## 支持作者

Если этот проект полностью соответствует вашим требованиям, добро пожаловать угостить меня кофе на [afdian](https://afdian.net/@dylech30th). Буду рад вашей поддержке. Спасибо!

<div>
  <a href="https://www.jetbrains.com/?from=Pixeval" align="right"><img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg" alt="JetBrains" class="logo-footer" width="150" align="left"></a>
  <br/>

В разработке этого проекта активно используется Jetbrains™ ReSharper. Благодарим компанию JetBrains s.r.o. за предоставление [Лицензии на открытый исходный код JetBrains](https://www.jetbrains.com/community/opensource/#support). Если вы один из увлеченных разработчиков, которые часто используют продукты JetBrains, вы можете попробовать подать заявку на Лицензию на открытый исходный код JetBrains через [официальный канал](https://www.jetbrains.com/shop/eform/opensource), чтобы помочь себе и вашим коллегам-разработчикам значительно повысить продуктивность.

</div>
