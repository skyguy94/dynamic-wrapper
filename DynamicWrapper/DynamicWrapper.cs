using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using Sigil.NonGeneric;

namespace DynamicWrapper
{
    public class DynamicWrapper
    {
        private static readonly ModuleBuilder ModuleBuilder;
        private static readonly WrapperDictionary WrapperDictionary = new WrapperDictionary();

        public class DynamicWrapperBase
        {
            protected internal object RealObject;
        }

        static DynamicWrapper()
        {
            var assembly = Thread.GetDomain()
                .DefineDynamicAssembly(new AssemblyName("DynamicWrapper"), AssemblyBuilderAccess.Run);
            ModuleBuilder = assembly.DefineDynamicModule("DynamicWrapperModule", false);
        }

        public static T CreateWrapper<T>(object realObject) where T : class
        {
            var dynamicType = GetWrapper(typeof(T), realObject.GetType());
            var dynamicWrapper = (DynamicWrapperBase) Activator.CreateInstance(dynamicType);

            dynamicWrapper.RealObject = realObject;

            return dynamicWrapper as T;
        }

        public static Type GetWrapper(Type interfaceType, Type realObjectType)
        {
            var wrapperType = WrapperDictionary.GetType(interfaceType, realObjectType);

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
                GenerateMethod(method, realObjectType, typeBuilder);
            }

            return typeBuilder.CreateType();
        }

        private static void GenerateMethod(MethodInfo methodDefinition, Type objectType, TypeBuilder typeBuilder)
        {
            if (methodDefinition.IsGenericMethod)
            {
                EmitGenericMethod(methodDefinition.GetGenericMethodDefinition(), objectType, typeBuilder);
            }
            else
            {
                EmitMethod(methodDefinition, objectType, typeBuilder);
            }
        }


        private static void EmitGenericMethod(MethodInfo methodDefinition, Type objectType, TypeBuilder typeBuilder)
        {
            var parameters = methodDefinition.GetParameters().ToList();
            var parameterTypes = parameters.Select(parameter => parameter.ParameterType).ToArray();

            var method = objectType.GetGenericMethod(methodDefinition.Name, parameterTypes);
            if (method == null) throw new MissingMethodException();

            var methodBuilder = typeBuilder.DefineMethod(
                methodDefinition.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                methodDefinition.ReturnType,
                parameterTypes);

            methodBuilder.DefineGenericParameters(
                methodDefinition.GetGenericArguments().Select(arg => arg.Name).ToArray());

            var ilGenerator = methodBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            var sourceField = typeof(DynamicWrapperBase).GetField("RealObject",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (sourceField == null) throw new InvalidOperationException();

            ilGenerator.Emit(OpCodes.Ldfld, sourceField);

            for (int i = 1; i < parameters.Count + 1; i++)
            {
                ilGenerator.Emit(OpCodes.Ldarg, i);
            }

            ilGenerator.Emit(OpCodes.Call, method);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private static void EmitMethod(MethodInfo methodDefinition, Type objectType, TypeBuilder typeBuilder)
        {
            var parameters = methodDefinition.GetParameters().ToList();
            var parameterTypes = parameters.Select(parameter => parameter.ParameterType).ToArray();

            var method = objectType.GetMethod(methodDefinition.Name);
            if (method == null) throw new MissingMethodException();

            var methodWrapper = Emit.BuildInstanceMethod(methodDefinition.ReturnType,
                parameterTypes,
                typeBuilder,
                methodDefinition.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                doVerify: false);

            var sourceField = typeof(DynamicWrapperBase).GetField("RealObject",
                BindingFlags.Instance | BindingFlags.NonPublic);
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
