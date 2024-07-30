using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace MFractor.Images
{
    /// <summary>
    /// 
    /// </summary>
    public class ImportImageOperation
    {
        /// <summary>
        /// The project that this downsampling operation will go into.
        /// </summary>
        public Project TargetProject { get; }

        /// <summary>
        /// The virtual folder path that the image should be added to.
        /// </summary>
        [Obsolete]
        public string FolderPath { get; }

        /// <summary>
        /// The name of the new image asset.
        /// </summary>
        public string ImageName { get; }

        /// <summary>
        /// The file path to the source image asset that will be used to generate the new density assets for the any
        /// </summary>
        public string AnyAppearanceImageFilePath { get; }

        /// <summary>
        /// The file path to the source 
        /// </summary>
        public string DarkModeImageFilePath { get; }

        /// <summary>
        /// Describes the type Resource Type for Image: Asset Catalog or Bundle Resource for iOS, Drawable or MipMap for Android.
        /// </summary>
        public ImageResourceType ResourceType { get; }

        /// <summary>
        /// Of the <see cref="Densities"/> that are to be generated, describes the size of the source image relative to the users selected output sizes.
        /// </summary>
        public ImageDensity SourceDensity { get; }

        /// <summary>
        /// The <see cref="ImageDensity"/>'s to generate.
        /// </summary>
        public IReadOnlyList<ImageDensity> Densities { get; }

        /// <summary>
        /// The maximum size
        /// </summary>
        public ImageSize SourceSize { get; }

        public ImportImageOperation(Project targetProject,
                                    string imageName,
                                    string anyAppearanceImageFilePath,
                                    string darkModeImageFilePath,
                                    ImageResourceType resourceType,
                                    ImageDensity sourceDensity,
                                    IReadOnlyList<ImageDensity> densities,
                                    ImageSize sourceSize,
                                    string folderPath = null)
        {
            TargetProject = targetProject ?? throw new ArgumentNullException(nameof(targetProject));
            ImageName = imageName;
            AnyAppearanceImageFilePath = anyAppearanceImageFilePath;
            DarkModeImageFilePath = darkModeImageFilePath;
            ResourceType = resourceType;
            SourceDensity = sourceDensity;
            Densities = densities;
            SourceSize = sourceSize;
            FolderPath = folderPath;
        }

        public ImportImageOperation(Project targetProject,
                                    string imageName,
                                    string anyAppearanceImageFilePath,
                                    string darkModeImageFilePath,
                                    string resourceType,
                                    ImageDensity sourceDensity,
                                    IReadOnlyList<ImageDensity> densities,
                                    ImageSize sourceSize,
                                    string folderPath = null)
            : this(targetProject,
                  imageName,
                  anyAppearanceImageFilePath,
                  darkModeImageFilePath,
                  ImageAssetHelper.ToImageResourceType(resourceType),
                  sourceDensity,
                  densities,
                  sourceSize,
                  folderPath)
        {
        }
    }
}
