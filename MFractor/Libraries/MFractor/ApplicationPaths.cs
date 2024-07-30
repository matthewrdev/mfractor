using System;
using System.ComponentModel.Composition;
using System.IO;

namespace MFractor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IApplicationPaths))]
    class ApplicationPaths : IApplicationPaths
    {
        [ImportingConstructor]
        public ApplicationPaths(IPlatformService platformService)
        {
            if (platformService.IsWindows)
            {
                ApplicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mfractor");
            }
            else
            {
                ApplicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library", "mfractor");
            }

            SetupAppDataFolder();
            SetupTempDataFolder();
        }

        void SetupAppDataFolder()
        {
            if (!Directory.Exists(ApplicationDataPath))
            {
                Directory.CreateDirectory(ApplicationDataPath);
            }
        }

        void SetupTempDataFolder()
        {
            if (!Directory.Exists(ApplicationTempPath))
            {
                Directory.CreateDirectory(ApplicationTempPath);
            }
        }

        public string ApplicationLogsPath => Path.Combine(ApplicationDataPath, "logs");

        public string ApplicationDataPath { get; }

        public string ApplicationTempPath => Path.Combine(ApplicationDataPath, ".temp");
    }
}
