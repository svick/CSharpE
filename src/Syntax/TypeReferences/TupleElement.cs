using System;
using System.Text;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public sealed class TupleElement : SyntaxNode, ISyntaxWrapper<TupleElementSyntax>
    {
        private TupleElementSyntax syntax;

        internal TupleElement(TupleElementSyntax syntax, SyntaxNode parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(TupleElementSyntax syntax)
        {
            this.syntax = syntax;
            name = new Identifier(syntax.Identifier, true);
        }

        public TupleElement(TypeReference type, string name = null)
        {
            this.type = type;
            this.name = new Identifier(name, true);
        }

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

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref type, null);
            Init((TupleElementSyntax)newSyntax);
        }

        private protected override SyntaxNode CloneImpl() => new TupleElement(Type, Name);

        TupleElementSyntax ISyntaxWrapper<TupleElementSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;
            var newName = name.GetWrapped(ref thisChanged);

            if (syntax == null || thisChanged == true || !IsAnnotated(syntax))
            {
                syntax = RoslynSyntaxFactory.TupleElement(newType, newName);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        public void ComputeFullName(StringBuilder stringBuilder)
        {
            stringBuilder.Append(Type);

            if (Name != null)
            {
                stringBuilder.Append(' ');
                stringBuilder.Append(Name);
            }
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (!(other is TupleElement otherElement)) return false;

            return Type.Equals(otherElement.Type) && Equals(Name, otherElement.Name);
        }

        public override int GetHashCode() => HashCode.Combine(Type, Name);
    }
}
