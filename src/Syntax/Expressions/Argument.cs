using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class Argument : ISyntaxWrapper<ArgumentSyntax>
    {
        public ArgumentSyntax GetWrapped()
        {
            throw new System.NotImplementedException();
        }

        public static implicit operator Argument(Expression expression)
        {
            throw new NotImplementedException();
        }
    }
}