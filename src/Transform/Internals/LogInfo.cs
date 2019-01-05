using CSharpE.Syntax;

namespace CSharpE.Transform.Internals
{
    internal static class LogInfo
    {
        public static string GetName(object obj)
        {
            switch (obj)
            {
                case SourceFile sourceFile: return sourceFile.Path;
                case BaseTypeDefinition typeDefinition: return typeDefinition.Name;
                case MethodDefinition methodDefinition: return methodDefinition.Name;
                case FieldDefinition fieldDefinition: return fieldDefinition.Name;
            }

            return null;
        }
    }
}