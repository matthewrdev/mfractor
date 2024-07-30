namespace MFractor.Maui.FontSizes
{
    public interface IFontSize
    {
        /// <summary>
        /// The name of this font size.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The iOS value of this named font size.
        /// </summary>
        double IOS { get; }

        /// <summary>
        /// The Android value of this named font size.
        /// </summary>
        double Android { get; }

        /// <summary>
        /// The UWP value of this named font size.
        /// </summary>
        double UWP { get; }
    }
}
