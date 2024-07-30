using System;
using System.ComponentModel.Composition;

namespace MFractor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IPlatformService))]
    class PlatformService : IPlatformService
	{
        public bool IsWindows => !IsOsx;

        public bool IsLinux => Environment.OSVersion.Platform == PlatformID.Unix;

        public bool IsOsx => Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;

        public string Name
        {
            get
            {
                if (IsOsx)
                {
                    return "Mac";
                }
                else if (IsLinux)
                {
                    return "Linux";
                }
                else
                {
                    return "Windows";
                }
            }
        }

        public DesktopPlatform DesktopPlatform
        {
            get
            {
                if (IsOsx)
                {
                    return DesktopPlatform.MacOS;
                }
                else if (IsLinux)
                {
                    return DesktopPlatform.Linux;
                }
                else
                {
                    return DesktopPlatform.Windows;
                }
            }
        }

    }
}

