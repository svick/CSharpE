using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class SyntaxNode
    {
        internal SyntaxNode() { }

        public IEnumerable<SyntaxNode> Children => GetChildren().SelectMany(c => c);

        protected abstract IEnumerable<IEnumerable<SyntaxNode>> GetChildren();

        public abstract SyntaxNode Parent { get; internal set; }

        public SourceFile SourceFile => this is SourceFile sourceFile ? sourceFile : Parent?.SourceFile;

        // local cached syntax might not be part of the tree, so it won't have correct Span
        public TextSpan Span =>
            SourceFile.GetWrapped().GetRoot().GetAnnotatedNodes(MarkerAnnotation).Single().Span;

        protected static IEnumerable<SyntaxNode> Node(SyntaxNode node)
        {
            yield return node;
        }

        protected static IEnumerable<SyntaxNode> Nodes(params SyntaxNode[] nodes) => nodes;

        private SyntaxAnnotation markerAnnotation;
        private SyntaxAnnotation MarkerAnnotation
        {
            get
            {
                if (markerAnnotation == null)
                    markerAnnotation = new SyntaxAnnotation();

                return markerAnnotation;
            }
        }

        protected bool IsAnnotated(Roslyn::SyntaxNode syntax) => syntax.HasAnnotation(MarkerAnnotation);

        protected T Annotate<T>(T syntax) where T : Roslyn::SyntaxNode =>
            syntax.WithAdditionalAnnotations(MarkerAnnotation);

        protected void SetNotNull<T>(ref T field, T value) where T : SyntaxNode
        {
            if (value == null)
                throw new ArgumentNullException();

            Set(ref field, value);
        }

        protected void Set<T>(ref T field, T value) where T : SyntaxNode
        {
            if (value.Parent != null)
                throw new InvalidOperationException(
                    $"Can't set the parent of syntax node '{value}', because it already has one.");

            if (field != null)
                field.Parent = null;

            value.Parent = this;
            field = value;
        }
    }
}