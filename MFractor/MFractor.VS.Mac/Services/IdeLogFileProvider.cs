using System.ComponentModel.Composition;
using System.IO;
using MFractor.Logging;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILogFileProvider))]
    class IdeLogFileProvider : ILogFileProvider
    {
        public string LogDirectory
        {
            get
            {
                var profile = MonoDevelop.Core.UserProfile.Current;
                if (profile != null && System.IO.Directory.Exists(profile.LogDir))
                {
                    return profile.LogDir;
                }

                return string.Empty;
            }
        }

        public string CurrentLogFile
        {
            get
            {
                var logDirectory = LogDirectory;
                if (!Directory.Exists(logDirectory))
                {
                    return string.Empty;
                }

                return Path.Combine(logDirectory, "Ide.log");
            }
        }
    }
}