using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.Work.WorkUnits
{
    public delegate IReadOnlyList<IWorkUnit> OnImageNameConfirmed(string imageName);

    /// <summary>
    /// An <see cref="IWorkUnit"/> that opens the image import wizard dialog in the IDE.
    /// <para/>
    /// This allows users to easily import images for their Android and iOS projects and generate the down sampled versions of that image.
    /// </summary>
    public class ImportImageAssetWorkUnit : WorkUnit
    {
        public string ImageName { get; set; }

        public string ImageFilePath { get; set; }

        public bool AllowMultipleImports { get; set; } = false;

        public bool LaunchImageManager { get; set; } = false;

        public IReadOnlyList<Project> Projects { get; set; } = new List<Project>();

        public OnImageNameConfirmed OnImageNameConfirmedCallback { get; set; }
    }
}
