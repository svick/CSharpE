using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax.Tests
{
    public class AllSyntaxTests
    {
        private HashSet<Type> encounteredNodes = new HashSet<Type>();

        private static Type[] excludedSyntaxNodeTypes = { typeof(AliasQualifiedNameSyntax) };

        [Fact]
        public async Task AllSyntaxRoundTrips()
        {
            var file = await SourceFile.OpenAsync("AllSyntax.cs");

            new Project(file);

            WalkNode(file);

            var syntaxNodeTypes = typeof(CSharpSyntaxNode).Assembly.GetExportedTypes()
                .Where(t => typeof(Roslyn::SyntaxNode).IsAssignableFrom(t) && !t.IsAbstract && !t.Name.EndsWith("ListSyntax"))
                .ToHashSet();

            syntaxNodeTypes.ExceptWith(excludedSyntaxNodeTypes);

            syntaxNodeTypes.ExceptWith(encounteredNodes);

            Assert.True(
                syntaxNodeTypes.Count <= 0,
                $"Missed {syntaxNodeTypes.Count} types, including {syntaxNodeTypes.FirstOrDefault()?.Name}.");
        }

        private void WalkNode(SyntaxNode node)
        {
            if (node == null)
                return;

            var syntaxWrapper = (ISyntaxWrapper<Roslyn::SyntaxNode>)node;
            encounteredNodes.Add(syntaxWrapper.GetWrapped().GetType());

            bool? changed = null;
            node.SetChanged(ref changed);

            foreach (var child in node.GetChildren())
            {
                WalkNode(child);
            }
        }
    }
}
