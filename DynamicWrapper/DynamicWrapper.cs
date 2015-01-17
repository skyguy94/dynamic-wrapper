using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace DynamicWrapper
{
  public class DynamicWrapper
  {
    public class DynamicWrapperBase
    {
      protected internal object RealObject;
    }

    private static readonly ModuleBuilder ModuleBuilder;

    static DynamicWrapper()
    {
      var assembly = Thread.GetDomain()
        .DefineDynamicAssembly(new AssemblyName("DynamicWrapper"), AssemblyBuilderAccess.Run);
      ModuleBuilder = assembly.DefineDynamicModule("DynamicWrapperModule", false);
    }

    private static readonly WrapperDictionary WrapperDictionary = new WrapperDictionary();

    public static Type GetWrapper(Type interfaceType, Type realObjectType)
    {
      Type wrapperType = WrapperDictionary.GetType(interfaceType, realObjectType);

      if (wrapperType == null)
      {
        wrapperType = GenerateWrapperType(interfaceType, realObjectType);
        WrapperDictionary.SetType(interfaceType, realObjectType, wrapperType);
      }

      return wrapperType;
    }

    private static Type GenerateWrapperType(Type interfaceType, Type realObjectType)
    {
      var wrapperName = string.Format("{0}_{1}_Wrapper", interfaceType.Name, realObjectType.Name);

      var typeBuilder = ModuleBuilder.DefineType(
        wrapperName,
        TypeAttributes.NotPublic | TypeAttributes.Sealed,
        typeof(DynamicWrapperBase),
        new[] {interfaceType});

      foreach (var method in interfaceType.AllMethods())
      {
        WrapperMethodBuilder.GenerateMethod(method, realObjectType, typeBuilder);
      }

      return typeBuilder.CreateType();
    }

    public static T CreateWrapper<T>(object realObject) where T : class
    {
      var dynamicType = GetWrapper(typeof(T), realObject.GetType());
      var dynamicWrapper = (DynamicWrapperBase) Activator.CreateInstance(dynamicType);

      dynamicWrapper.RealObject = realObject;

      return dynamicWrapper as T;
    }
  }
}
