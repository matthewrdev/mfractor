using System;
using System.ComponentModel.Composition;
using MFractor.Configuration;

namespace MFractor.Images.Settings
{
    [Export(typeof(IImageFeatureSettings))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class ImageFeatureSettings : IImageFeatureSettings
    {
        readonly Lazy<IUserOptions> userOptions;
        public IUserOptions UserOptions => userOptions.Value;

        [ImportingConstructor]
        public ImageFeatureSettings(Lazy<IUserOptions> userOptions)
        {
            this.userOptions = userOptions;
        }

        public const string MinimumAndroidDensityKey = "com.mfractor.settings.image_tools.minimum_android_density";

        public AndroidImageDensities MinimumAndroidDensity
        {
            get => UserOptions.Get(MinimumAndroidDensityKey, AndroidImageDensities.Low);
            set => UserOptions.Set(MinimumAndroidDensityKey, value);
        }

        public const string TinyPNGApiKeyKey = "com.mfractor.settings.image_tools.tiny_png_api_key";

        public string TinyPNGApiKey
        {
            get => UserOptions.Get(TinyPNGApiKeyKey, string.Empty);
            set => UserOptions.Set(TinyPNGApiKeyKey, value);
        }

        public const string DefaultIOSResourceTypeKey = "com.mfractor.settings.image_tools.default_ios_asset_kind";

        public ImageResourceType DefaultIOSResourceType
        {
            get => UserOptions.Get(DefaultIOSResourceTypeKey, ImageResourceType.AssetCatalog);
            set => UserOptions.Set(DefaultIOSResourceTypeKey, value);
        }

        public const string DefaultAndroidResourceTypeKey = "com.mfractor.settings.image_tools.default_android_asset_kind";

        public ImageResourceType DefaultAndroidResourceType
        {
            get => UserOptions.Get(DefaultAndroidResourceTypeKey, ImageResourceType.Drawable);
            set => UserOptions.Set(DefaultAndroidResourceTypeKey, value);
        }
    }
}
