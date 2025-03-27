// Copyright (c) Mako.
// Licensed under the GPL v3 License.

using System;

namespace Mako;

/// <summary>
/// 为标注的类型生成一个方法<see cref="IDefaultFactory{TSelf}.CreateDefault"/>，返回值为所有属性都是默认值的新实例，并使类型实现<see cref="IDefaultFactory{TSelf}"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal class FactoryAttribute : Attribute;
