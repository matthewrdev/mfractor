using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFractor.Models;
using Microsoft.CodeAnalysis;

namespace MFractor.Views.ViewModels
{
    public class ProjectItemViewModel : ObservableBase
    {

        public string Name { get; set; }

        bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => SetProperty(ref isSelected, value);
        }

        public static ProjectItemViewModel FromProject(Project project, bool isSelected = true)
        {
            return new ProjectItemViewModel
            {
                Name = project.Name,
                IsSelected = isSelected,
            };
        }

    }
}
