<div align="center">

<img src="../src/Pixeval/Assets/Images/logo.svg" alt="logo" width="200">

# Pixeval

Мощное, быстрое и красивое стороннее настольное приложение Pixiv на базе .NET 8 и WinUI 3

[<img src="https://get.microsoft.com/images/ru%20dark.svg" width="200"/>](https://apps.microsoft.com/detail/Pixeval/9p1rzl9z8454?launch=true&mode=mini)

![](https://img.shields.io/github/stars/Pixeval/Pixeval?color=red&style=for-the-badge&logo=data:image/svg+xml;charset=utf-8;base64,PHN2ZyB3aWR0aD0iNDgiIGhlaWdodD0iNDgiIHZpZXdCb3g9IjAgMCA0OCA0OCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHBhdGggZD0iTTIxLjgwMyA2LjA4NTQ0QzIyLjcwMTcgNC4yNjQ0OSAyNS4yOTgzIDQuMjY0NDggMjYuMTk3IDYuMDg1NDRMMzEuMDQ5MyAxNS45MTc0TDQxLjg5OTYgMTcuNDk0QzQzLjkwOTEgMTcuNzg2IDQ0LjcxMTUgMjAuMjU1NiA0My4yNTc0IDIxLjY3M0wzNS40MDYxIDI5LjMyNjFMMzcuMjU5NSA0MC4xMzI1QzM3LjYwMjggNDIuMTMzOSAzNS41MDIxIDQzLjY2MDIgMzMuNzA0NyA0Mi43MTUyTDI0IDM3LjYxMzJMMTQuMjk1MiA0Mi43MTUyQzEyLjQ5NzggNDMuNjYwMiAxMC4zOTcxIDQyLjEzMzkgMTAuNzQwNCA0MC4xMzI1TDEyLjU5MzggMjkuMzI2MUw0Ljc0MjU1IDIxLjY3M0MzLjI4ODQzIDIwLjI1NTYgNC4wOTA4MyAxNy43ODYgNi4xMDAzNyAxNy40OTRMMTYuOTUwNiAxNS45MTc0TDIxLjgwMyA2LjA4NTQ0WiIgZmlsbD0iI2ZmZmZmZiIvPgo8L3N2Zz4K)
![](https://img.shields.io/static/v1?label=contact%20me&message=hotmail&color=green&style=for-the-badge&logo=gmail&logoColor=white)




![](https://img.shields.io/badge/Platform-Windows10.0.19041-512BD4?&style=for-the-badge&logo=data:image/svg+xml;charset=utf-8;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI0OCIgaGVpZ2h0PSI0OCI+CjxwYXRoIGQ9Ik00LjggMy44NGEuOTYuOTYgMCAwIDAtLjk2Ljk2djE4LjI0aDE5LjJWMy44NFptMjAuMTYgMHYxOS4yaDE5LjJWNC44YS45Ni45NiAwIDAgMC0uOTYtLjk2Wk0zLjg0IDI0Ljk2VjQzLjJjMCAuNTMuNDMuOTYuOTYuOTZoMTguMjR2LTE5LjJabTIxLjEyIDB2MTkuMkg0My4yYS45Ni45NiAwIDAgMCAuOTYtLjk2VjI0Ljk2Wm0wIDAiIGZpbGw9IiNmZmZmZmYiLz4KPC9zdmc+)

</div>

🌏: [简体中文](README.md)，[English](README.en.md)，[**Русский**](README.ru.md)，[Français](README.fr.md)

---

**Pixeval на базе WinUI 3 сейчас находится в разработке. Старая версия на базе WPF устарела и больше не получает поддержку от разработчиков.**

> Кодовая база Pixeval для WinUI 3 поддерживается только на Windows 10 (2004, номер сборки 19041) и выше.
> Вы можете просмотреть её, выполнив следующие шаги.Щелкните правой кнопкой мыши по кнопке «Пуск», затем выберите «Система»; или в «Настройках» последовательно выберите «Система» > «О системе». Соответствующая информация будет отображена в разделе «Характеристики Windows».

Для получения дополнительной информации см. [главную страницу](https://sora.ink/pixeval/)

**Версия на базе WinUI 3 предлагает лучший пользовательский интерфейс,
более структурированную кодовую базу и современный опыт разработки по сравнению с версией на базе WPF.
Вы можете скачать и скомпилировать её самостоятельно, если хотите взглянуть на новую версию,
следуйте следующим шагам для компиляции и запуска:**

## Предварительные требования

1. Установленный [git](https://git-scm.com)
2. Установите [Visual Studio 2022](https://visualstudio.microsoft.com/vs) (Roslyn 4.x требует VS17.x, то есть VS2022).
  Если Visual Studio 2022 уже установлена, убедитесь, что это последняя версия, так как версия .NET SDK в рабочих нагрузках зависит от версии VS, и более старые версии могут не включать .NET 8 SDK.
3. В **Инструменты → Получить инструменты и функции**, в разделе **Рабочие нагрузки**, выберите разработку для .NET Desktop (В панели подробностей установки диалогового окна установки выберите шаблон Windows App SDK C# в нижней части списка, хотя это и не обязательно). Смотрите [Установка инструментов для Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/set-up-your-development-environment).

## Разработка

1. Клонируйте проект.
2. Если _Pixeval_ не установлен как стартовый проект, сделайте его таковым.
3. Соберите и запустите.

- Если возникают проблемы, попробуйте пересобрать решение или перезапустить Visual Studio 2022.

## Если вы хотите принять участие в разработке, есть несколько дополнительных требований

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

1. Проект _Pixeval_ содержит наиболее релевантные коды и файлы пакета.
2. Проект _Pixeval.Controls_ включает в себя ряд слабо связанных контролов.
3. Проект _Pixeval.CoreApi_ содержит точки API, необходимые для проекта.
4. Проект _Pixeval.SourceGen_ содержит генераторы кода для настроек.
5. Проект _Pixeval.Utilities_ содержит коды для универсальных функций утилит.

## Если у вас возникли проблемы... (Упорядочено по приоритету рекомендаций)

1. Откройте issue на [GitHub](https://github.com/dylech30th/Pixeval/issues/new/choose)
2. Отправьте email на [decem0730@hotmail.com](mailto:decem0730@hotmail.com)
3. Присоединитесь к группе QQ 815791942 и задайте вопрос разработчикам напрямую

## Благодарности (Без определенного порядка)

[![Toolkit Contributors](https://contrib.rocks/image?repo=Pixeval/Pixeval)](https://github.com/Pixeval/Pixeval/graphs/contributors)

Made with [contrib.rocks](https://contrib.rocks).

## Поддержите меня

Если этот проект полностью соответствует вашим требованиям, добро пожаловать угостить меня кофе на [afdian](https://afdian.net/@dylech30th). Буду рад вашей поддержке. Спасибо!

<div>
  <a href="https://www.jetbrains.com/?from=Pixeval" align="right"><img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.svg" alt="JetBrains" class="logo-footer" width="150" align="left"></a>
  <br/>

В разработке этого проекта активно используется Jetbrains™ ReSharper. Благодарим компанию JetBrains s.r.o. за предоставление [Лицензии на открытый исходный код JetBrains](https://www.jetbrains.com/community/opensource/#support). Если вы один из увлеченных разработчиков, которые часто используют продукты JetBrains, вы можете попробовать подать заявку на Лицензию на открытый исходный код JetBrains через [официальный канал](https://www.jetbrains.com/shop/eform/opensource), чтобы помочь себе и вашим коллегам-разработчикам значительно повысить продуктивность.

</div>
