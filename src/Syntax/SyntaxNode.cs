using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using SystemLinqExpression = System.Linq.Expressions.Expression;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class SyntaxNode
    {
        private protected SyntaxNode() { }
        private protected SyntaxNode(Roslyn::SyntaxNode syntax) => markerAnnotation = Annotation.Get(syntax);

        internal SyntaxNode Parent { get; set; }

        internal SourceFile SourceFile => this is SourceFile sourceFile ? sourceFile : Parent?.SourceFile;

        internal BaseTypeDefinition EnclosingType =>
            this is BaseTypeDefinition typeDefinition ? typeDefinition : Parent?.EnclosingType;

        // local cached syntax might not be part of the tree, so it won't have correct Span
        internal TextSpan Span => GetSourceFileNode().Span;

        internal FileSpan FileSpan => new FileSpan(Span, SourceFile.GetSyntaxTree());

        // returns a copy of Roslyn version of this node that's part of the SourceFile SyntaxTree
        private protected Roslyn::SyntaxNode GetSourceFileNode()
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
                    markerAnnotation = Annotation.Create();

                return markerAnnotation;
            }
        }

        // since annotations are only useful as part of a whole SyntaxTree,
        // there is no need to recreate the syntax node because of a missing annotation if we're not building a tree
        private protected bool ShouldAnnotate(Roslyn::SyntaxNode syntax, bool? changed) => changed != null && !syntax.HasAnnotation(MarkerAnnotation);

        private protected T Annotate<T>(T syntax) where T : Roslyn::SyntaxNode =>
            syntax.WithAdditionalAnnotations(MarkerAnnotation);

        private protected void SetNotNull<T>(ref T field, T value) where T : SyntaxNode
        {
            if (value == null)
                throw new ArgumentNullException();

            Set(ref field, value);
        }

        private protected void Set<T>(ref T field, T value) where T : SyntaxNode
        {
            if (ReferenceEquals(field, value))
                return;

            if (field != null)
                field.Parent = null;

            field = WithParent(value, this);

            thisHasChanged = true;
        }

        private protected void SetList<T>(ref T field, T value) where T : SyntaxListBase
        {
            if (field != null)
                field.Parent = null;

            field = value;

            thisHasChanged = true;
        }

        internal static T WithParent<T>(T node, SyntaxNode parent) where T : SyntaxNode
        {
            if (node is null)
                return null;

            var newNode = (T)node.Clone();

            newNode.Parent = parent;

            return newNode;
        }

        internal void SetSyntax(Roslyn::SyntaxNode newSyntax)
        {
            SetSyntaxImpl(newSyntax);
            changeTracker.SetChanged();
        }

        private protected abstract void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax);

        private ChangeTracker changeTracker = new ChangeTracker();
        private bool thisHasChanged = false;

        private protected void GetAndResetChanged(ref bool? changed) => changeTracker.GetAndResetChanged(ref changed);

        private protected void GetAndResetChanged(ref bool? changed, out bool? thisChanged)
        {
            GetAndResetChanged(ref changed);
            thisChanged = thisHasChanged;
            thisHasChanged = false;
        }

        private protected void SetChanged(ref bool? changed) => changeTracker.SetChanged(ref changed);

        internal SyntaxNode Clone()
        {
            var clone = CloneImpl();

            clone.markerAnnotation = this.markerAnnotation;

            return clone;
        }

        private protected abstract SyntaxNode CloneImpl();

        private static ConcurrentDictionary<Type, Func<SyntaxNode, IEnumerable<SyntaxNode>>> getChildrenDictionary =
            new ConcurrentDictionary<Type, Func<SyntaxNode, IEnumerable<SyntaxNode>>>();

        public virtual IEnumerable<SyntaxNode> GetChildren()
        {
            static Func<SyntaxNode, IEnumerable <SyntaxNode>> GenerateGetChildren(Type type)
            {
                Func<IEnumerable<SyntaxNode>> enumerableEmpty = Enumerable.Empty<SyntaxNode>;
                Func<IEnumerable<SyntaxNode>, IEnumerable<SyntaxNode>, IEnumerable<SyntaxNode>> enumerableConcat =
                    Enumerable.Concat;

                var thisParameter = SystemLinqExpression.Parameter(typeof(SyntaxNode));
                var thisCasted = SystemLinqExpression.Convert(thisParameter, type);

                var result = SystemLinqExpression.Call(enumerableEmpty.Method);

                foreach (var property in type.GetProperties())
                {
                    if (property.Name == nameof(Parent))
                        continue;

                    var getTheProperty = SystemLinqExpression.Property(thisCasted, property);

                    if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
                    {
                        result = SystemLinqExpression.Call(
                            enumerableConcat.Method, result, SystemLinqExpression.NewArrayInit(typeof(SyntaxNode), getTheProperty));
                    }
                    else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
                    {
                        result = SystemLinqExpression.Call(
                            enumerableConcat.Method, result, getTheProperty);
                    }
                }

                return SystemLinqExpression.Lambda<Func<SyntaxNode, IEnumerable<SyntaxNode>>>(result, thisParameter).Compile();
            }

            return getChildrenDictionary.GetOrAdd(this.GetType(), GenerateGetChildren).Invoke(this);
        }

        public IEnumerable<SyntaxNode> GetDescendants()
        {
            // PERF: this is quadratic

            foreach (var child in GetChildren())
            {
                if (child == null)
                    continue;

                yield return child;

                foreach (var childDescendant in child.GetDescendants())
                {
                    yield return childDescendant;
                }
            }
        }

        private ISyntaxWrapper<Roslyn::SyntaxNode> AsWrapper() => (ISyntaxWrapper<Roslyn::SyntaxNode>)this;

        public static bool AreEquivalent(SyntaxNode node1, SyntaxNode node2) => 
            node1.AsWrapper().GetWrapped().IsEquivalentTo(node2.AsWrapper().GetWrapped());

        public override string ToString() => this.AsWrapper().GetWrapped().NormalizeWhitespace().ToString();
    }
}
