using System;
using System.Linq;
using CSharpE.Syntax;

namespace CSharpE.Transform.Internals
{
    internal static class DescendantFinder
    {
        public static Func<TAncestor, TDescendant> Create<TAncestor, TDescendant>(
            TAncestor ancestor, TDescendant descendant) where TDescendant : class
        {
            if (ReferenceEquals(ancestor, descendant))
                return a => (TDescendant)(object)a;

            if (!typeof(SyntaxNode).IsAssignableFrom(typeof(TDescendant)))
                throw new ArgumentException(
                    $"The {nameof(descendant)} parameter has to be {nameof(Project)} or {nameof(SyntaxNode)}, but it's {typeof(TDescendant).Name}.",
                    nameof(descendant));

            var descendantNode = (SyntaxNode)(object)descendant;

            if (typeof(Syntax.Project).IsAssignableFrom(typeof(TAncestor)))
            {
                string sourceFilePath = descendantNode.SourceFile.Path;

                Syntax.SourceFile GetSourceFileFromProject(TAncestor a) =>
                    ((Syntax.Project)(object)a).SourceFiles.SingleOrDefault(f => f.Path == sourceFilePath);

                var getDescendantFromSourceFile = Create(GetSourceFileFromProject(ancestor), descendant);

                return a => getDescendantFromSourceFile(GetSourceFileFromProject(a));
            }

            if (!typeof(SyntaxNode).IsAssignableFrom(typeof(TAncestor)))
                throw new ArgumentException(
                    $"The {nameof(ancestor)} parameter has to be {nameof(Project)} or {nameof(SyntaxNode)}, but it's {typeof(TAncestor).Name}.",
                    nameof(ancestor));

            // TODO: somehow handle modified spans after the code was changed

            var descendantSpan = descendantNode.Span;

            TDescendant GetDescendant(TAncestor a)
            {
                var currentNode = (SyntaxNode)(object)a;

                while (true)
                {
                    if (currentNode is TDescendant foundDescendant && currentNode.Span == descendantSpan)
                        return foundDescendant;

                    bool found = false;

                    foreach (var child in currentNode.Children)
                    {
                        if (child.Span.Contains(descendantSpan))
                        {
                            currentNode = child;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        return null;
                }
            }

            return GetDescendant;
        }
    }
}