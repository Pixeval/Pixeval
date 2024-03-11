#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/IBookmarkableViewModel.cs
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
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Input;
using Pixeval.CoreApi.Model;

namespace Pixeval.Controls;

public interface IWorkViewModel
{
    long Id { get; }

    int TotalBookmarks { get; }

    bool IsBookmarked { get; set; }

    Tag[] Tags { get; }

    string Title { get; }

    string Caption { get; }

    UserInfo User { get; }

    DateTimeOffset PublishDate { get; }

    bool IsAiGenerated { get; }

    bool IsXRestricted { get; }

    BadgeMode XRestrictionCaption { get; }

    XamlUICommand BookmarkCommand { get; }

    XamlUICommand SaveCommand { get; }

    Task<bool> TryLoadThumbnailAsync(IDisposable key);
}
