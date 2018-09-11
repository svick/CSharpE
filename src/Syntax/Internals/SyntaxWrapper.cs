using System;
using System.Reflection;
using LinqExpression = System.Linq.Expressions.Expression;

namespace CSharpE.Syntax.Internals
{
    internal static class SyntaxWrapper<TSyntaxWrapper, TSyntax> where TSyntaxWrapper : ISyntaxWrapper<TSyntax>
    {
        private static Func<TSyntax, TSyntaxWrapper> CreateConstructor()
        {
            var param = LinqExpression.Parameter(typeof(TSyntax));

            // TODO: does this step actually ever work?
            var constructorInfo = typeof(TSyntaxWrapper).GetConstructor(new[] { typeof(TSyntax) });
            LinqExpression constructorExpression;
            if (constructorInfo != null)
            {
                constructorExpression = LinqExpression.New(constructorInfo, param);
            }
            else
            {
                constructorInfo = typeof(TSyntaxWrapper).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
                    null, new[] {typeof(TSyntax), typeof(SyntaxNode)}, null);
                
                constructorExpression = LinqExpression.New(
                    constructorInfo, param, LinqExpression.Constant(null, typeof(SyntaxNode)));
            }

            var lambda = LinqExpression.Lambda<Func<TSyntax, TSyntaxWrapper>>(constructorExpression, param);

            return lambda.Compile();
        }

        public static Func<TSyntax, TSyntaxWrapper> Constructor { get; } = CreateConstructor();
    }
}