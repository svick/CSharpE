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
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName(assemblyName), AssemblyBuilderAccess.Save);
            var moduleBuilder =
                assemblyBuilder.DefineDynamicModule(assemblyName, assemblyName + ".dll");

            foreach (var entityKind in EntityKinds.ToGenerate)
            {
                var typeBuilder = moduleBuilder.DefineType(entityKind.Name);

                typeBuilder.AddInterfaceImplementation(
                    typeof(IEquatable<>).MakeGenericType(typeBuilder));

                foreach (var property in entityKind.Properties)
                {
                    var type = Type.GetType(property.Type);

                    var fieldBuilder = typeBuilder.DefineField(
                        property.LowercaseName, type, FieldAttributes.Private);

                    var propertyBuilder = typeBuilder.DefineProperty(
                        property.Name, PropertyAttributes.None, type, new Type[0]);

                    var getMethod = typeBuilder.DefineMethod("get_" + property.Name,
                        MethodAttributes.Public | MethodAttributes.SpecialName,
                        type, new Type[0]);

                    var il = getMethod.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, fieldBuilder);
                    il.Emit(OpCodes.Ret);

                    propertyBuilder.SetGetMethod(getMethod);

                    var setMethod = typeBuilder.DefineMethod("set_" + property.Name,
                        MethodAttributes.Public | MethodAttributes.SpecialName,
                        typeof(void), new[] { type });

                    il = setMethod.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Stfld, fieldBuilder);
                    il.Emit(OpCodes.Ret);

                    propertyBuilder.SetSetMethod(setMethod);
                }

                typeBuilder.CreateType();
            }

            assemblyBuilder.Save(assemblyName + ".dll");
        }
    }
}
