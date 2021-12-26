using System;

namespace Pixeval.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class GenerateConstructorAttribute : Attribute
{

}