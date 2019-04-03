using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Roslyn = Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace CSharpE.Syntax.Tests
{
    public class AllSyntaxTests
    {
        private readonly HashSet<SyntaxKind> encounteredNodes = new HashSet<SyntaxKind>();

        private static readonly SyntaxKind[] ExcludedSyntaxNodeTypes =
        {
            None,

            // won't be supported
            MakeRefExpression,
            RefTypeExpression,
            RefValueExpression,

            // likely won't have matching type in CSharpE
            ArrayRankSpecifier,
            OmittedTypeArgument,
            MemberBindingExpression,
            ElementBindingExpression,
            NameColon,

            // might not be necessary to support right now
            AliasQualifiedName,

            // TODO: these have to be handled
            TypeOfExpression,
            SizeOfExpression,
            ElementAccessExpression,
            DeclarationExpression,
            CastExpression,
            AnonymousMethodExpression,
            SimpleLambdaExpression,
            ParenthesizedLambdaExpression,
        };

        [Fact]
        public async Task AllSyntaxRoundTrips()
        {
            var file = await SourceFile.OpenAsync("AllSyntax.cs");

            new Project(file);

            WalkNode(file);

            var syntaxKinds = ((SyntaxKind[])Enum.GetValues(typeof(SyntaxKind)))
                .Where(kind => !SyntaxFacts.IsAnyToken(kind) && !kind.ToString().EndsWith("List"))
                .ToHashSet();

            syntaxKinds.ExceptWith(ExcludedSyntaxNodeTypes);

            syntaxKinds.ExceptWith(encounteredNodes);

            Assert.True(
                syntaxKinds.Count <= 158,
                $"Missed {syntaxKinds.Count} kinds, including {syntaxKinds.FirstOrDefault()}.");
        }

        private void WalkNode(SyntaxNode node)
        {
            if (node == null)
                return;

            var syntaxWrapper = (ISyntaxWrapper<Roslyn::SyntaxNode>)node;
            encounteredNodes.Add(syntaxWrapper.GetWrapped().Kind());

            bool? changed = null;
            node.SetChanged(ref changed);

            foreach (var child in node.GetChildren())
            {
                WalkNode(child);
            }
        }
    }
}
