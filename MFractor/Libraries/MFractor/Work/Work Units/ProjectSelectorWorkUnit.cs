using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MFractor.Work.WorkUnits
{
    public enum ProjectSelectorMode
    {
        Single,

        Multiple
    }

    /// <summary>
    /// A workUnit that show a selector dialog displaying a selection of projects.
    /// </summary>
    public class ProjectSelectorWorkUnit : WorkUnit
    {
        /// <summary>
        /// The projects a user may choose.
        /// </summary>
        /// <value>The choices.</value>
        public IReadOnlyList<Project> Choices { get; }

        public IReadOnlyList<Project> EnabledChoices { get; set; }

        /// <summary>
        /// The title of the dialog.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; }

        public string Description { get; }

        public ProjectSelectorMode Mode { get; set; } = ProjectSelectorMode.Multiple;

        public Func<IReadOnlyList<Project>, IReadOnlyList<IWorkUnit>> ProjectSelectionCallback { get; }

        public ProjectSelectorWorkUnit(IReadOnlyList<Project> choices,
                                      string description,
                                      string title,
                                      Func<IReadOnlyList<Project>, IReadOnlyList<IWorkUnit>> projectSelectionCallback)
        {
            if (choices == null || !choices.Any())
            {
                throw new ArgumentException(nameof(choices) + " must not be null or empty.");
            }

            Choices = choices;
            EnabledChoices = choices;
            Title = title;
            Description = description;
            ProjectSelectionCallback = projectSelectionCallback ?? throw new ArgumentNullException(nameof(projectSelectionCallback));
        }
    }
}
