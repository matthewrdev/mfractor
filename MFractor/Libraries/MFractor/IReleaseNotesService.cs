using MFractor.Versioning;

namespace MFractor
{
    public interface IReleaseNotesService
    {
        string GetReleaseNotesUrl(SemanticVersion semanticVersion);

        VersionUpdateStatus GetVersionUpdateStatus(string version);

        void SetLatestVersion(string version);

    }
}
