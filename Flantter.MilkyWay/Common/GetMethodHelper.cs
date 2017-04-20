using System;
using System.Collections.Generic;
using System.Reflection;

namespace Flantter.MilkyWay.Common
{
    public static class GetMethodHelper
    {
        public static IEnumerable<MethodInfo> GetMethods(this Type type, string name)
        {
            return GetMethods(type.GetTypeInfo(), name);
        }

        public static IEnumerable<MethodInfo> GetMethods(this TypeInfo typeInfo, string name)
        {
            var currentTypeInfo = typeInfo;

            do
            {
                foreach (var methodInfo in currentTypeInfo.GetDeclaredMethods(name))
                    yield return methodInfo;

                currentTypeInfo = typeInfo.BaseType.GetTypeInfo();
            } while (currentTypeInfo != null);
        }
    }
}
