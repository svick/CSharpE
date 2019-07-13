using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class TypeParameter : SyntaxNode, ISyntaxWrapper<TypeParameterSyntax>
    {
        private TypeParameterSyntax syntax;

        internal TypeParameter(TypeParameterSyntax syntax, TypeDefinition parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(TypeParameterSyntax syntax)
        {
            this.syntax = syntax;
            Variance = GetSyntaxVariance();
            name = new Identifier(syntax.Identifier);
        }

        public TypeParameter(string name)
            : this(VarianceModifier.None, name) { }

        public TypeParameter(VarianceModifier variance, string name)
            : this(null, variance, name) { }

        public TypeParameter(IEnumerable<Attribute> attributes, VarianceModifier variance, string name)
        {
            this.attributes = new SyntaxList<Attribute, AttributeListSyntax>(attributes, this);
            Variance = variance;
            Name = name;
        }

        private SyntaxList<Attribute, AttributeListSyntax> attributes;
        public IList<Attribute> Attributes
        {
            get
            {
                if (attributes == null)
                    attributes = new SyntaxList<Attribute, AttributeListSyntax>(syntax.AttributeLists, this);

                return attributes;
            }
            set => SetList(ref attributes, new SyntaxList<Attribute, AttributeListSyntax>(value, this));
        }

        private static readonly BiDirectionalDictionary<SyntaxKind, VarianceModifier> ModifiersDictionary =
            new BiDirectionalDictionary<SyntaxKind, VarianceModifier>
            {
                {SyntaxKind.None, VarianceModifier.None },
                { SyntaxKind.InKeyword, VarianceModifier.In },
                { SyntaxKind.OutKeyword, VarianceModifier.Out }
            };

        private VarianceModifier GetSyntaxVariance() => ModifiersDictionary[syntax.VarianceKeyword.Kind()];

        public VarianceModifier Variance { get; set; }

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        public static implicit operator TypeParameter(string name) => new TypeParameter(name);

        TypeParameterSyntax ISyntaxWrapper<TypeParameterSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out bool? thisChanged);

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax.AttributeLists;
            var newName = name.GetWrapped(ref thisChanged);

            if (syntax == null || thisChanged == true || Variance != GetSyntaxVariance())
            {
                var varianceKeyword = RoslynSyntaxFactory.Token(ModifiersDictionary[Variance]);

                syntax = RoslynSyntaxFactory.TypeParameter(newAttributes, varianceKeyword, newName);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((TypeParameterSyntax)newSyntax);
            SetList(ref attributes, null);
        }

        private protected override SyntaxNode CloneImpl() => new TypeParameter(Attributes, Variance, Name);

        public override IEnumerable<SyntaxNode> GetChildren() => Attributes;
    }

    public enum VarianceModifier
    {
        None,
        In,
        Out
    }
}
