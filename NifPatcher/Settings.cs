using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mutagen.Bethesda.Synthesis.Settings;
namespace NifPatcher {
    public class Settings {
        [SynthesisTooltip(@"")]
        public string ruleDirectory = @"%DATA%\NifPatcher\Rules";
        [SynthesisTooltip(@"This directory is used as the root to determine the relative paths of files. It is recommended to use the data directory.")]
        public string rootPath = "%DATA%";
        [SynthesisTooltip(@"These directories inside the root directory are processed. To process everything, delete all entries.")]
        public List<string> inPaths = new() { 
            @"meshes\armor",
            @"meshes\weapons",
            @"meshes\dlc01\armor",
            @"meshes\dlc01\weapons",
            @"meshes\dlc02\armor",
            @"meshes\dlc02\weapons",
        };
        [SynthesisTooltip(@"The patched files are placed in this directory.")]
        public string outPath = "";
    }
}
