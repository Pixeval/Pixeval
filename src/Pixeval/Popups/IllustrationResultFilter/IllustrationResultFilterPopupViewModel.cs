#region Copyright (c) Pixeval/Pixeval

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/IllustrationResultFilterPopupViewModel.cs
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
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pixeval.Misc;
using Pixeval.UserControls.TokenInput;

namespace Pixeval.Popups.IllustrationResultFilter;

public partial class IllustrationResultFilterPopupViewModel : ObservableObject
{
    [DefaultValue(typeof(ObservableCollection<Token>))] [ObservableProperty] private ObservableCollection<Token> _excludeTags;

    [DefaultValue("")] [ObservableProperty] private string _illustrationId;

    [DefaultValue(typeof(Token))] [ObservableProperty] private Token _illustrationName;

    [DefaultValue("")] [ObservableProperty] private string _illustratorId;

    [DefaultValue(typeof(Token))] [ObservableProperty] private Token _illustratorName;

    [DefaultValue(typeof(ObservableCollection<Token>))] [ObservableProperty] private ObservableCollection<Token> _includeTags;

    [DefaultValue(0)] [ObservableProperty] private int _leastBookmark;

    [DefaultValue(int.MaxValue)] [ObservableProperty] private int _maximumBookmark;

    [DefaultValue(typeof(MaxDateTimeOffSetDefaultValueProvider))] [ObservableProperty] private DateTimeOffset _publishDateEnd;

    [DefaultValue(typeof(MinDateTimeOffSetDefaultValueProvider))] [ObservableProperty] private DateTimeOffset _publishDateStart;

    [DefaultValue(typeof(ObservableCollection<Token>))] [ObservableProperty] private ObservableCollection<Token> _userGroupName;
#pragma warning disable CS8618
    public IllustrationResultFilterPopupViewModel()
#pragma warning restore CS8618
    {
        DefaultValueAttributeHelper.Initialize(this);
    }
}