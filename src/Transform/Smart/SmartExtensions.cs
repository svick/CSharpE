using System;
using CSharpE.Syntax;
using CSharpE.Syntax.Internals;
using CSharpE.Transform.Internals;

namespace CSharpE.Transform.Smart
{
    public static class SmartExtensions
    {
        public static void ForEachTypeWithAttribute<TAttribute>(
            this Syntax.Project project, Action<TypeDefinition> action)
            where TAttribute : System.Attribute
        {
            ClosureChecker.ThrowIfHasClosure(action);

            // TODO: DRY with other methods here
            if (project is TransformProject transformProject)
            {
                transformProject.TransformerBuilder.Collection(
                    project, p => p.TypesWithAttribute<TAttribute>(), (_, item) => action(item), Unit.Value);
            }
            else
            {
                foreach (var type in project.TypesWithAttribute<TAttribute>())
                {
                    action(type);
                }
            }
        }

        public static void ForEachPublicMethod(this TypeDefinition type, Action<MethodDefinition> action)
        {
            ClosureChecker.ThrowIfHasClosure(action);

            foreach (var method in type.PublicMethods)
            {
                action(method);
            }
        }

        public static void ForEachPublicMethod<T1>(this TypeDefinition type, T1 arg1, Action<T1, MethodDefinition> action)
        {
            ClosureChecker.ThrowIfHasClosure(action);
            ArgumentChecker.ThrowIfNotPersistent(arg1);

            if (type.SourceFile?.Project is TransformProject transformProject)
            {
                transformProject.TransformerBuilder.Collection(type, t => t.PublicMethods, action, arg1);
            }
            else
            {
                foreach (var method in type.PublicMethods)
                {
                    action(arg1, method);
                }
            }
        }
    }
}