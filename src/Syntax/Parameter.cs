using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;
using static CSharpE.Syntax.ParameterModifiers;

namespace CSharpE.Syntax
{
    public sealed class Parameter : SyntaxNode, ISyntaxWrapper<ParameterSyntax>
    {
        private ParameterSyntax syntax;
        
        internal Parameter(ParameterSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(ParameterSyntax syntax)
        {
            this.syntax = syntax;
            Modifiers = FromRoslyn.ParameterModifiers(syntax.Modifiers);
            name = new Identifier(syntax.Identifier);
        }

        public Parameter(TypeReference type, string name) : this(None, type, name) { }

        public Parameter(ParameterModifiers modifiers, TypeReference type, string name, Expression defaultValue = null)
        {
            Modifiers = modifiers;
            Type = type;
            Name = name;
            DefaultValue = defaultValue;
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

        public ParameterModifiers Modifiers { get; set; }

        public bool IsRef
        {
            get => Modifiers.Contains(Ref);
            set => Modifiers = Modifiers.With(Ref, value);
        }

        public bool IsOut
        {
            get => Modifiers.Contains(Out);
            set => Modifiers = Modifiers.With(Out, value);
        }

        public bool IsIn
        {
            get => Modifiers.Contains(In);
            set => Modifiers = Modifiers.With(In, value);
        }

        public bool IsThis
        {
            get => Modifiers.Contains(This);
            set => Modifiers = Modifiers.With(This, value);
        }

        public bool IsParams
        {
            get => Modifiers.Contains(Params);
            set => Modifiers = Modifiers.With(Params, value);
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

        private bool defaultValueSet;
        private Expression defaultValue;
        public Expression DefaultValue
        {
            get
            {
                if (!defaultValueSet)
                {
                    defaultValue = FromRoslyn.Expression(syntax.Default?.Value, this);
                    defaultValueSet = true;
                }

                return defaultValue;
            }
            set
            {
                Set(ref defaultValue, value);
                defaultValueSet = true;
            }
        }
        
        ParameterSyntax ISyntaxWrapper<ParameterSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);
            
            bool? thisChanged = false;

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;
            var newName = name.GetWrapped(ref thisChanged);
            var newDefaultValue = defaultValueSet ? defaultValue?.GetWrapped(ref thisChanged) : syntax.Default?.Value;

            if (syntax == null || thisChanged == true || Modifiers != FromRoslyn.ParameterModifiers(syntax.Modifiers)
                || ShouldAnnotate(syntax, changed))
            {
                var defaultClause = newDefaultValue == null
                    ? null
                    : RoslynSyntaxFactory.EqualsValueClause(newDefaultValue);

                var newSyntax = RoslynSyntaxFactory.Parameter(newAttributes, Modifiers.GetWrapped(), newType, newName, defaultClause);

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((ParameterSyntax)newSyntax);

            SetList(ref attributes, null);
            Set(ref type, null);
            Set(ref defaultValue, null);
            defaultValueSet = false;
        }

        private protected override SyntaxNode CloneImpl() => new Parameter(Modifiers, Type, Name, DefaultValue) { Attributes = Attributes };

        public override IEnumerable<SyntaxNode> GetChildren() => Attributes.Concat(new SyntaxNode[] { Type, DefaultValue });
    }
}