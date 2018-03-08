﻿using System;
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

            foreach (var type in project.TypesWithAttribute<TAttribute>())
            {
                action(type);
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

            foreach (var method in type.PublicMethods)
            {
                action(arg1, method);
            }
        }
    }
}