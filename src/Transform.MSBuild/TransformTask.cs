using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace CSharpE.Transform.MSBuild
{
    public class TransformTask : Task
    {
        [Required]
        public ITaskItem[] InputSourceFiles { get; set; }

        [Required]
        public string InputReferences { get; set; }

        [Output]
        public ITaskItem[] OutputSourceFiles { get; set; }

        public override bool Execute()
        {
            OutputSourceFiles = InputSourceFiles;

            var assemblyLocation = typeof(Program).Assembly.Location;

            var psi = new ProcessStartInfo
            {
#if NET46
                FileName = assemblyLocation,
#else
                FileName = "dotnet",
                Arguments = assemblyLocation,
#endif
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };

            var process = Process.Start(psi);

            var processInput = process.StandardInput;

            processInput.WriteLine(InputReferences);

            foreach (var item in InputSourceFiles)
            {
                processInput.WriteLine(item.ItemSpec);
            }

            processInput.Close();

            var processOutput = process.StandardOutput;

            string line;

            bool readingOutput = false;

            var outputFiles = new List<ITaskItem>();

            while ((line = processOutput.ReadLine()) != null)
            {
                switch (line)
                {
                    case "error":
                        Log.LogError(processOutput.ReadToEnd());
                        return false;
                    case "output":
                        readingOutput = true;
                        break;
                    default:
                        if (readingOutput)
                            outputFiles.Add(new TaskItem(line));
                        else
                            Log.LogWarning(line);
                        break;
                }
            }

            OutputSourceFiles = outputFiles.ToArray();

            // TODO: handle added references?

            return true;
        }
    }
}
