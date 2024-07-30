using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFractor.Images;
using MFractor.Models;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.VS.Windows.UI.ViewModels
{
    public class ImageImporterTargetProjectViewModel : ObservableBase
    {
        public Project Project { get; }

        bool isIncluded;
        public bool IsIncluded
        {
            get => isIncluded;
            set => SetProperty(ref isIncluded, value);
        }

        public string ProjectName => Project.Name;

        ImageResourceType selectedImageType;
        public ImageResourceType SelectedImageType
        {
            get => selectedImageType;
            set => SetProperty(ref selectedImageType, value);
        }

        ImageDensity selectedImageDensity;
        public ImageDensity SelectedImageDensity {
            get => selectedImageDensity;
            set => SetProperty(ref selectedImageDensity, value);
        }

        string folderPath;
        public string FolderPath
        {
            get => folderPath;
            set => SetProperty(ref folderPath, value);
        }

        public IReadOnlyList<ImageResourceType> ImageTypes { get; }

        public IReadOnlyList<ImageDensity> ImageDensities { get; }

        public IReadOnlyList<ImageDensity> ConsideredImageDensities =>
            ImageDensities
                .Where(i => i.Scale <= SelectedImageDensity?.Scale)
                .OrderByDescending(i => i.Scale)
                .ToList();

        public ImageImporterTargetProjectViewModel(Project project)
        {
            this.Project = project;

            ImageTypes = GetImageTypesOfProject(project);
            SelectedImageType = ImageTypes.First();
            ImageDensities = GetDensitiesOfProject(project);
            SelectedImageDensity = GetDefaultImageDensityOfProject(ImageDensities, project);
        }

        static ImageDensity GetDefaultImageDensityOfProject(IEnumerable<ImageDensity> densities, Project project)
        {
            if (project.IsAndroidProject())
            {
                return densities.First(d => d.Name == EnumHelper.GetDisplayValue(AndroidImageDensities.ExtraExtraExtraHigh).Item1);
            }

            if (project.IsAppleUnifiedProject())
            {
                return densities.First(d => d.Name == EnumHelper.GetDisplayValue(AppleUnifiedImageDensities.Triple).Item1);
            }

            return default;
        }

        static IReadOnlyList<ImageResourceType> GetImageTypesOfProject(Project project)
        {
            if (project.IsMauiProject())
            {
                return new List<ImageResourceType>()
                {
                    ImageResourceType.MauiImage
                };
            }
            if (project.IsAndroidProject())
            {
                return new List<ImageResourceType>()
                {
                    ImageResourceType.Drawable,
                    ImageResourceType.MipMap    
                };
            }
            else if (project.IsAppleUnifiedProject())
            {
                return new List<ImageResourceType>()
                {
                    ImageResourceType.AssetCatalog,
                    ImageResourceType.BundleResource
                };
            }

            return Array.Empty<ImageResourceType>();
        }

        static IReadOnlyList<ImageDensity> GetDensitiesOfProject(Project project)
        {
            if (project.IsAndroidProject())
            {
                return GetDensitiesOfPlatform<AndroidImageDensities>();
            }
            else if (project.IsAppleUnifiedProject())
            {
                return GetDensitiesOfPlatform<AppleUnifiedImageDensities>();
            }

            return Array.Empty<ImageDensity>();
        }

        static IReadOnlyList<ImageDensity> GetDensitiesOfPlatform<TPlatformDensities>() where TPlatformDensities : System.Enum => 
            ((TPlatformDensities[])Enum.GetValues(typeof(TPlatformDensities)))
                .Select(d => ImageDensity.FromEnumValue(d))
                .OrderByDescending(d => d.Scale)
                .ToList();
    }
}
