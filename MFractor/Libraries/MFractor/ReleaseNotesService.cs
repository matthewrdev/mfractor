using System;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Versioning;

namespace MFractor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IReleaseNotesService))]
    class ReleaseNotesService : IReleaseNotesService
    {
        const string releasesFolder = ".releases";
        const string lastReleaseFile = ".last_release";

        readonly Lazy<IPlatformService> platformService;
        public IPlatformService PlatformService => platformService.Value;

        readonly Lazy<IApplicationPaths> applicationPaths;
        public IApplicationPaths ApplicationPaths => applicationPaths.Value;

        [ImportingConstructor]
        public ReleaseNotesService(Lazy<IPlatformService> platformService,
                                   Lazy<IApplicationPaths> applicationPaths)
        {
            this.platformService = platformService;
            this.applicationPaths = applicationPaths;
        }

        public VersionUpdateStatus GetVersionUpdateStatus(string version)
        {
            var releasesFolder = Path.Combine(ApplicationPaths.ApplicationDataPath, ReleaseNotesService.releasesFolder);
            if (!Directory.Exists(releasesFolder))
            {
                Directory.CreateDirectory(releasesFolder);
                return VersionUpdateStatus.None;
            }

            var releasesFile = Path.Combine(releasesFolder, lastReleaseFile);

            if (!File.Exists(releasesFile))
            {
                File.WriteAllText(releasesFile, version);
                return VersionUpdateStatus.None;
            }

            var lastRelease = File.ReadAllText(releasesFile).Trim();

            if (!SemanticVersion.TryParse(version, out var thisVersion)
                || !SemanticVersion.TryParse(lastRelease, out var lastVersion))
            {
                return VersionUpdateStatus.None;
            }

            SetLatestVersion(version);

            // Only considered a new version when the major or minor was bumped.
            if (thisVersion.Major > lastVersion.Major)
            {
                return VersionUpdateStatus.SignificantVersionUpgrade;
            }

            if (thisVersion.Minor > lastVersion.Minor)
            {
                return VersionUpdateStatus.SignificantVersionUpgrade;
            }

            if (thisVersion.Patch > lastVersion.Patch)
            {
                return VersionUpdateStatus.VersionUpgrade;
            }

            return VersionUpdateStatus.None;
        }

        public void SetLatestVersion(string version)
        {
            var releasesFolder = Path.Combine(ApplicationPaths.ApplicationDataPath, ReleaseNotesService.releasesFolder);
            if (!Directory.Exists(releasesFolder))
            {
                Directory.CreateDirectory(releasesFolder);
            }

            var releasesFile = Path.Combine(releasesFolder, lastReleaseFile);

            File.WriteAllText(releasesFile, version);
        }

        public string GetReleaseNotesUrl(SemanticVersion semanticVersion)
        {
            if (semanticVersion is null)
            {
                throw new ArgumentNullException(nameof(semanticVersion));
            }

            const string template = "https://docs.mfractor.com/release-notes/$platform$/v$major$/v$major$.$minor$#v$major$$minor$$patch$";

            var url = template.Replace("$platform$", PlatformService.Name.ToLower())
                              .Replace("$major$", semanticVersion.Major.ToString())
                              .Replace("$minor$", semanticVersion.Minor.ToString())
                              .Replace("$patch$", semanticVersion.Patch.ToString());

            return url;
        }
    }
}
