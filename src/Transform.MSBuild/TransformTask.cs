using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CSharpE.Syntax;
using CSharpE.Transform.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Nerdbank.MSBuildExtension;
using SourceFile = CSharpE.Transform.Execution.SourceFile;

namespace CSharpE.Transform.MSBuild
{
    public class TransformTask : ContextIsolatedTask
    {
        // https://github.com/dotnet/corefx/blob/778fea7/src/Common/src/System/HResults.cs#L74
        private const int CorELoadingReferenceAssembly = unchecked((int)0x80131058);

        [Required]
        public ITaskItem[] InputSourceFiles { get; set; }

        [Required]
        public string InputReferences { get; set; }

        [Output]
        public ITaskItem[] OutputSourceFiles { get; set; }

        protected override bool ExecuteIsolated()
        {
            OutputSourceFiles = InputSourceFiles;

            var assemblyPaths = 
                (InputReferences ?? string.Empty).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            var transformations = new List<ITransformation>();

            foreach (var assemblyPath in assemblyPaths)
            {
                Assembly assembly;
                try
                {
                    assembly = LoadAssemblyByPath(assemblyPath);
                }
                catch (BadImageFormatException ex) when (ex.HResult == CorELoadingReferenceAssembly)
                {
                    // Reference assemblies can't contain transformer implementations
                    continue;
                }
                catch (FileLoadException) when (Path.GetFileName(assemblyPath).StartsWith("CSharpE."))
                {
                    // On .Net Core, we're getting "Assembly with same name is already loaded"
                    continue;
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"Could not load assembly '{assemblyPath}': {ex.GetType()}: {ex.Message}");
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
                return true;

            var sourceFiles =
                InputSourceFiles.Select(item => SourceFile.OpenAsync(item.ItemSpec).GetAwaiter().GetResult());

            var references = assemblyPaths.Select(path => new AssemblyReference(path));

            var transformer = new ProjectTransformer(sourceFiles, references, transformations);

            var result = transformer.Transform(designTime: false);

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

            var outputFiles = new List<ITaskItem>();

            foreach (var file in result.SourceFiles)
            {
                var outputFilePath = GetUniqueFilePath(file.Path);

                File.WriteAllText(outputFilePath, file.Text);

                outputFiles.Add(new TaskItem(outputFilePath));
            }

            OutputSourceFiles = outputFiles.ToArray();

            // TODO: handle added references?

            return true;
        }
    }
}
