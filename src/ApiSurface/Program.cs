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

            var compilation = CSharpCompilation.Create(null).AddReferences(references)
                .AddReferences(
                    MetadataReference.CreateFromFile(
                        @"C:\Users\Svick\.nuget\packages\netstandard.library\2.0.3\build\netstandard2.0\ref\netstandard.dll"));

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

                if (!s.CanBeReferencedByName &&
                    !(s is IMethodSymbol methodSymbol &&
                      (methodSymbol.MethodKind == MethodKind.Conversion ||
                       methodSymbol.MethodKind == MethodKind.Constructor)))
                    return false;

                if (s.DeclaredAccessibility == Accessibility.Internal ||
                    s.DeclaredAccessibility == Accessibility.ProtectedAndInternal)
                    return false;

                if (s.IsOverride)
                    return false;

                return true;
            }

            if (!ShouldShow(symbol))
                return;

            string indent = new string(' ', indentLevel);

            Console.WriteLine($"{indent}{symbol.Kind}: {symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}");

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
