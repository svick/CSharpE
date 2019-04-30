using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class NamespaceDefinition : SyntaxNode, ISyntaxWrapper<NamespaceDeclarationSyntax>
    {
        private NamespaceDeclarationSyntax syntax;

        internal NamespaceDefinition(NamespaceDeclarationSyntax syntax, SyntaxNode parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(NamespaceDeclarationSyntax syntax)
        {
            this.syntax = syntax;

            Name = syntax.Name.ToString();
        }

        public NamespaceDefinition(string name, params NamespaceOrTypeDefinition[] members)
            : this(name, members.AsEnumerable()) { }

        public NamespaceDefinition(string name, IEnumerable<NamespaceOrTypeDefinition> members)
        {
            Name = name;
            this.members = new NamespaceOrTypeList(members, this);
        }

        // TODO: cache NameSyntax
        public string Name { get; set; }

        private NamespaceOrTypeList members;
        public IList<NamespaceOrTypeDefinition> Members
        {
            get
            {
                if (members == null)
                    members = new NamespaceOrTypeList(syntax.Members, this);

                return ProjectionList.Create(
                    members, member => new NamespaceOrTypeDefinition(member), notd => notd.NamespaceOrType);
            }
            set => SetList(ref members, new NamespaceOrTypeList(value, this));
        }

        NamespaceDeclarationSyntax ISyntaxWrapper<NamespaceDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newMembers = members?.GetWrapped(ref thisChanged) ?? syntax.Members;

            if (syntax == null || thisChanged == true || Name != syntax.Name.ToString())
            {
                syntax = RoslynSyntaxFactory.NamespaceDeclaration(
                    RoslynSyntaxFactory.ParseName(Name), default, default, newMembers);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private SyntaxNode parent;
        internal override SyntaxNode Parent
        {
            get => parent;
            set
            {
                switch (value)
                {
                    case null:
                    case NamespaceDefinition _:
                    case SourceFile _:
                        parent = value;
                        break;
                    default:
                        throw new ArgumentException(nameof(value));
                }
            }
        }

        public override IEnumerable<SyntaxNode> GetChildren() => Members.Select(m => m.Value);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            throw new NotImplementedException();
        }

        internal override SyntaxNode Clone() => new NamespaceDefinition(Name, Members);
    }
}
