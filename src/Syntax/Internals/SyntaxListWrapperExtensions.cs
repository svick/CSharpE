using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CSharpE.Syntax.Internals
{
    internal static class SyntaxListWrapperExtensions
    {
        public static SyntaxListWrapper<TWrapper, TNode> ToWrapperList<TWrapper, TNode>(this IEnumerable<TWrapper> source)
            where TWrapper : ISyntaxWrapper<TNode> where TNode : SyntaxNode =>
            new SyntaxListWrapper<TWrapper, TNode>(source);
    }
}