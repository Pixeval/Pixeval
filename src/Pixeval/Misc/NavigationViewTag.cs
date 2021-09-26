using System;

namespace Pixeval.Misc
{
    public sealed class NavigationViewTag
    {
        public Type NavigateTo { get; }

        public object? Parameter { get; set; }

        public NavigationViewTag(Type navigateTo, object? parameter)
        {
            NavigateTo = navigateTo;
            Parameter = parameter;
        }
    }
}