using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CSharpE.Syntax.Internals
{
    // TODO: this will need to be a lazy wrapper over actual SyntaxList
    internal sealed class SyntaxListWrapper<TWrapper, TNode> : Collection<TWrapper>, ISyntaxWrapper<SyntaxList<TNode>>
        where TWrapper : ISyntaxWrapper<TNode> where TNode : SyntaxNode
    {
        private bool changed;

        public SyntaxListWrapper(IEnumerable<TWrapper> source)
            : base(source.ToList())
        {
        }

        protected override void ClearItems()
        {
            changed = true;
            base.ClearItems();
        }

        protected override void InsertItem(int index, TWrapper item)
        {
            changed = true;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            changed = true;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TWrapper item)
        {
            changed = true;
            base.SetItem(index, item);
        }

        public SyntaxList<TNode> GetSyntax() => throw new NotImplementedException();

        public SyntaxList<TNode> GetChangedSyntaxOrNull() => throw new NotImplementedException();
    }
}