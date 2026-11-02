using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentIcons.Common;
using Mako.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.AppManagement;
using Pixeval.Extensions.Common;
using Pixeval.Extensions.Common.Commands.Transformers;
using Pixeval.I18N;
using Pixeval.Models.Extensions;
using Pixeval.Utilities;
using Pixeval.ViewModels;
using Pixeval.ViewModels.Viewers;

namespace Pixeval.Tests;

[TestClass]
[DoNotParallelize]
public sealed class NovelTranslationTest
{
    [TestMethod]
    public async Task FullTextTranslationPreservesPagesAndCachesBothDirections()
    {
        var translator = new TestTranslator();
        using var fixture = new TranslationFixture(translator, ["first", "second"]);
        var model = fixture.Model;
        model.CurrentPageIndex = 1;

        await model.ToggleTranslationCommand.ExecuteAsync(null);

        Assert.IsTrue(model.IsTranslationShown);
        Assert.AreEqual("translated:second", model.CurrentMarkdown);
        Assert.AreEqual(2, model.PageCount);
        model.CurrentPageIndex = 0;
        Assert.AreEqual("translated:first", model.CurrentMarkdown);
        await model.ToggleTranslationCommand.ExecuteAsync(null);
        Assert.AreEqual("first", model.CurrentMarkdown);
        await model.ToggleTranslationCommand.ExecuteAsync(null);
        Assert.AreEqual("translated:first", model.CurrentMarkdown);
        CollectionAssert.AreEqual(new[] { "first", "second" }, translator.Inputs);
    }

