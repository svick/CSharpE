namespace CSharpE.Syntax.Internals
{
    internal static class LogInfo
    {
        public static string GetName(object obj)
        {
            switch (obj)
            {
                case SourceFile sourceFile: return sourceFile.Path;
                case TypeDefinition typeDefinition: return typeDefinition.Name;
                case MethodDefinition methodDefinition: return methodDefinition.Name;
            }

            return null;
        }
    }
}