using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Images
{
    /// <summary>
    /// A bundle for passing arguments onto the missing images code fix.
    /// </summary>
    public class MissingImageResourceBundle
    {
        /// <summary>
        /// The projects that are missing the image resource.
        /// </summary>
        public IReadOnlyList<Project> MissingProjects { get; }

        /// <summary>
        /// All the projects that contain mobile image assets.
        /// </summary>
        public IReadOnlyList<Project> AllProjects { get; }

        /// <summary>
        /// The suggested image asset.
        /// </summary>
        public string Suggestion { get; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="MFractor.Maui.CodeAnalysis.Images.MissingImageResourceBundle"/> class.
        /// </summary>
        /// <param name="missingProjects">Missing projects.</param>
        /// <param name="allProjects">All projects.</param>
        public MissingImageResourceBundle(IReadOnlyList<Project> missingProjects,
                                          IReadOnlyList<Project> allProjects,
                                          string suggestion)
        {
            MissingProjects = missingProjects;
            AllProjects = allProjects;
            Suggestion = suggestion;
        }
    }
}
