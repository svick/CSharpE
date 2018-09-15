using System;
using System.Reflection;
using System.Reflection.Emit;
using CSharpE.Samples.Core;

namespace CSharpE.Samples.ReflectionEmit
{
    static class Program
    {
        static void Main()
        {
            var assemblyName = "MyAssembly";
            var assembly = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName(assemblyName), AssemblyBuilderAccess.Save);
            var module =
                assembly.DefineDynamicModule(assemblyName, assemblyName + ".dll");

            foreach (var entityKind in EntityKinds.ToGenerate)
            {
                var type = module.DefineType(entityKind.Name);

                type.AddInterfaceImplementation(
                    typeof(IEquatable<>).MakeGenericType(type));

                foreach (var propertyInfo in entityKind.Properties)
                {
                    var propertyType = Type.GetType(propertyInfo.Type);

                    var field = type.DefineField(propertyInfo.LowercaseName, propertyType,
                        FieldAttributes.Private);

                    var property = type.DefineProperty(propertyInfo.Name,
                        PropertyAttributes.None, propertyType, new Type[0]);

                    var getMethod = type.DefineMethod("get_" + propertyInfo.Name,
                        MethodAttributes.Public | MethodAttributes.SpecialName,
                        propertyType, new Type[0]);

                    var il = getMethod.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, field);
                    il.Emit(OpCodes.Ret);

                    property.SetGetMethod(getMethod);

                    var setMethod = type.DefineMethod("set_" + propertyInfo.Name,
                        MethodAttributes.Public | MethodAttributes.SpecialName,
                        typeof(void), new[] { propertyType });

                    il = setMethod.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Stfld, field);
                    il.Emit(OpCodes.Ret);

                    property.SetSetMethod(setMethod);
                }

                type.CreateType();
            }

            assembly.Save(assemblyName + ".dll");
        }
    }
}
