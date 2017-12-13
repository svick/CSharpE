using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace CSharpE.Extensions.Actor
{
    public class RoslynActorTransformation
    {
        public CSharpCompilation Process(CSharpCompilation compilation)
        {
            for (int i = 0; i < compilation.SyntaxTrees.Length; i++)
            {
                var tree = compilation.SyntaxTrees[i];

                var root = tree.GetCompilationUnitRoot();

                var semanticModel = compilation.GetSemanticModel(tree);

                var newRoot = root.ReplaceNodes(
                    root.DescendantNodes().OfType<ClassDeclarationSyntax>(), (classDeclaration, _) =>
                    {
                        var newClassDeclaration = classDeclaration;

                        var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);

                        if (classSymbol.GetAttributes()
                            .Any(a => a.AttributeClass.ToString() == typeof(ActorAttribute).ToString()))
                        {
                            string actorSemaphoreFieldName = "_actor_semaphore";

                            newClassDeclaration = newClassDeclaration.ReplaceNodes(
                                newClassDeclaration.Members.OfType<MethodDeclarationSyntax>(),
                                (methodDeclaration, __) => methodDeclaration
                                    .WithReturnType(
                                        GenericName("Task")
                                            .AddTypeArgumentListArguments(methodDeclaration.ReturnType))
                                    .AddModifiers(Token(AsyncKeyword))
                                    .WithBody(
                                        Block(
                                            ExpressionStatement(
                                                AwaitExpression(
                                                    InvocationExpression(
                                                        MemberAccessExpression(
                                                            SimpleMemberAccessExpression,
                                                            IdentifierName(actorSemaphoreFieldName),
                                                            IdentifierName("WaitAsync"))))),
                                            TryStatement(
                                                methodDeclaration.Body, default, FinallyClause(
                                                    Block(
                                                        ExpressionStatement(
                                                            InvocationExpression(
                                                                MemberAccessExpression(
                                                                    SimpleMemberAccessExpression,
                                                                    IdentifierName(actorSemaphoreFieldName),
                                                                    IdentifierName("Release"))))))))));

                            var semaphoreType = IdentifierName("SemaphoreSlim");

                            newClassDeclaration = newClassDeclaration.AddMembers(
                                FieldDeclaration(
                                        VariableDeclaration(semaphoreType).AddVariables(
                                            VariableDeclarator(actorSemaphoreFieldName).WithInitializer(
                                                EqualsValueClause(
                                                    ObjectCreationExpression(semaphoreType).AddArgumentListArguments(
                                                        Argument(
                                                            LiteralExpression(
                                                                NumericLiteralExpression, Literal(1))))))))
                                    .WithModifiers(TokenList(Token(ReadOnlyKeyword))));
                        }

                        return newClassDeclaration;
                    });

                newRoot = newRoot.AddUsings(
                    UsingDirective(ParseName("System.Threading")), UsingDirective(ParseName("System.Threading.Tasks")));

                newRoot = newRoot.NormalizeWhitespace();

                var newTree = tree.WithRootAndOptions(newRoot, tree.Options);

                compilation = compilation.ReplaceSyntaxTree(tree, newTree);
            }

            return compilation;
        }
    }
}