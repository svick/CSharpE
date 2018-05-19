using Microsoft.CodeAnalysis;

namespace CSharpE.Transform
{
    public class FileDiff
    {
        public static FileDiff Between(SyntaxTree oldTree, SyntaxTree newTree)
        {
            var changes = newTree.GetChanges(oldTree);

            throw new System.NotImplementedException();
        }
    }
}