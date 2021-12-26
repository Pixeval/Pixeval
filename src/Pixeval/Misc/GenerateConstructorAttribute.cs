using System;

namespace Pixeval.Misc;

/// <summary>
/// 只能用在有<see cref="IDataOnlyClass"/>接口的类
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class GenerateConstructorAttribute : Attribute
{

}