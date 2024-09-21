#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2024 Pixeval/AiUpscalerModelSettingsEntry.cs
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

using System.Linq.Expressions;
using System;
using Pixeval.AppManagement;
using Pixeval.Controls.Settings;
using Pixeval.Controls;
using System.Collections.Generic;

namespace Pixeval.Settings.Models;

public class AiUpscalerModelSettingsEntry(AppSettings appSettings, Expression<Func<AppSettings, Enum>> property, IReadOnlyList<StringRepresentableItem> array)
    : EnumAppSettingsEntry(appSettings, property, array)
{
    public override AiUpscalerModelSettingsCard Element => new() { Entry = this };
}
