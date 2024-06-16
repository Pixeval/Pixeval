#region Copyright

// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/IllustrationDownloadFormat.cs
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

using Pixeval.Attributes;

namespace Pixeval.Options;

[LocalizationMetadata(typeof(MiscResources))]
public enum IllustrationDownloadFormat
{
    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Jpg))]
    Jpg,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Png))]
    Png,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Bmp))]
    Bmp,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.WebPLossy))]
    WebPLossy,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.WebPLossless))]
    WebPLossless,

    [LocalizedResource(typeof(MiscResources), nameof(MiscResources.Original))]
    Original,
}
