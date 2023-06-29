#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2022 Pixeval/MainWindowViewModel.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media.Animation;
using Pixeval.Messages;
using Pixeval.Pages;
using Pixeval.Pages.IllustrationViewer;
using Pixeval.Util.UI;
using IllustrationViewModel = Pixeval.UserControls.IllustrationView.IllustrationViewModel;
using WinUI3Utilities;

namespace Pixeval;

public partial class MainWindowViewModel : AutoActivateObservableRecipient, IRecipient<LoginCompletedMessage>
{

    [ObservableProperty]
    private bool _showTitleBar;

    public bool TitleBarSearchBarEnabled => AppWindowTitleBar.IsCustomizationSupported();

    public SuggestionStateMachine SuggestionProvider { get; } = new();

    public void Receive(LoginCompletedMessage message)
    {
        ShowTitleBar = true;
    }
}
