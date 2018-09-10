using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using CSharpE.Syntax;

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

            bool interactive = false;
            if (args[0] == "-i")
            {
                interactive = true;
                args = args.Skip(1).ToArray();
            }

            string transformationAssemblyPath = args[0];
            var inputFilePaths = args.Skip(1).ToArray();

            var transformationAssembly =
                AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(transformationAssemblyPath));

            var transformationTypes = transformationAssembly.ExportedTypes.Where(t => typeof(ITransformation).IsAssignableFrom(t));

            List<ITransformation> transformations =
                transformationTypes.Select(tt => (ITransformation) Activator.CreateInstance(tt)).ToList();

            if (!interactive)
            {
                var inputFiles = await Task.WhenAll(inputFilePaths.Select(Syntax.SourceFile.OpenAsync));

                var project = new Syntax.Project(inputFiles);

                foreach (var transformation in transformations)
                {
                    transformation.Process(project, designTime: false);
                }

                foreach (var sourceFile in project.SourceFiles)
                {
                    File.WriteAllText(Path.ChangeExtension(sourceFile.Path, "g.cs"), sourceFile.GetText());
                }

                return;
            }

            var transformInputFiles = await Task.WhenAll(inputFilePaths.Select(SourceFile.OpenAsync));

            var transformProject = new Project(transformInputFiles, new LibraryReference[0], transformations);

            transformProject.Log += Console.WriteLine;

            while (true)
            {
                string input = Console.ReadLine();

                switch (input)
                {
                    case "d":
                    default:
                        await transformProject.ReloadSourceFilesAsync();
                        
                        var transformed = transformProject.Transform(designTime: true);

                        foreach (var sourceFile in transformed.SourceFiles)
                        {
                            var newPath = Path.Combine(Path.GetDirectoryName(sourceFile.Path), "design",
                                Path.GetFileName(sourceFile.Path));

                            Directory.CreateDirectory(Path.GetDirectoryName(newPath));

                            File.WriteAllText(newPath, sourceFile.Text);
                        }

                        break;
                }
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Usage: [-i] transformation-assembly input-files");
        }
    }
}
