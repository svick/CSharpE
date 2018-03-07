using System;

namespace CSharpE.Syntax.Smart
{
    public static class SmartExtensions
    {
        public static void ForEachTypeWithAttribute<TAttribute>(
            this Project project, System.Action<TypeDefinition, Attribute> action)
            where TAttribute : System.Attribute
        {
            throw new NotImplementedException();
        }

        public static void ForEachPublicMethod(this TypeDefinition type, Action<MethodDefinition> action)
        {
            throw new NotImplementedException();
        }
    }
}