using System;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class SyntaxNode : IEquatable<SyntaxNode>
    {
        internal SyntaxNode() { }

        internal abstract SyntaxNode Parent { get; set; }

        internal SourceFile SourceFile => this is SourceFile sourceFile ? sourceFile : Parent?.SourceFile;

        // local cached syntax might not be part of the tree, so it won't have correct Span
        internal TextSpan Span => GetSourceFileNode().Span;

        internal FileSpan FileSpan => new FileSpan(Span, SourceFile.GetSyntaxTree());

        // returns a copy of Roslyn version of this node that's part of the SourceFile SyntaxTree
        protected Roslyn::SyntaxNode GetSourceFileNode()
        {
            var root = SourceFile.GetSyntaxTree().GetRoot();

            return this is SourceFile ? root : root.GetAnnotatedNodes(MarkerAnnotation).Single();
        }

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
            if (field != null)
                field.Parent = null;

            field = WithParent(value, this);
        }

        private protected void SetList<T>(ref T field, T value) where T : SyntaxListBase
        {
            if (field != null)
                field.Parent = null;

            field = value;
        }

        internal static T WithParent<T>(T node, SyntaxNode parent) where T : SyntaxNode
        {
            if (node is null)
                return null;

            if (ReferenceEquals(node.Parent, parent))
                return node;

            if (!(node.Parent is null))
                node = (T)node.Clone();

            node.Parent = parent;

            return node;
        }

        internal void SetSyntax(Roslyn::SyntaxNode newSyntax)
        {
            SetSyntaxImpl(newSyntax);
            changeTracker.SetChanged();
        }

        protected abstract void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax);

        private ChangeTracker changeTracker = new ChangeTracker();

        protected void GetAndResetChanged(ref bool? changed) => changeTracker.GetAndResetChanged(ref changed);

        protected void SetChanged(ref bool? changed) => changeTracker.SetChanged(ref changed);

        internal abstract SyntaxNode Clone();

        public bool Equals(SyntaxNode other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != this.GetType()) return false;

            // nodes that are part of a tree are compared by reference
            if (other.SourceFile != null || this.SourceFile != null)
                return false;

            var thisWrapper = (ISyntaxWrapper<Roslyn::SyntaxNode>)this;
            var otherWrapper = (ISyntaxWrapper<Roslyn::SyntaxNode>)other;

            return thisWrapper.GetWrapped().IsEquivalentTo(otherWrapper.GetWrapped());
        }

        public override bool Equals(object obj) => Equals(obj as SyntaxNode);

        public override int GetHashCode()
        {
            if (SourceFile != null)
                return base.GetHashCode();

            var thisWrapper = (ISyntaxWrapper<Roslyn::SyntaxNode>)this;

            // if nodes are equivalent, their strings should be equal
            return StringComparer.Ordinal.GetHashCode(thisWrapper.GetWrapped().ToString());
        }
    }
}