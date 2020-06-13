using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class RecursivePattern : Pattern
    {
        private protected RecursivePatternSyntax syntax;

        private protected RecursivePattern(RecursivePatternSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        private protected RecursivePattern(
            TypeReference type, IEnumerable<Subpattern> positionalSubpatterns, IEnumerable<Subpattern> propertySubpatterns,
            VariableDesignation designation)
        {
            Type = type;
            PositionalSubpatterns = positionalSubpatterns?.ToList();
            PropertySubpatterns = propertySubpatterns?.ToList();
            Designation = designation;
        }

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

        private SeparatedSyntaxList<Subpattern, SubpatternSyntax> positionalSubpatterns;
        private protected IList<Subpattern> PositionalSubpatterns
        {
            get
            {
                if (positionalSubpatterns == null)
                    positionalSubpatterns = new SeparatedSyntaxList<Subpattern, SubpatternSyntax>(
                        syntax.PositionalPatternClause?.Subpatterns ?? default, this);

                return positionalSubpatterns;
            }
            set => SetList(ref positionalSubpatterns, new SeparatedSyntaxList<Subpattern, SubpatternSyntax>(value, this));
        }

        private SeparatedSyntaxList<Subpattern, SubpatternSyntax> propertySubpatterns;
        public IList<Subpattern> PropertySubpatterns
        {
            get
            {
                if (propertySubpatterns == null)
                    propertySubpatterns = new SeparatedSyntaxList<Subpattern, SubpatternSyntax>(
                        syntax.PropertyPatternClause?.Subpatterns ?? default, this);

                return propertySubpatterns;
            }
            set => SetList(ref propertySubpatterns, new SeparatedSyntaxList<Subpattern, SubpatternSyntax>(value, this));
        }

        private bool designationSet;
        private VariableDesignation designation;
        public VariableDesignation Designation
        {
            get
            {
                if (!designationSet)
                {
                    designation = FromRoslyn.VariableDesignation(syntax.Designation, this);
                    designationSet = true;
                }

                return designation;
            }
            set
            {
                Set(ref designation, value);
                designationSet = true;
            }
        }

        protected override PatternSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newType = typeSet ? type?.GetWrapped(ref thisChanged) : syntax.Type;
            var newPositionalSubpatterns =
                positionalSubpatterns?.GetWrapped(ref thisChanged) ?? syntax.PositionalPatternClause?.Subpatterns ?? default;
            var newPropertySubpatterns =
                propertySubpatterns?.GetWrapped(ref thisChanged) ?? syntax.PropertyPatternClause?.Subpatterns ?? default;
            var newDesignation = designationSet ? designation?.GetWrapped(ref thisChanged) : syntax.Designation;

            if (syntax == null || thisChanged == true)
            {
                var positionalPatternClause = this is PositionalPattern
                    ? RoslynSyntaxFactory.PositionalPatternClause(newPositionalSubpatterns)
                    : null;
                var propertyPatternClause = this is PropertyPattern || newPropertySubpatterns.Count > 0
                    ? RoslynSyntaxFactory.PropertyPatternClause(newPropertySubpatterns)
                    : null;

                syntax = RoslynSyntaxFactory.RecursivePattern(newType, positionalPatternClause, propertyPatternClause, newDesignation);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (RecursivePatternSyntax)newSyntax;
            Set(ref type, null);
            typeSet = false;
            SetList(ref positionalSubpatterns, null);
            SetList(ref propertySubpatterns, null);
            Set(ref designation, null);
            designationSet = false;
        }

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var pattern in PositionalSubpatterns)
            {
                pattern.ReplaceExpressions(filter, projection);
            }

            foreach (var pattern in PropertySubpatterns)
            {
                pattern.ReplaceExpressions(filter, projection);
            }
        }
    }

    public sealed class PositionalPattern : RecursivePattern
    {
        internal PositionalPattern(RecursivePatternSyntax syntax, SyntaxNode parent)
            : base(syntax, parent)
        {
            Debug.Assert(syntax.PositionalPatternClause != null);
        }

        public PositionalPattern(
            IEnumerable<Subpattern> positionalSubpatterns, IEnumerable<Subpattern> propertySubpatterns = null,
            VariableDesignation designation = null)
            : this(null, positionalSubpatterns, propertySubpatterns, designation) { }

        public PositionalPattern(
            TypeReference type, IEnumerable<Subpattern> positionalSubpatterns, IEnumerable<Subpattern> propertySubpatterns = null,
            VariableDesignation designation = null)
            : base(type, positionalSubpatterns, propertySubpatterns, designation) { }

        public new IList<Subpattern> PositionalSubpatterns
        {
            get => base.PositionalSubpatterns;
            set => base.PositionalSubpatterns = value;
        }

        private protected override SyntaxNode CloneImpl() =>
            new PositionalPattern(Type, PositionalSubpatterns, PropertySubpatterns, Designation);
    }

    public sealed class PropertyPattern : RecursivePattern
    {
        internal PropertyPattern(RecursivePatternSyntax syntax, SyntaxNode parent)
            : base(syntax, parent)
        {
            Debug.Assert(syntax.PositionalPatternClause == null);
        }

        public PropertyPattern(
            IEnumerable<Subpattern> propertySubpatterns, VariableDesignation designation = null)
            : this(null, propertySubpatterns, designation) { }

        public PropertyPattern(
            TypeReference type, IEnumerable<Subpattern> propertySubpatterns, VariableDesignation designation = null)
            : base(type, null, propertySubpatterns, designation) { }

        private protected override SyntaxNode CloneImpl() => new PropertyPattern(Type, PropertySubpatterns, Designation);
    }
}