// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.ComponentModel;
using FluentIcons.Common;

namespace Pixeval.Settings;

public abstract class ObservableSettingsEntryBase(
    string header,
    string description,
    Symbol headerIcon)
    : SettingsEntryBase(header, description, headerIcon), INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