    [TestMethod]
    [DataRow("cancel")]
    [DataRow("change-novel")]
    [DataRow("dispose")]
    public async Task LateTranslationCannotReplaceOriginalAfterCancellation(string action)
    {
        var translator = new TestTranslator { HoldResult = true };
        using var fixture = new TranslationFixture(translator, ["first", "second"]);
        var model = fixture.Model;
        var translation = model.ToggleTranslationCommand.ExecuteAsync(null);
        Assert.IsTrue(model.ToggleTranslationCommand.IsRunning);
        Assert.AreEqual("first", model.CurrentMarkdown);

        switch (action)
        {
            case "cancel":
                model.ToggleTranslationCommand.Cancel();
                break;
            case "change-novel":
                _ = typeof(NovelViewerPageViewModel).GetMethod("ResetLoadingToken", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .Invoke(model, null);
                SetField(model, "_pageMarkdowns", new List<string> { "new novel" });
                break;
            case "dispose":
                model.Dispose();
                break;
        }

        await translation.WaitAsync(TimeSpan.FromSeconds(5));
        translator.Pending!.SetCompleted();
        Assert.IsFalse(model.IsTranslationShown);
        Assert.IsFalse(model.ToggleTranslationCommand.IsRunning);
        Assert.AreEqual(action is "change-novel" ? "new novel" : "first", model.CurrentMarkdown);
        Assert.HasCount(1, translator.Inputs);
    }

    [TestMethod]
    public async Task ExtensionErrorTextIsDisplayedAndOriginalRemainsAvailable()
    {
        using var fixture = new TranslationFixture(new TestTranslator { Result = "API quota exceeded" }, ["original"]);

        await fixture.Model.ToggleTranslationCommand.ExecuteAsync(null);

        Assert.AreEqual("API quota exceeded", fixture.Model.CurrentMarkdown);
        await fixture.Model.ToggleTranslationCommand.ExecuteAsync(null);
        Assert.AreEqual("original", fixture.Model.CurrentMarkdown);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task FailedTranslationKeepsOriginalAndAllowsRetry(bool throwException)
    {
        var translator = new TestTranslator { Result = "", ThrowException = throwException };
        using var fixture = new TranslationFixture(translator, ["original"]);

        await fixture.Model.ToggleTranslationCommand.ExecuteAsync(null);

        Assert.AreEqual("original", fixture.Model.CurrentMarkdown);
        Assert.IsFalse(fixture.Model.IsTranslationShown);
        Assert.IsTrue(fixture.Model.ToggleTranslationCommand.CanExecute(null));
        translator.ThrowException = false;
        translator.Result = "translation";
        await fixture.Model.ToggleTranslationCommand.ExecuteAsync(null);
        Assert.AreEqual("translation", fixture.Model.CurrentMarkdown);
    }

    [TestMethod]
    public async Task MissingTranslatorHidesButtonWithoutReportingError()
    {
        using var fixture = new TranslationFixture(null, ["original"]);

        Assert.IsFalse(fixture.Model.HasTranslator);
        Assert.IsFalse(fixture.Model.ToggleTranslationCommand.CanExecute(null));

        await fixture.Model.ToggleTranslationCommand.ExecuteAsync(null);

        Assert.AreEqual("original", fixture.Model.CurrentMarkdown);
        Assert.IsFalse(fixture.Model.IsTranslationShown);
    }

    [TestMethod]
    public void DisabledTranslatorHidesButton()
    {
        using var fixture = new TranslationFixture(new TestTranslator(), ["original"]);
        var extensions = App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>();
        Assert.IsTrue(fixture.Model.HasTranslator);
        extensions.HostModels[0].IsActive = false;
        Assert.IsFalse(fixture.Model.HasTranslator);
        Assert.IsFalse(fixture.Model.ToggleTranslationCommand.CanExecute(null));
        extensions.HostModels[0].IsActive = true;
        Assert.IsTrue(fixture.Model.HasTranslator);
        Assert.IsTrue(fixture.Model.ToggleTranslationCommand.CanExecute(null));
    }

    private static void SetField(object target, string name, object value) => target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);

    private sealed class TranslationFixture : IDisposable
    {
        private readonly PropertyInfo _appProperty = typeof(App).GetProperty(nameof(App.AppViewModel))!;
        private readonly object? _previousApp = App.AppViewModel;
        private readonly ExtensionService _extensions = new(null!, [], [], false);
        private readonly ServiceProvider _services;

        public NovelViewerPageViewModel Model { get; } = new(null!, -1, false);

        public TranslationFixture(TestTranslator? translator, List<string> pages)
        {
            I18NManager.Register(new JsonMarkdownLangPlugin(), LanguageHelper.DefaultLanguage);
            // Supply loaded content and a managed extension without starting network/database services or loading a native DLL.
            var novel = (NovelItemViewModel) RuntimeHelpers.GetUninitializedObject(typeof(NovelItemViewModel));
            typeof(EntryViewModel<Novel>).GetField("<Entry>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(novel, Novel.CreateDefault());
            typeof(NovelViewerPageViewModel).GetProperty(nameof(Model.CurrentNovel))!.SetValue(Model, novel);
            SetField(Model, "_pageMarkdowns", pages);
            if (translator is not null)
            {
                var host = (ExtensionsHostModel) RuntimeHelpers.GetUninitializedObject(typeof(ExtensionsHostModel));
                SetField(host, "<Values>k__BackingField", new Dictionary<string, object?>());
                SetField(host, "<Extensions>k__BackingField", new IExtension[] { translator });
                _extensions.HostModels.Add(host);
            }

            var logger = new FileLogger(System.IO.Path.GetTempPath());
            logger.Logging += (_, _) => true;
            _services = new ServiceCollection().AddSingleton(_extensions).AddSingleton(logger).BuildServiceProvider();
            var app = (AppViewModel) RuntimeHelpers.GetUninitializedObject(typeof(AppViewModel));
            typeof(AppViewModel).GetProperty(nameof(AppViewModel.AppServiceProvider))!.SetValue(app, _services);
            _appProperty.SetValue(null, app);
        }

        public void Dispose()
        {
            Model.Dispose();
            _appProperty.SetValue(null, _previousApp);
            _extensions.HostModels.Clear();
            _services.Dispose();
            _extensions.Dispose();
        }
    }

    private sealed class TestTranslator : ITextTransformerCommandExtension
    {
        public List<string> Inputs { get; } = [];
        public bool HoldResult { get; init; }
        public bool ThrowException { get; set; }
        public string? Result { get; set; }
        public ITaskCompletionSource? Pending { get; private set; }

        public void TransformAsync(ITaskCompletionSource task, string originalString, TextTransformerType type)
        {
            Assert.AreEqual(TextTransformerType.Novel, type);
            Inputs.Add(originalString);
            if (ThrowException)
                throw new InvalidOperationException("Translator unavailable");
            Pending = task;
            if (!HoldResult)
                task.SetCompleted();
        }

        public string GetTransformResult() => Result ?? "translated:" + Inputs[^1];
        public Symbol GetIcon() => Symbol.Translate;
        public string GetLabel() => "Test translator";
        public string GetDescription() => GetLabel();
        public void OnExtensionLoaded() { }
        public void OnExtensionUnloaded() { }
    }
}
