using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax.Internals
{
    internal sealed class TypeList : SeparatedSyntaxList<TypeReference, TypeSyntax>
    {
        public TypeList(SyntaxNode parent) : base(parent) { }
        public TypeList(IEnumerable<TypeReference> list, SyntaxNode parent) : base(list, parent) { }
        public TypeList(SeparatedSyntaxList<TypeSyntax> syntaxList, SyntaxNode parent) : base(syntaxList, parent) { }

        protected override TypeReference CreateWrapper(TypeSyntax roslynSyntax) =>
            FromRoslyn.TypeReference(roslynSyntax, Parent);
    }

    internal sealed class NamespaceOrTypeList : SyntaxList<NamespaceOrTypeDefinition, MemberDeclarationSyntax>
    {
        internal NamespaceOrTypeList(SyntaxNode parent) : base(parent) { }
        internal NamespaceOrTypeList(IEnumerable<NamespaceOrTypeDefinition> list, SyntaxNode parent) 
            : base(list, parent) { }
        internal NamespaceOrTypeList(SyntaxList<MemberDeclarationSyntax> syntaxList, SyntaxNode parent)
            : base(syntaxList, parent) { }

        protected override NamespaceOrTypeDefinition CreateWrapper(MemberDeclarationSyntax roslynSyntax)
        {
            switch (roslynSyntax)
            {
                case NamespaceDeclarationSyntax ns: return new NamespaceDefinition(ns);
                case TypeDeclarationSyntax type: return new TypeDefinition(type, Parent);
                default: throw new InvalidOperationException();
            }
        }
    }

    internal sealed class StatementList : SyntaxList<Statement, StatementSyntax>
    {
        internal StatementList(SyntaxNode parent) : base(parent) { }
        internal StatementList(IEnumerable<Statement> list, SyntaxNode parent) : base(list, parent) { }
        internal StatementList(SyntaxList<StatementSyntax> syntaxList, SyntaxNode parent)
            : base(syntaxList, parent) { }

        protected override Statement CreateWrapper(StatementSyntax roslynSyntax) =>
            FromRoslyn.Statement(roslynSyntax, Parent);
    }

    internal sealed class MemberList : SyntaxList<MemberDefinition, MemberDeclarationSyntax>
    {
        internal MemberList(SyntaxNode parent) : base(parent) { }
        internal MemberList(IEnumerable<MemberDefinition> list, SyntaxNode parent) : base(list, parent) { }
        internal MemberList(SyntaxList<MemberDeclarationSyntax> syntaxList, SyntaxNode parent) : base(
            syntaxList, parent) { }

        protected override MemberDefinition CreateWrapper(MemberDeclarationSyntax roslynSyntax) =>
            FromRoslyn.MemberDefinition(roslynSyntax, (TypeDefinition)Parent);
    }
}