// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using Microsoft.Extensions.DependencyInjection;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Commands.Transformers;
using Pixeval.I18N;
using Pixeval.Models.Blocking;
using Pixeval.Models.Extensions;
using Pixeval.Utilities;

namespace Pixeval.ViewModels.Viewers;

public sealed partial class NovelViewerPageViewModel
{
    private List<string>? _translatedPageMarkdowns;

    public bool HasTranslator => App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>()
        .ActiveTextTransformerCommands.Any();

    public IReadOnlyList<TextTransformerExtensionCommandItem> TransformerExtensionItems =>
    [
        .. App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>()
            .ActiveTextTransformerCommands.Select(extension => new TextTransformerExtensionCommandItem(extension, ToggleTranslationCommand))
    ];

    internal void RefreshTranslationAvailability()
    {
        OnPropertyChanged(nameof(HasTranslator));
        OnPropertyChanged(nameof(TransformerExtensionItems));
        ToggleTranslationCommand.NotifyCanExecuteChanged();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentMarkdown))]
    [NotifyPropertyChangedFor(nameof(TranslationButtonText))]
    public partial bool IsTranslationShown { get; private set; }

    public string TranslationButtonText => I18NManager.GetResource(IsTranslationShown
        ? NovelViewerPageResources.ShowOriginal
        : _translatedPageMarkdowns is not null
            ? NovelViewerPageResources.ShowTranslation
            : NovelViewerPageResources.TranslateFullText);

    private bool CanTranslate => !_disposed && !IsLoading && LoadErrorMessage is null && PageCount > 0 && HasTranslator
        && CurrentNovel is { } novel && !BlockedContentHelper.IsBlockedPlaceholder(novel.Entry);

    [RelayCommand(CanExecute = nameof(CanTranslate))]
    private async Task ToggleTranslationAsync(Control? control, CancellationToken token)
    {
        if (!CanTranslate)
            return;

        var translator = control?.Tag as ITextTransformerCommandExtension;
        if (translator is null && _translatedPageMarkdowns is not null)
        {
            IsTranslationShown = !IsTranslationShown;
            return;
        }

        var translators = App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>()
            .ActiveTextTransformerCommands;
        translator ??= translators.FirstOrDefault();
        if (translator is null || !translators.Contains(translator))
            return;

        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(token, _loadingCts.Token);
        token = cancellation.Token;
        var viewContainer = control is null ? null : TopLevel.GetTopLevel(control)?.ViewContainer;
        try
        {
            viewContainer?.ShowInformation(I18NManager.GetResource(NovelViewerPageResources.ApplyingTransformerExtensions));
            var pages = _pageMarkdowns;
            var translatedPages = new List<string>(pages.Count);
            foreach (var page in pages)
            {
                token.ThrowIfCancellationRequested();
                // 扩展没有取消参数；停止等待并在写回前检查，避免切换小说后显示旧译文。
                var translated = string.IsNullOrWhiteSpace(page) ? page :
                    await translator.TransformAsync(page, TextTransformerType.Novel).WaitAsync(token);
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(translated) && !string.IsNullOrWhiteSpace(page))
                {
                    viewContainer?.ShowError(I18NManager.GetResource(NovelViewerPageResources.TransformerExtensionFailed));
                    return;
                }

                translatedPages.Add(translated);
            }

            // 完整翻译成功后再切换显示，取消或异常时仍可继续阅读原文。
            _translatedPageMarkdowns = translatedPages;
            if (IsTranslationShown)
                OnPropertyChanged(nameof(CurrentMarkdown));
            else
                IsTranslationShown = true;
            viewContainer?.ShowSuccess(I18NManager.GetResource(NovelViewerPageResources.TransformerExtensionFinishedSuccessfully));
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception e)
        {
            if (token.IsCancellationRequested)
                return;

            viewContainer?.ShowError(I18NManager.GetResource(NovelViewerPageResources.TransformerExtensionFailed), e.Message);
            App.AppViewModel.AppServiceProvider.GetRequiredService<FileLogger>()
                .LogError(nameof(NovelViewerPageViewModel), e);
        }
    }
}

public sealed class TextTransformerExtensionCommandItem(ITextTransformerCommandExtension extension, IAsyncRelayCommand<Control?> command)
{
    public ITextTransformerCommandExtension Extension { get; } = extension;

    public string Label { get; } = extension.Label;

    public string Description { get; } = extension.Description;

    public Symbol Symbol { get; } = extension.Icon;

    public IAsyncRelayCommand<Control?> Command { get; } = command;
}
