using System.Diagnostics;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class TypeParameterConstraint : SyntaxNode, ISyntaxWrapper<TypeParameterConstraintSyntax>
    {
        private protected TypeParameterConstraint() { }
        private protected TypeParameterConstraint(TypeParameterConstraintSyntax syntax) : base(syntax) { }

        TypeParameterConstraintSyntax ISyntaxWrapper<TypeParameterConstraintSyntax>.GetWrapped(ref bool? changed) =>
            GetWrappedConstraint(ref changed);

        internal abstract TypeParameterConstraintSyntax GetWrappedConstraint(ref bool? changed);
    }

    public sealed class ClassConstraint : TypeParameterConstraint
    {
        private ClassOrStructConstraintSyntax syntax;

        internal ClassConstraint(ClassOrStructConstraintSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Debug.Assert(syntax.Kind() == SyntaxKind.ClassConstraint);

            this.syntax = syntax;
            Parent = parent;
        }

        public ClassConstraint() { }

        internal override TypeParameterConstraintSyntax GetWrappedConstraint(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            if (syntax == null)
            {
                syntax = RoslynSyntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            syntax = (ClassOrStructConstraintSyntax)newSyntax;

        private protected override SyntaxNode CloneImpl() => new ClassConstraint();
    }

    public sealed class StructConstraint : TypeParameterConstraint
    {
        private ClassOrStructConstraintSyntax syntax;

        internal StructConstraint(ClassOrStructConstraintSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Debug.Assert(syntax.Kind() == SyntaxKind.StructConstraint);

            this.syntax = syntax;
            Parent = parent;
        }

        public StructConstraint() { }

        internal override TypeParameterConstraintSyntax GetWrappedConstraint(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            if (syntax == null)
            {
                syntax = RoslynSyntaxFactory.ClassOrStructConstraint(SyntaxKind.StructConstraint);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            syntax = (ClassOrStructConstraintSyntax)newSyntax;

        private protected override SyntaxNode CloneImpl() => new StructConstraint();
    }

    public sealed class ConstructorConstraint : TypeParameterConstraint
    {
        private ConstructorConstraintSyntax syntax;

        internal ConstructorConstraint(ConstructorConstraintSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public ConstructorConstraint() { }

        internal override TypeParameterConstraintSyntax GetWrappedConstraint(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            if (syntax == null)
            {
                syntax = RoslynSyntaxFactory.ConstructorConstraint();

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            syntax = (ConstructorConstraintSyntax)newSyntax;

        private protected override SyntaxNode CloneImpl() => new ConstructorConstraint();
    }

    public sealed class TypeConstraint : TypeParameterConstraint
    {
        private TypeConstraintSyntax syntax;

        internal TypeConstraint(TypeConstraintSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public TypeConstraint(TypeReference type) => Type = type;

        private TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (type == null)
                    type = FromRoslyn.TypeReference(syntax.Type, this);

                return type;
            }
            set => SetNotNull(ref type, value);
        }

        internal override TypeParameterConstraintSyntax GetWrappedConstraint(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.TypeConstraint(newType);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (TypeConstraintSyntax)newSyntax;
            Set(ref type, null);
        }

        private protected override SyntaxNode CloneImpl() => new TypeConstraint(Type);
    }
}
