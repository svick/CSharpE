using System;
using CSharpE.Syntax.Internals;

namespace CSharpE.Syntax.Smart
{
    public static class SmartExtensions
    {
        public static void ForEachTypeWithAttribute<TAttribute>(
            this Project project, Action<TypeDefinition> action)
            where TAttribute : System.Attribute
        {
            ClosureChecker.ThrowIfHasClosure(action);

            var types = project.TypesWithAttribute<TAttribute>();

            foreach (var type in types)
            {
                action(type);
            }
        }

        public static void ForEachPublicMethod(this TypeDefinition type, Action<MethodDefinition> action)
        {
            ClosureChecker.ThrowIfHasClosure(action);

            throw new NotImplementedException();
        }
    }
}