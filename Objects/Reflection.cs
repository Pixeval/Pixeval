using System;
using System.Linq;
using System.Reflection;

namespace Pixeval.Objects
{
    public static class Reflection
    {
        public static MethodInfo[] PublicInstanceMethods<T>()
        {
            return typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public);
        }

        public static bool AttributeAttached<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return memberInfo.GetCustomAttribute<T>() != null;
        }

        public static bool IsImplementOf<TDerived, TInterface>()
        {
            return typeof(TDerived).IsImplementOf(typeof(TInterface));
        }

        public static bool IsImplementOf(this Type derivedType, Type interfaceType)
        {
            return interfaceType.IsAssignableFrom(derivedType);
        }

        public static T NewInstance<T>()
        {
            return (T) NewInstance(typeof(T));
        }

        public static object NewInstance(Type type)
        {
            return type.GetConstructors().First(ctor => !ctor.GetParameters().Any()).Invoke(new object[] { });
        }
    }
}