using System;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class LambdaParameter : SyntaxNode, ISyntaxWrapper<ParameterSyntax>
    {
        private ParameterSyntax syntax;
        
        internal LambdaParameter(ParameterSyntax syntax, LambdaExpression parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(ParameterSyntax syntax)
        {
            this.syntax = syntax;
            Modifier = GetSyntaxModifier();
            name = new Identifier(syntax.Identifier);
        }

        public LambdaParameter(string name) : this(null, name) { }

        public LambdaParameter(TypeReference type, string name) : this(LambdaParameterModifier.None, type, name) { }

        public LambdaParameter(LambdaParameterModifier modifier, TypeReference type, string name)
        {
            Modifier = modifier;
            Type = type;
            Name = name;
        }

        private static readonly BiDirectionalDictionary<SyntaxKind, LambdaParameterModifier> ModifiersDictionary =
            new BiDirectionalDictionary<SyntaxKind, LambdaParameterModifier>
            {
                { SyntaxKind.RefKeyword, LambdaParameterModifier.Ref },
                { SyntaxKind.OutKeyword, LambdaParameterModifier.Out },
                { SyntaxKind.InKeyword, LambdaParameterModifier.In }
            };

        private LambdaParameterModifier GetSyntaxModifier()
        {
            switch (syntax.Modifiers.Count)
            {
                case 0:
                    return LambdaParameterModifier.None;
                case 1:
                    return ModifiersDictionary[syntax.Modifiers.Single().Kind()];
            }

            throw new InvalidOperationException();
        }

        public LambdaParameterModifier Modifier { get; set; }

        private bool typeSet;
        private TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (!typeSet)
                {
                    type = FromRoslyn.TypeReference(syntax.Type, this);
                    typeSet = true;
                }

                return type;
            }
            set
            {
                Set(ref type, value);
                typeSet = true;
            }
        }

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }
        
        ParameterSyntax ISyntaxWrapper<ParameterSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);
            
            bool? thisChanged = false;
            
            var newType = typeSet ? type?.GetWrapped(ref thisChanged) : syntax.Type;
            var newName = name.GetWrapped(ref thisChanged);

            if (syntax == null || thisChanged == true || Modifier != GetSyntaxModifier() || !IsAnnotated(syntax))
            {
                SyntaxTokenList GetModifierSyntax()
                {
                    if (Modifier == LambdaParameterModifier.None)
                        return default;

                    return new SyntaxTokenList(RoslynSyntaxFactory.Token(ModifiersDictionary[Modifier]));
                }

                var newSyntax = RoslynSyntaxFactory.Parameter(default, GetModifierSyntax(), newType, newName, default);

                syntax = Annotate(newSyntax);
                
                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref type, null);
            typeSet = false;
            Init((ParameterSyntax)newSyntax);
        }

        private protected override SyntaxNode CloneImpl() => new LambdaParameter(Modifier, Type, Name);
    }

    public enum LambdaParameterModifier
    {
        None,
        Ref,
        Out,
        In
    }
}