using System.Collections.Generic;
using Windows.ApplicationModel.Activation;
using Microsoft.Windows.AppLifecycle;

namespace Pixeval.Activation
{
    public static class ActivationRegistrar
    {
        static ActivationRegistrar()
        {
            FeatureHandlers[IllustrationActivationFragment] = new IllustrationAppActivationHandler();
        }

        public static readonly Dictionary<string, IAppActivationHandler> FeatureHandlers = new();

        public const string IllustrationActivationFragment = "illust";

        public static void Dispatch(AppActivationArguments args)
        {
            if (args.Kind == ExtendedActivationKind.Protocol && args.Data is IProtocolActivatedEventArgs { Uri: var activationUri } && FeatureHandlers.TryGetValue(activationUri.Host, out var handler))
            {
                handler.Execute(activationUri.PathAndQuery[1..]);
            }
        }
    }
}