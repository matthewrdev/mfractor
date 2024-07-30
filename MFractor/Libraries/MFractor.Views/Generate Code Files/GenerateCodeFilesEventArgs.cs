using System;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.GenerateCodeFiles
{
    public class GenerateCodeFilesEventArgs : EventArgs
    {
        public GenerateCodeFilesResult Result { get; }

        public GenerateCodeFilesEventArgs(GenerateCodeFilesResult result)
        {
            Result = result;
        }
    }
}
