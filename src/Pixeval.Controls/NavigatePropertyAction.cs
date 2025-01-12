// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using Microsoft.UI.Xaml;
using System.Reflection;
using System;
using Microsoft.Xaml.Interactivity;
using Pixeval.Utilities;
using WinUI3Utilities;
using WinUI3Utilities.Attributes;

namespace Pixeval.Actions;

/// <summary>
/// An action that will change a specified property to a specified value when invoked.
/// </summary>
[DependencyProperty<PropertyPath>("PropertyName", DependencyPropertyDefaultValue.Default)]
[DependencyProperty<object>("TargetObject", DependencyPropertyDefaultValue.Default)]
public sealed partial class NavigatePropertyAction : DependencyObject, IAction
{
    /// <summary>
    /// Executes the action.
    /// </summary>
    /// <param name="sender">The <see cref="object"/> that is passed to the action by the behavior. Generally this is <seealso cref="Microsoft.Xaml.Interactivity.IBehavior.AssociatedObject"/> or a target object.</param>
    /// <param name="parameter">The value of this parameter is determined by the caller.</param>
    /// <returns>True if updating the property value succeeds; else false.</returns>
    public object Execute(object sender, object parameter)
    {
        var targetObject = ReadLocalValue(TargetObjectProperty) != DependencyProperty.UnsetValue ? TargetObject : sender;

        if (targetObject is null || PropertyName is null)
        {
            return false;
        }

        UpdatePropertyValue(targetObject);
        return true;
    }

    private void UpdatePropertyValue(object targetObject)
    {
        var targetType = targetObject.GetType();
        var propertyInfo = targetType.GetRuntimeProperty(PropertyName.Path)!;
        ValidateProperty(targetType.Name, propertyInfo);

        Exception? innerException = null;
        try
        {
            if (propertyInfo.PropertyType != typeof(bool))
            {
                ThrowHelper.Argument(propertyInfo.PropertyType, $"{PropertyName} 应该是 bool 类型的属性，实际类型是 {propertyInfo.PropertyType.Name}。");
            }

            var boolValue = (bool)propertyInfo.GetValue(targetObject)!;

            propertyInfo.SetValue(targetObject, !boolValue);
        }
        catch (Exception e)
        {
            innerException = e;
        }

        if (innerException is not null)
        {
            ThrowUtils.Argument($"无法将 bool 类型的值赋予 {propertyInfo.PropertyType.Name} 类型的属性 {PropertyName}。", innerException);
        }
    }

    /// <summary>
    /// Ensures the property is not null and can be written to.
    /// </summary>
    private void ValidateProperty(string targetTypeName, PropertyInfo? propertyInfo)
    {
        if (propertyInfo is null)
        {
            ThrowHelper.Argument(propertyInfo, $"在类型 {targetTypeName} 上找不到名为 {PropertyName} 的属性。");
        }

        if (!propertyInfo.CanWrite)
        {
            ThrowHelper.Argument(propertyInfo.CanWrite, $"类型 {targetTypeName} 定义的属性 {PropertyName} 没有 set 方法。");
        }
    }
}
