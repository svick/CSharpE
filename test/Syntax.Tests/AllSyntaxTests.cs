using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            ArgListExpression,
            GlobalStatement,

            // won't have matching type in CSharpE
            ArrayRankSpecifier,
            OmittedTypeArgument,
            OmittedArraySizeExpression,
            MemberBindingExpression,
            ElementBindingExpression,
            NameColon,
            InterpolationAlignmentClause,
            InterpolationFormatClause,
            CatchDeclaration,
            CatchFilterClause,
            FinallyClause,
            SyntaxKind.Attribute,
            AttributeTargetSpecifier,
            NameEquals,
            SimpleBaseType,
            EqualsValueClause,

            // might not be necessary to support right now
            ArrayInitializerExpression,
            AliasQualifiedName,
            AnonymousMethodExpression
        };

        [Fact]
        public async Task AllSyntaxKinds()
        {
            var paths = new[] { "AllSyntax.cs", "AllSyntaxSource.cs" };

            foreach (var path in paths)
            {
                var file = await SourceFile.OpenAsync(path);

                new Project(file);

                WalkNode(file);
            }

            var excludedNamesRegexes = new[] { "Trivia$", "^Xml", "Cref", "List$" };

            var syntaxKinds = ((SyntaxKind[])Enum.GetValues(typeof(SyntaxKind)))
                .Where(
                    kind =>
                        !SyntaxFacts.IsAnyToken(kind) &&
                        !excludedNamesRegexes.Any(regex => Regex.IsMatch(kind.ToString(), regex)))
                .ToHashSet();

            syntaxKinds.ExceptWith(ExcludedSyntaxNodeTypes);

            syntaxKinds.ExceptWith(encounteredNodes);

            Assert.True(
                syntaxKinds.Count <= 73,
                $"Missed {syntaxKinds.Count} kinds, including {syntaxKinds.FirstOrDefault()}.");
        }

        [Fact]
        public async Task AllSyntaxRoundTrips1()
        {
            var path = "AllSyntax.cs";

            var file = await SourceFile.OpenAsync(path);

            new Project(file);

            WalkNode(file);

            Assert.Equal(await File.ReadAllTextAsync(path), file.ToString());
        }

        [Fact]
        public async Task AllSyntaxRoundTrips2()
        {
            // this mostly exists due to weirdness in NormalizeWhitespace()

            var sourcePath = "AllSyntaxSource.cs";
            var targetPath = "AllSyntaxTarget.cs";

            var file = await SourceFile.OpenAsync(sourcePath);

            new Project(file);

            WalkNode(file);

            Assert.Equal(await File.ReadAllTextAsync(targetPath), file.ToString());
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
