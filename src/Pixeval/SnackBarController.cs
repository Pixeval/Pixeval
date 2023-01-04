﻿#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2023 Pixeval/SnackBarController.cs
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

using System.Threading.Tasks;

namespace Pixeval;

public static class SnackBarController
{
    public const int SnackBarDurationShort = 1750;
    public const int SnackBarDurationLong = 2500;

    public static async void ShowSnack(string text, int duration)
    {
        App.AppViewModel.Window.PixevalAppSnackBar.Title = text;

        App.AppViewModel.Window.PixevalAppSnackBar.IsOpen = true;
        await Task.Delay(duration);
        App.AppViewModel.Window.PixevalAppSnackBar.IsOpen = false;
    }
}