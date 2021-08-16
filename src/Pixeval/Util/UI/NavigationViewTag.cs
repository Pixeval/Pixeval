using System;

namespace Pixeval.Util.UI
{
    public sealed class NavigationViewTag
    {
        public Type NavigateTo { get; }

        public object Parameter { get; }

        public NavigationViewTag(Type navigateTo, object parameter)
        {
            NavigateTo = navigateTo;
            Parameter = parameter;
        }
    }
}