using System;
using System.Reflection;
using LinqExpression = System.Linq.Expressions.Expression;

namespace CSharpE.Syntax.Internals
{
    public static class SyntaxWrapper<TSyntaxWrapper, TSyntax> where TSyntaxWrapper : ISyntaxWrapper<TSyntax>
    {
        private static Func<TSyntax, TSyntaxWrapper> CreateConstructor()
        {
            var param = LinqExpression.Parameter(typeof(TSyntax));

            var constructorInfo = typeof(TSyntaxWrapper).GetTypeInfo().GetConstructor(new[] { typeof(TSyntax) });
            var lambda = LinqExpression.Lambda<Func<TSyntax, TSyntaxWrapper>>(LinqExpression.New(constructorInfo, param), param);

            return lambda.Compile();
        }

        private static readonly Func<TSyntax, TSyntaxWrapper> Constructor = CreateConstructor();

        public static TSyntaxWrapper Create(TSyntax wrapped) => Constructor(wrapped);
    }
}