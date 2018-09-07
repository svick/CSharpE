using System;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace CSharpE.Transform.App
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                if (args.Length != 0)
                    Console.WriteLine("Error: At least two arguments required.");

                Usage();
                return;
            }

            string transformationAssemblyPath = args[0];
            var inputFilePaths = args.Skip(1);

            var transformationAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(transformationAssemblyPath);

            var transformationTypes = transformationAssembly.ExportedTypes.Where(t => typeof(ITransformation).IsAssignableFrom(t));

            var inputFiles = await Task.WhenAll(inputFilePaths.Select(Syntax.SourceFile.OpenAsync));

            var project = new Syntax.Project(inputFiles);

            foreach (var transformationType in transformationTypes)
            {
                var transformation = (ITransformation)Activator.CreateInstance(transformationType);

                transformation.Process(project, designTime: false);
            }

            foreach (var sourceFile in project.SourceFiles)
            {
                File.WriteAllText(Path.ChangeExtension(sourceFile.Path, "g.cs"), sourceFile.GetText());
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Usage: transformation-assembly input-files");
        }
    }
}
