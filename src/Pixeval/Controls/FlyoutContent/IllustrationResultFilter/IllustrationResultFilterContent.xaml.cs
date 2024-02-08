#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/IllustrationResultFilterContent.xaml.cs
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

using System.Linq;
using System.Reflection;
using Pixeval.Attributes;

namespace Pixeval.Controls.FlyoutContent;

public sealed partial class IllustrationResultFilterContent
{
    public IllustrationResultFilterContent()
    {
        InitializeComponent();
    }

    public IllustrationResultFilterContentViewModel ViewModel { get; set; } = new();

    public FilterSettings GetFilterSettings =>
        new(ViewModel.IncludeTags.ToArray(),
            ViewModel.ExcludeTags.ToArray(),
            ViewModel.LeastBookmark,
            ViewModel.MaximumBookmark,
            ViewModel.UserGroupName.ToArray(),
            ViewModel.IllustratorName.DeepClone(),
            ViewModel.IllustratorId,
            ViewModel.IllustrationName.DeepClone(),
            ViewModel.IllustrationId,
            ViewModel.PublishDateStart,
            ViewModel.PublishDateEnd);

    public void Reset()
    {
        foreach (var propertyInfo in typeof(IllustrationResultFilterContentViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            propertyInfo.SetIfHasDefaultValue(ViewModel);
        }
    }
}
