using System;
using CSharpE.Samples.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace CSharpE.Samples.MonoCecil
{
    static class Program
    {
        static void Main()
        {
            var assemblyName = "MyAssembly";
            var assembly = AssemblyDefinition.CreateAssembly(
                new AssemblyNameDefinition(assemblyName, new Version(1, 0)), assemblyName,
                ModuleKind.Dll);
            var module = assembly.MainModule;

            foreach (var entityKind in EntityKinds.ToGenerate)
            {
                var type = new TypeDefinition(null, entityKind.Name, TypeAttributes.Class, module.TypeSystem.Object);

                type.Interfaces.Add(new InterfaceImplementation(module
                    .ImportReference(typeof(IEquatable<>))
                    .MakeGenericInstanceType(type)));

                foreach (var propertyInfo in entityKind.Properties)
                {
                    var propertyType =
                        module.ImportReference(Type.GetType(propertyInfo.Type));

                    var field = new FieldDefinition(propertyInfo.LowercaseName,
                        FieldAttributes.Private, propertyType);
                    type.Fields.Add(field);

                    var property = new PropertyDefinition(propertyInfo.Name,
                        PropertyAttributes.None, propertyType);

                    var getMethod = new MethodDefinition("get_" + propertyInfo.Name,
                        MethodAttributes.Public | MethodAttributes.SpecialName,
                        propertyType);

                    var il = getMethod.Body.GetILProcessor();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, field);
                    il.Emit(OpCodes.Ret);

                    type.Methods.Add(getMethod);
                    property.GetMethod = getMethod;

                    var setMethod = new MethodDefinition("set_" + propertyInfo.Name,
                        MethodAttributes.Public | MethodAttributes.SpecialName,
                        module.TypeSystem.Void);
                    setMethod.Parameters.Add(new ParameterDefinition(propertyType));

                    il = setMethod.Body.GetILProcessor();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Stfld, field);
                    il.Emit(OpCodes.Ret);

                    type.Methods.Add(setMethod);
                    property.SetMethod = setMethod;

                    type.Properties.Add(property);
                }

                module.Types.Add(type);
            }

            assembly.Write(assemblyName + ".dll");
        }
    }
}
