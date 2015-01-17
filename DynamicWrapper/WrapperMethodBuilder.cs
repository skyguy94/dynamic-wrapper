using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Sigil.NonGeneric;

namespace DynamicWrapper
{
  internal class WrapperMethodBuilder
  {
    public static void GenerateMethod(MethodInfo methodDefinition, Type objectType, TypeBuilder typeBuilder)
    {
      if (methodDefinition.IsGenericMethod)
      {
        methodDefinition = methodDefinition.GetGenericMethodDefinition();
      }

      var sourceField = typeof(DynamicWrapper.DynamicWrapperBase)
        .GetField("RealObject", BindingFlags.Instance | BindingFlags.NonPublic);

      var parameters = methodDefinition.GetParameters().ToList();
      var parameterTypes = parameters.Select(parameter => parameter.ParameterType).ToArray();

      var method = methodDefinition.IsGenericMethodDefinition
        ? objectType.GetGenericMethod(methodDefinition.Name, parameterTypes)
        : objectType.GetMethod(methodDefinition.Name);
      if (method == null) throw new MissingMethodException();

      var methodWrapper = Emit.BuildInstanceMethod(methodDefinition.ReturnType,
        parameterTypes,
        typeBuilder,
        methodDefinition.Name,
        MethodAttributes.Public | MethodAttributes.Virtual,
        doVerify: false);

      methodWrapper.LoadArgument(0);
      methodWrapper.LoadField(sourceField);
      for (ushort i = 1; i < parameters.Count + 1; i++)
      {
        methodWrapper.LoadArgument(i);
      }

      methodWrapper.Call(method);
      methodWrapper.Return();
      methodWrapper.CreateMethod();
    }
  }
}