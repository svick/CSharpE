using System;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    public class TypeReference : ISyntaxWrapper<TypeSyntax>
    {
        private TypeSyntax syntax;
        private SyntaxContext context;

        // name used by syntax, computed lazily
        private string syntaxFullName;
        private string fullName;

        public string FullName
        {
            get
            {
                if (fullName == null && syntaxFullName == null)
                {
                    syntaxFullName = context.GetFullName(syntax);
                    context = default;
                }

                return fullName ?? syntaxFullName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException(nameof(value));

                fullName = value;
            }
        }

        public TypeReference(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                throw new ArgumentException(nameof(fullName));

            FullName = fullName;
        }

        public TypeReference(Type type)
        {
            // type.IsGenericType is not in .Net Standard 1.x
            if (type.GenericTypeArguments.Any())
                throw new NotImplementedException();

            FullName = type.FullName;
        }

        internal TypeReference(TypeSyntax syntax, SyntaxContext context)
        {
            this.syntax = syntax;
            this.context = context;
        }

        public static implicit operator TypeReference(Type type) => new TypeReference(type);

        public TypeSyntax GetWrapped()
        {
            // don't have to do anything if either both are null or both are non-null and equal
            if (syntaxFullName != fullName)
            {
                syntax = CSharpSyntaxFactory.ParseName(fullName);
                syntaxFullName = fullName;
                context = default;
            }

            return syntax;
        }
    }
}