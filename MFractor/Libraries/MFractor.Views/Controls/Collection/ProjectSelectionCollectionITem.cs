using System;
using Microsoft.CodeAnalysis;
using MFractor.Utilities;
using Xwt.Drawing;

namespace MFractor.Views.Controls.Collection
{
    public class ProjectSelectionCollectionItem : ICollectionItem
    {
        public ProjectSelectionCollectionItem(Project project)
        {
            Project = project;
        }

        public Project Project { get; }

        public Image Icon => null;

        public string DisplayText => Project.Name;

        public string SecondaryDisplayText => string.Empty;

        public string SearchText => DisplayText.ToLower().RemoveDiacritics();

        public bool IsChecked { get; set; } = true;
    }
}
