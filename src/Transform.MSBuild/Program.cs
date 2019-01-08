using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CSharpE.Syntax;
using CSharpE.Transform.Execution;

namespace CSharpE.Transform.MSBuild
{
    static class Program
    {
        static void Main()
        {
            var assemblyPaths = Console.ReadLine().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            var inputSourceFiles = new List<string>();

            string line;

            while ((line = Console.ReadLine()) != null)
            {
                inputSourceFiles.Add(line);
            }

            try
            {
                var outputSourceFiles = Execute(inputSourceFiles, assemblyPaths, Console.WriteLine).ToList();

                Console.WriteLine("output");

                foreach (var file in outputSourceFiles)
                {
                    Console.WriteLine(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error");

                Console.WriteLine(ex);
            }
        }

        // https://github.com/dotnet/corefx/blob/778fea7/src/Common/src/System/HResults.cs#L74
        private const int CorELoadingReferenceAssembly = unchecked((int)0x80131058);

        static IEnumerable<string> Execute(IEnumerable<string> inputSourceFiles, IEnumerable<string> assemblyPaths, Action<string> reportWarning)
        {
            var transformations = new List<ITransformation>();

            foreach (var assemblyPath in assemblyPaths)
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(assemblyPath);
                }
                catch (BadImageFormatException ex) when (ex.HResult == CorELoadingReferenceAssembly)
                {
                    // Reference assemblies can't contain transformer implementations
                    continue;
                }
#if NETCOREAPP2_0
                catch (FileLoadException)
                    when (Path.GetFileName(assemblyPath) is var fileName && (fileName.StartsWith("System.") || fileName == "Microsoft.Win32.Primitives.dll"))
                {
                    // On .Net Core, we're also getting these harmless exceptions
                    continue;
                }
#endif
                catch (Exception ex)
                {
                    reportWarning($"Could not load assembly '{assemblyPath}': {ex.GetType()}: {ex.Message}");
                    continue;
                }

                IEnumerable<ITransformation> assemblyTransformations;

                try
                {
                    assemblyTransformations = assembly.ExportedTypes
                        .Where(t => typeof(ITransformation).IsAssignableFrom(t) && !t.IsAbstract)
                        .Select(type => (ITransformation)Activator.CreateInstance(type));
                }
                catch when (assembly.GetName().Name == "Microsoft.Build.Tasks.Core")
                {
                    // TODO: this assembly shouldn't be referenced in the first place
                    continue;
                }

                transformations.AddRange(assemblyTransformations);
            }

            if (!transformations.Any())
                yield break;

            var sourceFiles =
                inputSourceFiles.Select(file => SourceFile.OpenAsync(file).GetAwaiter().GetResult());

            var references = assemblyPaths.Select(path => new AssemblyReference(path));

            var transformer = new ProjectTransformer(transformations);

            var result = transformer.Transform(new Project(sourceFiles, references), designTime: false);

            var tmpDirectory = Path.Combine(Directory.GetCurrentDirectory(), "obj", "CSharpE");
            Directory.CreateDirectory(tmpDirectory);

            var fileNameTracker = new Dictionary<string, int>();

            string GetUniqueFilePath(string path)
            {
                var fileName = Path.GetFileName(path);

                var uniqueFileName = fileName;

                if (fileNameTracker.TryGetValue(fileName, out int n))
                {
                    n++;
                    uniqueFileName = Path.GetFileNameWithoutExtension(fileName) + n + Path.GetExtension(fileName);
                }
                else
                {
                    n = 2;
                }

                fileNameTracker[fileName] = n;

                return Path.Combine(tmpDirectory, uniqueFileName);
            }

            foreach (var file in result.SourceFiles)
            {
                var outputFilePath = GetUniqueFilePath(file.Path);

                File.WriteAllText(outputFilePath, file.GetText());

                yield return outputFilePath;
            }
        }
    }
}
