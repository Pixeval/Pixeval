using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Controls.Windowing;
using WinUI3Utilities;

namespace Pixeval.Controls.DialogContent;

public sealed partial class CertificateRequiredDialog : UserControl
{
    public CertificateRequiredDialog() => InitializeComponent();

    public X509Certificate2? X509Certificate2 { get; private set; }

    public bool CheckCertificate()
    {
        try
        {
            X509Certificate2 = new X509Certificate2(PathBox.Text, PasswordBox.Password, X509KeyStorageFlags.UserKeySet);
            return true;
        }
        catch
        {
            InfoBar.IsOpen = true;
            return false;
        }
    }

    private async void PathBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e)
    {
        if (await WindowFactory.GetWindowForElement(this).PickSingleFileAsync() is { } file)
            PathBox.Text = file.Path;
    }
}
