// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Numerics;

namespace Pixeval.Util.UI.Animating;

public interface IEasingFunction<out TV> where TV : INumber<TV>
{
    TV GetValue(double percentage);
}