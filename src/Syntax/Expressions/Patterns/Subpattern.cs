using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class Subpattern : SyntaxNode, ISyntaxWrapper<SubpatternSyntax>
    {
        private SubpatternSyntax syntax;

        internal Subpattern(SubpatternSyntax syntax, RecursivePattern parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(SubpatternSyntax syntax)
        {
            this.syntax = syntax;
            Name = syntax.NameColon?.Name.Identifier.ValueText;
        }

        public Subpattern(Pattern pattern)
            : this(null, pattern) { }

        public Subpattern(string name, Pattern pattern)
        {
            Name = name;
            Pattern = pattern;
        }

        public string Name { get; set; }

        private Pattern pattern;
        public Pattern Pattern
        {
            get
            {
                if (pattern == null)
                    pattern = FromRoslyn.Pattern(syntax.Pattern, this);

                return pattern;
            }
            set => SetNotNull(ref pattern, value);
        }

        SubpatternSyntax ISyntaxWrapper<SubpatternSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newPattern = pattern?.GetWrapped(ref thisChanged) ?? syntax.Pattern;

            if (syntax == null || thisChanged == true || syntax.NameColon?.Name.Identifier.ValueText != Name
                || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.Subpattern(
                    Name == null ? null : RoslynSyntaxFactory.NameColon(Name), newPattern);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        public static implicit operator Subpattern(Pattern pattern) => new Subpattern(pattern);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((SubpatternSyntax)newSyntax);
            pattern = null;
        }

        private protected override SyntaxNode CloneImpl() => new Subpattern(Name, Pattern);

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression =>
            Pattern.ReplaceExpressions(filter, projection);
    }
}