// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

namespace Pixeval.Util.ComponentModels;

public interface INotifyDetailedPropertyChanging
{
    event DetailedPropertyChangingEventHandler? DetailedPropertyChanging;
}
