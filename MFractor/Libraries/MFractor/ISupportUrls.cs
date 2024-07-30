using System;
namespace MFractor
{
    /// <summary>
    /// A collection of URLs that a customer can use to access various support channels.
    /// </summary>
    public interface IProductUrls
    {
        string ProductUrl { get; }

        string BuyUrl { get; }

        string DocumentationUrl { get; }

        string VersionReleaseUrl { get; }

        string FeedbackUrl { get; }

        string GitterSupportUrl { get; }

        string SlackSupportUrl { get; }
    }
}
