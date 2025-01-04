using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Pixeval.Extensions;

namespace Pixeval.Pages.Misc;

public sealed partial class ExtensionsPage : Page
{
    private IReadOnlyList<ExtensionsHostModel> Models =>
        App.AppViewModel.AppServiceProvider.GetRequiredService<ExtensionService>().ExtensionHosts;

    public ExtensionsPage()
    {
        InitializeComponent();
    }
}
