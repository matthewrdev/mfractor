using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Images;
using MFractor.Images.Settings;
using MFractor.IOC;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;

namespace MFractor.Views.ImageImporter
{
    public class ImageImportTargetProject
    {
        public readonly Project Project;
        public readonly bool Selected;

        IReadOnlyList<ImageDensity> densities;
        public IReadOnlyList<ImageDensity> Densities
        {
            get
            {
                if (densities == null)
                {
                    PopulateDensities();
                }

                return densities;
            }
        }

        public ImageDensity DefaultDensity
        {
            get
            {
                if (densities == null)
                {
                    PopulateDensities();
                }

                ImageDensity max = null;
                if (Project.IsAppleUnifiedProject())
                {
                    max = Densities.FirstOrDefault(density => density.Name == EnumHelper.GetDisplayValue(AppleUnifiedImageDensities.Triple).Item1);

                }
                else if (Project.IsAndroidProject())
                {
                    max = Densities.FirstOrDefault(density => density.Name == EnumHelper.GetDisplayValue(AndroidImageDensities.ExtraExtraExtraHigh).Item1);
                }

                return max;
            }
        }

        void PopulateDensities()
        {
            List<Tuple<string, int>> names = null;
            List<Tuple<double, int>> scales = null;

            if (Project.IsMauiProject())
            {
                names = EnumHelper.GetDisplayValues<MauiImageDensities>();
                scales = EnumHelper.GetScaleValues<MauiImageDensities>();
            }
            else if (Project.IsAppleUnifiedProject())
            {
                names = EnumHelper.GetDisplayValues<AppleUnifiedImageDensities>();
                scales = EnumHelper.GetScaleValues<AppleUnifiedImageDensities>();
            }
            else if (Project.IsAndroidProject())
            {
                names = EnumHelper.GetDisplayValues<AndroidImageDensities>();
                scales = EnumHelper.GetScaleValues<AndroidImageDensities>();

                var minimumDensity = (int)Resolver.Resolve<IImageFeatureSettings>().MinimumAndroidDensity;

                if (minimumDensity > 0)
                {
                    names.RemoveRange(0, minimumDensity);
                    scales.RemoveRange(0, minimumDensity);
                }
            }

            if (names == null
                || scales == null
                || names.Count != scales.Count)
            {
                throw new InvalidOperationException("The names and scales of the image density for the project " + Project.Name + " do not match");
            }

            var temp = new List<ImageDensity>();
            for (var i = 0; i < names.Count; ++i)
            {
                var name = names[i].Item1;
                var scale = scales.First(s => s.Item2 == names[i].Item2).Item1;

                temp.Add(new ImageDensity(name, scale));
            }

            densities = temp;
        }

        public List<ImageResourceType> ImageTypes
        {
            get
            {
                if (Project.IsAppleUnifiedProject())
                {
                    return new List<ImageResourceType>
                    {
                        ImageResourceType.AssetCatalog,
                        ImageResourceType.BundleResource,
                    };
                }

                if (Project.IsAndroidProject())
                {
                    return new List<ImageResourceType>
                    {
                        ImageResourceType.Drawable,
                        ImageResourceType.MipMap,
                    };
                }

                if (Project.IsMauiProject())
                {
                    return new List<ImageResourceType>
                    {
                        ImageResourceType.MauiImage,
                    };
                }

                throw new NotSupportedException("The project is an unknown type. It must be an iOS or Android project");
            }
        }

        public ImageImportTargetProject(Project project, bool selected = true)
        {
            Project = project;
            Selected = selected;
        }
    }
}
