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
        private readonly HashSet<Type> encounteredNodes = new HashSet<Type>();

        private static readonly Type[] ExcludedSyntaxNodeTypes =
        {
            // won't be supported
            typeof(MakeRefExpressionSyntax),
            typeof(RefTypeExpressionSyntax),
            typeof(RefValueExpressionSyntax),

            // likely won't have matching type in CSharpE
            typeof(ArrayRankSpecifierSyntax),
            typeof(OmittedTypeArgumentSyntax),
            typeof(MemberBindingExpressionSyntax),
            typeof(ElementBindingExpressionSyntax),
            typeof(NameColonSyntax),

            // might not be necessary to support right now
            typeof(AliasQualifiedNameSyntax),

            // TODO: these have to be handled
            typeof(TypeOfExpressionSyntax),
            typeof(SizeOfExpressionSyntax),
            typeof(ElementAccessExpressionSyntax),
            typeof(DeclarationExpressionSyntax),
            typeof(CastExpressionSyntax),
            typeof(AnonymousMethodExpressionSyntax),
            typeof(SimpleLambdaExpressionSyntax),
            typeof(ParenthesizedLambdaExpressionSyntax),
        };

        [Fact]
        public async Task AllSyntaxRoundTrips()
        {
            var file = await SourceFile.OpenAsync("AllSyntax.cs");

            new Project(file);

            WalkNode(file);

            var syntaxNodeTypes = typeof(CSharpSyntaxNode).Assembly.GetExportedTypes()
                .Where(t => typeof(Roslyn::SyntaxNode).IsAssignableFrom(t) && !t.IsAbstract && !t.Name.EndsWith("ListSyntax"))
                .ToHashSet();

            syntaxNodeTypes.ExceptWith(ExcludedSyntaxNodeTypes);

            syntaxNodeTypes.ExceptWith(encounteredNodes);

            Assert.True(
                syntaxNodeTypes.Count <= 136,
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
