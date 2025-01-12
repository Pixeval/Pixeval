// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pixeval.Controls;

public static partial class Growl
{
    public static void Info(string title) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Severity = InfoBarSeverity.Informational
    });

    public static void InfoGrowl(this FrameworkElement token, string title) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Severity = InfoBarSeverity.Informational,
        Token = GetToken(token)
    });

    public static InfoBar? InfoGrowlReturn(this FrameworkElement token, string title) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Severity = InfoBarSeverity.Informational,
        Token = GetToken(token),
        StaysOpen = true
    });

    public static void Info(string title, string message) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Message = message,
        Severity = InfoBarSeverity.Informational
    });

    public static void InfoGrowl(this FrameworkElement token, string title, string message) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Message = message,
        Severity = InfoBarSeverity.Informational,
        Token = GetToken(token)
    });

    public static void Success(string title) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Severity = InfoBarSeverity.Success
    });

    public static void SuccessGrowl(this FrameworkElement token, string title) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Severity = InfoBarSeverity.Success,
        Token = GetToken(token)
    });

    public static void Success(string title, string message) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Message = message,
        Severity = InfoBarSeverity.Success
    });

    public static void SuccessGrowl(this FrameworkElement token, string title, string message) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Message = message,
        Severity = InfoBarSeverity.Success,
        Token = GetToken(token)
    });

    public static void Warning(string title) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Severity = InfoBarSeverity.Warning,
        StaysOpen = true
    });

    public static void WarningGrowl(this FrameworkElement token, string title) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Severity = InfoBarSeverity.Warning,
        Token = GetToken(token),
        StaysOpen = true
    });

    public static void Warning(string title, string message) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Message = message,
        Severity = InfoBarSeverity.Warning,
        StaysOpen = true
    });

    public static void WarningGrowl(this FrameworkElement token, string title, string message) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Message = message,
        Severity = InfoBarSeverity.Warning,
        Token = GetToken(token),
        StaysOpen = true
    });

    public static void Error(string title) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Severity = InfoBarSeverity.Error,
        StaysOpen = true
    });

    public static void ErrorGrowl(this FrameworkElement token, string title) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Severity = InfoBarSeverity.Error,
        Token = GetToken(token),
        StaysOpen = true
    });

    public static void Error(string title, string message) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Message = message,
        Severity = InfoBarSeverity.Error,
        StaysOpen = true
    });

    public static void ErrorGrowl(this FrameworkElement token, string title, string message) => InitGrowl(new GrowlInfo
    {
        Title = title,
        Message = message,
        Severity = InfoBarSeverity.Error,
        Token = GetToken(token),
        StaysOpen = true
    });

    public static void Show(GrowlInfo growlInfo) => InitGrowl(growlInfo);

    public static async void RemoveSuccessGrowlAfterDelay(this FrameworkElement token, InfoBar growl, string? title = null, string? message = null)
    {
        growl.Severity = InfoBarSeverity.Success;
        if (title is not null)
            growl.Title = title;
        if (message is not null)
            growl.Message = message;
        await Task.Delay(3000);
        RemoveGrowl(token, growl);
    }
}
