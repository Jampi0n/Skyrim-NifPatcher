using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace NifPatcher {
    public class Program {

        static Lazy<Settings> _Settings = null!;
        public static Settings Settings => _Settings.Value;
        public static async Task<int> Main(string[] args) {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch, new PatcherPreferences() {
                    NoPatch = true
                })
                .SetAutogeneratedSettings(
                    nickname: "Settings",
                    path: "settings.json",
                    out _Settings)
                .SetTypicalOpen(GameRelease.SkyrimSE, "YourPatcher.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state) {

            var outPath = Settings.outPath.Replace("%DATA%", state.DataFolderPath);
            var rootPath = Settings.rootPath.Replace("%DATA%", state.DataFolderPath);

            Console.WriteLine();

            var outNifFiles = Directory.GetFiles(outPath, "*.nif", SearchOption.AllDirectories);
            if(outNifFiles.Length > 0) {
                Console.WriteLine("Warning! " + outNifFiles.Length + " .nif files were detected in the output directory. If the patcher tries to overwrite an existing .nif file in the output directory, the patcher will fail.");
                Console.WriteLine();
            }

            var stopwatch = Stopwatch.StartNew();
            var ruleFiles = Directory.GetFiles(Settings.ruleDirectory.Replace("%DATA%", state.DataFolderPath), "*.txt", SearchOption.AllDirectories);
            foreach(var ruleFile in ruleFiles) {
                Console.WriteLine("Parsing rule file " + ruleFile + " ...");
                var lines = File.ReadAllLines(ruleFile);
                try {
                    RuleParser.ParseRuleBlock(lines);
                } catch(RuleParser.RuleParseException e) {
                    throw new Exception("Cannot parse line in file " + ruleFile + " at line " + e.line + ": " + lines[e.line] + "\n" + "Message:\n" + e.Message);
                }
            }
            Console.WriteLine();

            var inPaths = Settings.inPaths.ToList();

            if(inPaths.Count == 0) {
                inPaths.Add("");
            }

            string[] inFiles;
            var inFilesList = new List<string>();
            foreach(var inPath in inPaths) {
                var newPath = Path.Combine(Settings.rootPath, inPath).Replace("%DATA%", state.DataFolderPath);
                inFilesList.AddRange(Directory.GetFiles(newPath, "*.nif", SearchOption.AllDirectories));
            }
            inFiles = inFilesList.ToArray();
            int progressCounter = 0;
            try {
                Parallel.For(0, inFiles.Length, new ParallelOptions() {
                    MaxDegreeOfParallelism = 8,
                }, (i) => {
                    var inFile = inFiles[i];
                    var relativePath = Path.GetRelativePath(rootPath, inFile);
                    var progress = "" + (i + 1) + "/" + inFiles.Length + ": ";
                    var nif = new NifFileWrapper(inFile);
                    if(RuleParser.PatchNif(nif, relativePath)) {
                        var outFile = Path.Combine(outPath, relativePath);
                        Directory.CreateDirectory(Directory.GetParent(outFile)!.FullName);
                        if(!nif.SaveAs(outFile, false)) {
                            Console.WriteLine("    " + Interlocked.Increment(ref progressCounter) + "/" + inFiles.Length + " " + relativePath + " already exists in output directory");
                            throw new IOException("Tried to overwrite an existing .nif file in the output directory.");
                        } else {
                            Console.WriteLine("    " + Interlocked.Increment(ref progressCounter) + "/" + inFiles.Length + " " + relativePath + " saved");
                        }
                    } else {
                        Console.WriteLine("    " + Interlocked.Increment(ref progressCounter) + "/" + inFiles.Length + " " + relativePath + " no change");
                    }
                });
            } catch(AggregateException e) {
                foreach(var inner in e.InnerExceptions) {
                    throw inner;
                }
            }

            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds + "ms");
        }
    }
}
