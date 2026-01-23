using System;
using Avalonia.Controls;

namespace FluentAvalonia.UI.Controls;

/// <summary>
/// Specifies factory methods a <see cref="Frame"/> can use to resolve pages from
/// Types that aren't controls or from object/ViewModel instances directly
/// </summary>
public interface INavigationPageFactory
{
    /// <summary>
    /// Returns a user specified page based on the given type passed to <see cref="Frame.Navigate(Type)"/>
    /// </summary>
    /// <param name="srcType">The type of object used for creating the page</param>
    /// <returns>An IControl for the new page or <c>null</c> to use the default behavior</returns>
    Control GetPage(Type srcType);

    /// <summary>
    /// Returns a user specified page based on an instance of an existing object
    /// </summary>
    /// <param name="target">The target object that should be used to create the page</param>
    /// <returns>An IControl for the new page. Returning null will cancel the navigation operation</returns>
    Control? GetPageFromObject(object target);
}
