using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax.Internals
{
    internal sealed class NamespaceOrTypeList : SyntaxList<ISyntaxWrapper<MemberDeclarationSyntax>, MemberDeclarationSyntax>
    {
        internal NamespaceOrTypeList(SyntaxNode parent) : base(parent) { }
        internal NamespaceOrTypeList(IEnumerable<NamespaceOrTypeDefinition> list, SyntaxNode parent)
            : base(list.Select(x => x.NamespaceOrType), parent) { }
        internal NamespaceOrTypeList(SyntaxList<MemberDeclarationSyntax> syntaxList, SyntaxNode parent)
            : base(syntaxList, parent) { }

        protected override ISyntaxWrapper<MemberDeclarationSyntax> CreateWrapper(MemberDeclarationSyntax roslynSyntax)
        {
            if (roslynSyntax is NamespaceDeclarationSyntax ns)
                return new NamespaceDefinition(ns, Parent);

            return FromRoslyn.TypeDefinition(roslynSyntax, Parent);
        }
    }

    internal sealed class TypeList : SeparatedSyntaxList<TypeReference, TypeSyntax>
    {
        public TypeList(SyntaxNode parent) : base(parent) { }
        public TypeList(IEnumerable<TypeReference> list, SyntaxNode parent) : base(list, parent) { }
        public TypeList(SeparatedSyntaxList<TypeSyntax> syntaxList, SyntaxNode parent) : base(syntaxList, parent) { }

        protected override TypeReference CreateWrapper(TypeSyntax roslynSyntax) =>
            FromRoslyn.TypeReference(roslynSyntax, Parent);

        protected override SeparatedSyntaxList<TypeSyntax> CreateList(IEnumerable<TypeSyntax> nodes) =>
            base.CreateList(nodes.Select(n => n ?? RoslynSyntaxFactory.OmittedTypeArgument()));
    }

    internal sealed class MemberList : SyntaxList<MemberDefinition, MemberDeclarationSyntax>
    {
        internal MemberList(SyntaxNode parent) : base(parent) { }
        internal MemberList(IEnumerable<MemberDefinition> list, SyntaxNode parent) : base(list, parent) { }
        internal MemberList(SyntaxList<MemberDeclarationSyntax> syntaxList, SyntaxNode parent) : base(
            syntaxList, parent)
        { }

        protected override MemberDefinition CreateWrapper(MemberDeclarationSyntax roslynSyntax) =>
            FromRoslyn.MemberDefinition(roslynSyntax, (TypeDefinition)Parent);
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

    internal sealed class ExpressionList : SeparatedSyntaxList<Expression, ExpressionSyntax>
    {
        internal ExpressionList(SyntaxNode parent) : base(parent) { }
        internal ExpressionList(IEnumerable<Expression> list, SyntaxNode parent) : base(list, parent) { }
        internal ExpressionList(SeparatedSyntaxList<ExpressionSyntax> syntaxList, SyntaxNode parent)
            : base(syntaxList, parent) { }

        protected override Expression CreateWrapper(ExpressionSyntax roslynSyntax) =>
            FromRoslyn.Expression(roslynSyntax, Parent);

        protected override SeparatedSyntaxList<ExpressionSyntax> CreateList(IEnumerable<ExpressionSyntax> nodes) =>
            base.CreateList(nodes.Select(n => n ?? RoslynSyntaxFactory.OmittedArraySizeExpression()));
    }

    internal sealed class VariableDesignationList : SeparatedSyntaxList<VariableDesignation, VariableDesignationSyntax>
    {
        internal VariableDesignationList(SyntaxNode parent) : base(parent) { }
        internal VariableDesignationList(IEnumerable<VariableDesignation> list, SyntaxNode parent) : base(list, parent) { }
        internal VariableDesignationList(SeparatedSyntaxList<VariableDesignationSyntax> syntaxList, SyntaxNode parent)
            : base(syntaxList, parent) { }

        protected override VariableDesignation CreateWrapper(VariableDesignationSyntax roslynSyntax) =>
            FromRoslyn.VariableDesignation(roslynSyntax, Parent);
    }

    internal sealed class VariableInitializerList : SeparatedSyntaxList<VariableInitializer, ExpressionSyntax>
    {
        internal VariableInitializerList(SyntaxNode parent) : base(parent) { }
        internal VariableInitializerList(IEnumerable<VariableInitializer> list, SyntaxNode parent) : base(list, parent) { }
        internal VariableInitializerList(SeparatedSyntaxList<ExpressionSyntax> syntaxList, SyntaxNode parent)
            : base(syntaxList, parent) { }

        protected override VariableInitializer CreateWrapper(ExpressionSyntax roslynSyntax) =>
            FromRoslyn.VariableInitializer(roslynSyntax, Parent);
    }

    internal sealed class InterpolatedStringContentList
        : SyntaxList<InterpolatedStringContent, InterpolatedStringContentSyntax>
    {
        internal InterpolatedStringContentList(SyntaxNode parent) : base(parent) { }

        internal InterpolatedStringContentList(IEnumerable<InterpolatedStringContent> list, SyntaxNode parent)
            : base(list, parent) { }

        internal InterpolatedStringContentList(
            SyntaxList<InterpolatedStringContentSyntax> syntaxList, SyntaxNode parent)
            : base(syntaxList, parent) { }

        protected override InterpolatedStringContent CreateWrapper(InterpolatedStringContentSyntax roslynSyntax) =>
            FromRoslyn.InterpolatedStringContent(roslynSyntax, (InterpolatedStringExpression)Parent);
    }

    internal sealed class SwitchLabelList : SyntaxList<SwitchLabel, SwitchLabelSyntax>
    {
        internal SwitchLabelList(SyntaxNode parent) : base(parent) { }

        internal SwitchLabelList(IEnumerable<SwitchLabel> list, SyntaxNode parent)
            : base(list, parent) { }

        internal SwitchLabelList(
            SyntaxList<SwitchLabelSyntax> syntaxList, SyntaxNode parent)
            : base(syntaxList, parent) { }

        protected override SwitchLabel CreateWrapper(SwitchLabelSyntax roslynSyntax) =>
            FromRoslyn.SwitchLabel(roslynSyntax, (SwitchSection)Parent);
    }

    internal sealed class LinqClauseList : SyntaxListBase<LinqClause, Roslyn::SyntaxNode, List<Roslyn::SyntaxNode>>
    {
        internal LinqClauseList(SyntaxNode parent) : base(parent) { }

        internal LinqClauseList(IEnumerable<LinqClause> list, SyntaxNode parent)
            : base(list, parent) { }

        internal LinqClauseList(List<Roslyn::SyntaxNode> syntaxList, SyntaxNode parent)
            : base(syntaxList, parent) { }

        protected override List<Roslyn::SyntaxNode> CreateList(IEnumerable<Roslyn::SyntaxNode> nodes) =>
            (List<Roslyn::SyntaxNode>)nodes;

        protected override LinqClause CreateWrapper(Roslyn::SyntaxNode roslynSyntax) =>
            FromRoslyn.LinqClause(roslynSyntax, (LinqExpression)Parent);
    }

    internal sealed class TypeParameterConstraintList : SeparatedSyntaxList<TypeParameterConstraint, TypeParameterConstraintSyntax>
    {
        internal TypeParameterConstraintList(SyntaxNode parent) : base(parent) { }

        internal TypeParameterConstraintList(IEnumerable<TypeParameterConstraint> list, SyntaxNode parent)
            : base(list, parent) { }

        internal TypeParameterConstraintList(
            SeparatedSyntaxList<TypeParameterConstraintSyntax> syntaxList, SyntaxNode parent)
            : base(syntaxList, parent) { }

        protected override TypeParameterConstraint CreateWrapper(TypeParameterConstraintSyntax roslynSyntax) =>
            FromRoslyn.TypeParameterConstraint(roslynSyntax, (TypeParameterConstraintClause)Parent);
    }
}