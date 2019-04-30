using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class AttributeArgument : SyntaxNode, ISyntaxWrapper<AttributeArgumentSyntax>
    {
        private AttributeArgumentSyntax syntax;

        internal AttributeArgument(AttributeArgumentSyntax syntax, Attribute parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(AttributeArgumentSyntax syntax)
        {
            this.syntax = syntax;

            var identifierSyntax = syntax.NameEquals?.Name ?? syntax.NameColon?.Name;
            name = new Identifier(identifierSyntax?.Identifier);
            IsConstructorArgument = GetSyntaxIsConstructorArgument();
        }

        public AttributeArgument(Expression expression) : this(null, expression) { }

        public AttributeArgument(string name, Expression expression) : this(name, false, expression) { }

        public AttributeArgument(string name, bool isConstructorArgument, Expression expression)
        {
            this.name = new Identifier(name, true);
            IsConstructorArgument = isConstructorArgument;
            Expression = expression;
        }

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        private bool GetSyntaxIsConstructorArgument() => syntax.NameEquals == null;

        private bool isConstructorArgument;
        public bool IsConstructorArgument
        {
            get => isConstructorArgument;
            set
            {
                if (value == false && Name == null)
                    throw new ArgumentException(
                        "Can't set IsConstructorArgument to false when Name is null.", nameof(value));

                isConstructorArgument = value;
            }
        }

        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (expression == null)
                    expression = FromRoslyn.Expression(syntax.Expression, this);

                return expression;
            }
            set => SetNotNull(ref expression, value);
        }

        AttributeArgumentSyntax ISyntaxWrapper<AttributeArgumentSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newName = name.GetWrapped(ref thisChanged);
            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true || IsConstructorArgument != GetSyntaxIsConstructorArgument())
            {
                NameEqualsSyntax nameEquals = null;
                NameColonSyntax nameColon = null;

                if (newName != default)
                {
                    var identifierName = RoslynSyntaxFactory.IdentifierName(newName);

                    if (IsConstructorArgument)
                        nameColon = RoslynSyntaxFactory.NameColon(identifierName);
                    else
                        nameEquals = RoslynSyntaxFactory.NameEquals(identifierName);
                }

                syntax = RoslynSyntaxFactory.AttributeArgument(nameEquals, nameColon, newExpression);

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override SyntaxNode Parent { get; set; }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((AttributeArgumentSyntax)newSyntax);
            Set(ref expression, null);
        }

        private protected override SyntaxNode CloneImpl() => new AttributeArgument(Name, IsConstructorArgument, Expression);
    }
}