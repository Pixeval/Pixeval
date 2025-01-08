// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

namespace Pixeval.Util.ComponentModels;

public interface INotifyDetailedPropertyChanged
{
    event DetailedPropertyChangedEventHandler? DetailedPropertyChanged;
}
