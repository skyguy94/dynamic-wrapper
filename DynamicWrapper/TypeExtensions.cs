using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DynamicWrapper
{
  public static class TypeExtensions
  {
    public static MethodInfo GetGenericMethod(this Type type, string name, params Type[] parameterTypes)
    {
      var methods = type.GetMethods().Where(m => m.Name == name);

      var found = methods.FirstOrDefault(m => m.HasParameters(parameterTypes));
      return found;
    }

    public static bool HasParameters(this MethodInfo method, params Type[] parameterTypes)
    {
      var methodParameters = method.GetParameters().Select(parameter => parameter.ParameterType).ToArray();

      if (methodParameters.Length != parameterTypes.Length)
        return false;

      for (int i = 0; i < methodParameters.Length; i++)
        if (methodParameters[i].ToString() != parameterTypes[i].ToString())
          return false;

      return true;
    }

    public static IEnumerable<Type> AllInterfaces(this Type target)
    {
      foreach (var parentInterface in target.GetInterfaces())
      {
        yield return parentInterface;
        foreach (var childInterface in parentInterface.AllInterfaces())
        {
          yield return childInterface;
        }
      }
    }

    public static IEnumerable<MethodInfo> AllMethods(this Type target)
    {
      var allTypes = target.AllInterfaces().ToList();
      allTypes.Add(target);

      return from type in allTypes
        from method in type.GetMethods()
        select method;
    }
  }
}