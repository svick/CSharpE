using System;
using System.Linq;
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
                constructorInfo =
                    (from ctor in typeof(TSyntaxWrapper).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                        let parameters = ctor.GetParameters()
                        where parameters.Length == 2 &&
                              typeof(TSyntax).IsAssignableFrom(parameters[0].ParameterType) &&
                              typeof(SyntaxNode).IsAssignableFrom(parameters[1].ParameterType)
                        select ctor).Single();

                var syntaxType = constructorInfo.GetParameters()[0].ParameterType;
                var parentType = constructorInfo.GetParameters()[1].ParameterType;

                constructorExpression = LinqExpression.New(
                    constructorInfo, LinqExpression.Convert(param, syntaxType),
                    LinqExpression.Constant(null, parentType));
            }

            var lambda = LinqExpression.Lambda<Func<TSyntax, TSyntaxWrapper>>(constructorExpression, param);

            return lambda.Compile();
        }

        public static Func<TSyntax, TSyntaxWrapper> Constructor { get; } = CreateConstructor();
    }
}