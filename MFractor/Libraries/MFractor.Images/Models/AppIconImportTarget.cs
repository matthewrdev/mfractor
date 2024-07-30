using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.Models
{
    public class AppIconImportTarget
    {
        public Project TargetProject { get; }

        public IEnumerable<IconImage> Icons { get; }

        public bool IncludeAdaptiveIcons { get; }

        public AppIconImportTarget(Project project, bool includeAdaptiveIcons)
        {
            TargetProject = project;
            IncludeAdaptiveIcons = includeAdaptiveIcons;

            if (project.IsAndroidProject())
            {
                var icons = AppIconSet.AndroidIcons().ToList();
                if (IncludeAdaptiveIcons)
                {
                    icons.AddRange(AppIconSet.AndroidAdaptiveIcons());
                }

                Icons = icons;

                return;
            }
            else if (project.IsAppleUnifiedProject())
            {
                Icons = AppIconSet.IOSIcons();
                return;
            }

            throw new InvalidOperationException("The Project type is not supported.");
        }
    }
}
