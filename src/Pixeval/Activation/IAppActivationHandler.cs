// Copyright (c) Pixeval.
// Licensed under the GPL v3 License.

using System.Threading.Tasks;

namespace Pixeval.Activation;

public interface IAppActivationHandler
{
    string ActivationFragment { get; }

    Task Execute(string id);
}
