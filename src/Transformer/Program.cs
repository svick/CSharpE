using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using CSharpE.Syntax;
using CSharpE.Transform;

namespace CSharpE.Transformer
{
    static class Program
    {
        public static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();

        private static async Task MainAsync(string[] args)
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

            var inputFiles = await Task.WhenAll(inputFilePaths.Select(SourceFile.OpenAsync));

            var project = new Project(inputFiles);

            foreach (var transformationType in transformationTypes)
            {
                var transformation = (ITransformation)Activator.CreateInstance(transformationType);

                transformation.Process(project);
            }

            foreach (var sourceFile in project.SourceFiles)
            {
                if (sourceFile.IsModified)
                {
                    if (Path.GetExtension(sourceFile.Path) != ".cse")
                    {
                        // TODO: Either should not be possible, or should throw earlier to identify which transformation it was
                        Console.WriteLine($"Error: Input file '{sourceFile.Path}' with extension other than .cse modified by transformation.");
                    }
                    
                    File.WriteAllText(Path.ChangeExtension(sourceFile.Path, "g.cs"), sourceFile.GetText());
                }
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Usage: transformation-assembly input-files");
        }
    }
}
