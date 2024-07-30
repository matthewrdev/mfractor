using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Images.Settings;
using MFractor.IOC;
using MFractor.Utilities;

namespace MFractor.Images
{
    public static class ImageDensityHelper
    {
        private static readonly Lazy<IImageFeatureSettings> imageFeatureSettings = new Lazy<IImageFeatureSettings>(Resolver.Resolve<IImageFeatureSettings>);
        static IImageFeatureSettings ImageFeatureSettings => imageFeatureSettings.Value;

        public static IReadOnlyList<ImageDensity> BuildAndroidImageDensities()
        {
            return BuildImageDensities<AndroidImageDensities>((names, scales) =>
            {
                var minimumDensity = (int)Resolver.Resolve<IImageFeatureSettings>().MinimumAndroidDensity;

                if (minimumDensity > 0)
                {
                    names.RemoveRange(0, minimumDensity);
                    scales.RemoveRange(0, minimumDensity);
                }
            });
        }

        public static IReadOnlyList<ImageDensity> BuildAppleUnifiedImageDensities()
        {
            return BuildImageDensities<AppleUnifiedImageDensities>();
        }

        static IReadOnlyList<ImageDensity> BuildImageDensities<TEnum>(Action<List<Tuple<string, int>>, List<Tuple<double, int>>> filteringPredicate = null) where TEnum : System.Enum
        {
            List<Tuple<string, int>> names = null;
            List<Tuple<double, int>> scales = null;

            names = EnumHelper.GetDisplayValues<TEnum>();
            scales = EnumHelper.GetScaleValues<TEnum>();

            if (filteringPredicate != null)
            {
                filteringPredicate(names, scales);
            }

            if (names == null
                || scales == null
                || names.Count != scales.Count)
            {
                throw new InvalidOperationException($"The names and scales of the image density for {nameof(TEnum)} do not match");
            }

            var result = new List<ImageDensity>();
            for (var i = 0; i < names.Count; ++i)
            {
                var name = names[i].Item1;
                var scale = scales.First(s => s.Item2 == names[i].Item2).Item1;

                result.Add(new ImageDensity(name, scale));
            }

            return result;
        }

    }
}

