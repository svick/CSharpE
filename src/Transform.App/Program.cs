using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using CSharpE.Syntax;
using CSharpE.Transform.Execution;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Roslyn = Microsoft.CodeAnalysis;

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
            string outputDirectory = null;
            if (args[0] == "-i")
            {
                interactive = true;
                outputDirectory = args[1];
                args = args.Skip(2).ToArray();
            }

            string transformationAssemblyPath = args[0];
            var inputPaths = args.Skip(1);

            var inputFilePaths = inputPaths.SelectMany(path => Directory.Exists(path)
                    ? Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories)
                    : new[] {path})
                .ToList();

            var transformationAssembly =
                AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(transformationAssemblyPath));

            var transformationTypes = transformationAssembly.ExportedTypes.Where(t => typeof(ITransformation).IsAssignableFrom(t));

            List<ITransformation> transformations =
                transformationTypes.Select(tt => (ITransformation) Activator.CreateInstance(tt)).ToList();

            var inputFiles = await Task.WhenAll(inputFilePaths.Select(SourceFile.OpenAsync));

            var project = new Syntax.Project(inputFiles, new[] { new AssemblyReference(typeof(object)) });

            if (!interactive)
            {
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

            var designTransformer = new ProjectTransformer(transformations);

            designTransformer.Log += Console.WriteLine;
            
            var buildTransformer = new ProjectTransformer(transformations);

            buildTransformer.Log += Console.WriteLine;

            Syntax.Project designTransformed = null;
            
            while (true)
            {
                Console.Write("Enter a command: ");
            
                string input = Console.ReadLine();

                switch (input)
                {
                    case "":
                    case "d":
                        await ReloadSourceFilesAsync(project);
                        
                        designTransformed = designTransformer.Transform(project, designTime: true);

                        foreach (var sourceFile in designTransformed.SourceFiles)
                        {
                            // TODO: figure out a better way to handle this
                            var newPath = Path.Combine(outputDirectory, "design",
                                Path.ChangeExtension(Path.GetFileName(sourceFile.Path), ".cs"));

                            Directory.CreateDirectory(Path.GetDirectoryName(newPath));

                            File.WriteAllText(newPath, sourceFile.GetText());
                        }

                        break;
                    case "b":
                        await ReloadSourceFilesAsync(project);
                        
                        var buildTransformed = buildTransformer.Transform(project, designTime: false);

                        foreach (var sourceFile in buildTransformed.SourceFiles)
                        {
                            var newPath = Path.Combine(outputDirectory, "build",
                                Path.ChangeExtension(Path.GetFileName(sourceFile.Path), ".cs"));

                            Directory.CreateDirectory(Path.GetDirectoryName(newPath));

                            File.WriteAllText(newPath, sourceFile.GetText());
                        }

                        break;
                    case "i":
                        if (designTransformed == null)
                        {
                            Console.WriteLine("Have to perform design-time transformation before accessing IntelliSense.");
                            break;
                        }

                        var roslynProject = ToRoslynProject(designTransformed);
                        var document = roslynProject.Documents.First();

                        var completionService = CompletionService.GetService(document);

                        var rootNode = await document.GetSyntaxRootAsync();

                        var missingToken = rootNode.DescendantTokens().FirstOrDefault(t => t.IsMissing);

                        if (missingToken == default)
                        {
                            Console.WriteLine("Didn't find any missing tokens, not sure where to run IntelliSense.");
                            break;
                        }

                        var completions = await completionService.GetCompletionsAsync(
                            document, missingToken.GetLocation().SourceSpan.Start);

                        if (completions == null)
                        {
                            Console.WriteLine("No completions.");
                            break;
                        }
                        
                        foreach (var item in completions.Items)
                        {
                            Console.WriteLine(item);
                        }
                        break;
                    default:
                        Console.WriteLine($"Invalid input {input}.");
                        break;
                }
            }
        }

        private static async Task ReloadSourceFilesAsync(Syntax.Project project)
        {
            for (int i = 0; i < project.SourceFiles.Count; i++)
            {
                project.SourceFiles[i] = await SourceFile.OpenAsync(project.SourceFiles[i].Path);
            }
        }

        private static Roslyn::Project ToRoslynProject(Syntax.Project projectTransformer)
        {
            var workspace = new AdhocWorkspace();
            var roslynProject = workspace.AddProject("Project", LanguageNames.CSharp);
            
            foreach (var sourceFile in projectTransformer.SourceFiles)
            {
                roslynProject = roslynProject.AddDocument(sourceFile.Path, sourceFile.GetText()).Project;
            }

            return roslynProject;
        }

        private static void Usage()
        {
            Console.WriteLine("Usage: transformation-assembly input-files");
            Console.WriteLine("       -i output-directory transformation-assembly input-files");
        }
    }
}
