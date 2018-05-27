using System;
using LinqExpression = System.Linq.Expressions.Expression;

namespace CSharpE.Syntax.Internals
{
    internal static class SyntaxWrapper<TSyntaxWrapper, TSyntax> where TSyntaxWrapper : ISyntaxWrapperBase<TSyntax>
    {
        private static Func<TSyntax, TSyntaxWrapper> CreateConstructor()
        {
            var param = LinqExpression.Parameter(typeof(TSyntax));

            var constructorInfo = typeof(TSyntaxWrapper).GetConstructor(new[] { typeof(TSyntax) });
            var lambda = LinqExpression.Lambda<Func<TSyntax, TSyntaxWrapper>>(
                LinqExpression.New(constructorInfo, param), param);

            return lambda.Compile();
        }

        public static Func<TSyntax, TSyntaxWrapper> Constructor { get; } = CreateConstructor();
    }
}