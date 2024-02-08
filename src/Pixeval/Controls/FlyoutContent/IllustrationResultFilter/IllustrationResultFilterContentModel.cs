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
using Pixeval.Controls.TokenInput;

namespace Pixeval.Controls.FlyoutContent;

public class IllustrationResultFilterContentModel
{
    public ObservableCollection<Token> IncludeTags { get; set; } = [];

    public ObservableCollection<Token> ExcludeTags { get; set; } = [];

    public int LeastBookmark { get; set; } = 0;

    public int MaximumBookmark { get; set; } = int.MaxValue;

    public ObservableCollection<Token> UserGroupName { get; set; } = [];

    public Token IllustratorName { get; set; } = new();

    public long IllustratorId { get; set; } = -1;

    public Token IllustrationName { get; set; } = new();

    public long IllustrationId { get; set; } = -1;

    public DateTimeOffset PublishDateStart { get; set; } = DateTimeOffset.MinValue;

    public DateTimeOffset PublishDateEnd { get; set; } = DateTimeOffset.MaxValue;
}
