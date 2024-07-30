using System.Collections.Generic;
using System.IO;
using MFractor.Utilities;

namespace MFractor.Code.Scaffolding
{
    public class ScaffoldingInput : IScaffoldingInput
    {
        public ScaffoldingInput(string input)
        { 
            RawInput = input ?? string.Empty;
            var correctInput = PathHelper.CorrectDirectorySeparatorsInPath(input);

            if (!string.IsNullOrEmpty(RawInput))
            {
                Name = Path.GetFileName(correctInput) ?? string.Empty;
                NameNoExtension = Path.GetFileNameWithoutExtension(Name) ?? string.Empty;
                Extension = Path.GetExtension(Name) ?? string.Empty;

                FolderPath = Path.GetDirectoryName(correctInput) ?? string.Empty;
                VirtualFolderPath = PathHelper.CorrectDirectorySeparatorsInPath(FolderPath).Split(System.IO.Path.DirectorySeparatorChar);
            }
        }

        public string RawInput { get; }

        public string Name { get; } = string.Empty;

        public string NameNoExtension { get; } = string.Empty;

        public string Extension { get; } = string.Empty;

        public IReadOnlyList<string> VirtualFolderPath { get; } = new List<string>();

        public string FolderPath { get; } = string.Empty;

        public bool HasInput => !string.IsNullOrEmpty(RawInput);
    }
}