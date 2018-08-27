using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpE.ApiSurface
{
    static class Program
    {
        static void Main()
        {
            var references = new[] { typeof(Syntax.Project), typeof(Transform.Project) }
                .Select(t => MetadataReference.CreateFromFile(t.Assembly.Location)).ToList();

            var compilation = CSharpCompilation.Create(null).AddReferences(references);

            foreach (var reference in references)
            {
                var symbol = compilation.GetAssemblyOrModuleSymbol(reference);

                Dump(symbol);
            }
        }

        static readonly GetChildSymbolsVisitor getChildSymbols = new GetChildSymbolsVisitor();

        static void Dump(ISymbol symbol, int indentLevel = 0)
        {
            bool ShouldShow(ISymbol s)
            {
                if (s is IAssemblySymbol)
                    return true;

                if (!s.CanBeReferencedByName)
                    return false;

                if (s.DeclaredAccessibility == Accessibility.Internal)
                    return false;

                if (s.IsOverride)
                    return false;

                return true;
            }

            if (!ShouldShow(symbol))
                return;

            string indent = new string(' ', indentLevel);

            Console.WriteLine($"{indent}{symbol.Kind}: {symbol}");

            var children = getChildSymbols.Visit(symbol);

            foreach (var child in children)
            {
                Dump(child, indentLevel + 2);
            }
        }
    }

    class GetChildSymbolsVisitor : SymbolVisitor<IEnumerable<ISymbol>>
    {
        public override IEnumerable<ISymbol> VisitAssembly(IAssemblySymbol symbol) => Visit(symbol.GlobalNamespace);

        public override IEnumerable<ISymbol> VisitNamespace(INamespaceSymbol symbol) => symbol.GetMembers();

        public override IEnumerable<ISymbol> VisitNamedType(INamedTypeSymbol symbol) => symbol.GetMembers();

        public override IEnumerable<ISymbol> VisitMethod(IMethodSymbol symbol) => Enumerable.Empty<ISymbol>();

        public override IEnumerable<ISymbol> VisitProperty(IPropertySymbol symbol) => Enumerable.Empty<ISymbol>();

        public override IEnumerable<ISymbol> VisitField(IFieldSymbol symbol) => Enumerable.Empty<ISymbol>();

        public override IEnumerable<ISymbol> VisitEvent(IEventSymbol symbol) => Enumerable.Empty<ISymbol>();
    }
}
