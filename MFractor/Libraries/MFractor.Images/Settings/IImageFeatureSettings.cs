namespace MFractor.Images.Settings
{
    public interface IImageFeatureSettings
    {
        AndroidImageDensities MinimumAndroidDensity { get; set; }

        ImageResourceType DefaultIOSResourceType { get; set; }

        ImageResourceType DefaultAndroidResourceType { get; set; }

    }
}
