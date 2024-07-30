using System;
using System.Collections.Generic;
using System.Text;
using MFractor.Models;
using Microsoft.CodeAnalysis;

namespace MFractor.Views.ViewModels
{
    /// <summary>
    /// A View Model for a selectable project as a target of a feature.
    /// </summary>
    public class TargetProjectViewModel : ObservableBase
    {
        /// <summary>
        /// The wrapped project object.
        /// </summary>
        public Project Project { get; }

        bool isIncluded;
        /// <summary>
        /// Flag indicating if the project is currently selected for the desired operation.
        /// </summary>
        public bool IsIncluded
        {
            get => isIncluded;
            set => SetProperty(ref isIncluded, value);
        }

        /// <summary>
        /// Gets the name of the wrapped project.
        /// </summary>
        public string ProjectName => Project.Name;

        public TargetProjectViewModel(Project project)
        {
            Project = project;
        }
    }
}
