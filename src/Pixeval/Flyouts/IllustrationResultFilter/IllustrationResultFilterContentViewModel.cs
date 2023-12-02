#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationResultFilterContentViewModel.cs
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
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Attributes;
using Pixeval.Misc;
using Pixeval.Controls.TokenInput;

namespace Pixeval.Flyouts.IllustrationResultFilter;

public class IllustrationResultFilterContentViewModel : ObservableObject
{
    private ObservableCollection<Token> _excludeTags;

    private string _illustrationId;

    private Token _illustrationName;

    private string _illustratorId;

    private Token _illustratorName;

    private ObservableCollection<Token> _includeTags;

    private int _leastBookmark;

    private int _maximumBookmark;

    private DateTimeOffset _publishDateEnd;

    private DateTimeOffset _publishDateStart;

    private ObservableCollection<Token> _userGroupName;
#pragma warning disable CS8618
    public IllustrationResultFilterContentViewModel()
#pragma warning restore CS8618
    {
        DefaultValueAttributeHelper.Initialize(this);
    }

    [DefaultValue(typeof(ObservableCollection<Token>))]
    public ObservableCollection<Token> IncludeTags
    {
        get => _includeTags;
        set => SetProperty(ref _includeTags, value);
    }

    [DefaultValue(typeof(ObservableCollection<Token>))]
    public ObservableCollection<Token> ExcludeTags
    {
        get => _excludeTags;
        set => SetProperty(ref _excludeTags, value);
    }

    [DefaultValue(0)]
    public int LeastBookmark
    {
        get => _leastBookmark;
        set => SetProperty(ref _leastBookmark, value);
    }

    [DefaultValue(int.MaxValue)]
    public int MaximumBookmark
    {
        get => _maximumBookmark;
        set => SetProperty(ref _maximumBookmark, value);
    }

    [DefaultValue(typeof(ObservableCollection<Token>))]
    public ObservableCollection<Token> UserGroupName
    {
        get => _userGroupName;
        set => SetProperty(ref _userGroupName, value);
    }

    [DefaultValue(typeof(Token))]
    public Token IllustratorName
    {
        get => _illustratorName;
        set => SetProperty(ref _illustratorName, value);
    }

    [DefaultValue("")]
    public string IllustratorId
    {
        get => _illustratorId;
        set => SetProperty(ref _illustratorId, value);
    }

    [DefaultValue(typeof(Token))]
    public Token IllustrationName
    {
        get => _illustrationName;
        set => SetProperty(ref _illustrationName, value);
    }

    [DefaultValue("")]
    public string IllustrationId
    {
        get => _illustrationId;
        set => SetProperty(ref _illustrationId, value);
    }

    [DefaultValue(typeof(MinDateTimeOffSetDefaultValueProvider))]
    public DateTimeOffset PublishDateStart
    {
        get => _publishDateStart;
        set => SetProperty(ref _publishDateStart, value);
    }

    [DefaultValue(typeof(MaxDateTimeOffSetDefaultValueProvider))]
    public DateTimeOffset PublishDateEnd
    {
        get => _publishDateEnd;
        set => SetProperty(ref _publishDateEnd, value);
    }
}
