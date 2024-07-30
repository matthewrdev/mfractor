namespace MFractor.Images.Settings
{
    public interface IImageFeatureSettings
    {
        AndroidImageDensities MinimumAndroidDensity { get; set; }

        string TinyPNGApiKey { get; set; }

        ImageResourceType DefaultIOSResourceType { get; set; }

        ImageResourceType DefaultAndroidResourceType { get; set; }

    }
}
