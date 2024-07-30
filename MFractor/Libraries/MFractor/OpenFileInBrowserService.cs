using System;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Utilities;

namespace MFractor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IOpenFileInBrowserService))]
	class OpenFileInBrowserService : IOpenFileInBrowserService
	{
        readonly Lazy<IPlatformService> platformService;
        public IPlatformService PlatformService => platformService.Value;

        [ImportingConstructor]
        public OpenFileInBrowserService(Lazy<IPlatformService> platformService)
        {
            this.platformService = platformService;
        }

		public bool OpenAndSelect(string path)
		{
            if (PlatformService.IsOsx)
            {
                OpenAndSelectMac(path);
                return true;
            }

            if (PlatformService.IsWindows)
            {
                OpenAndSelectWindows(path);
                return true;
            }

            return false;
		}

        /// <summary>
        /// Opens and selects the given <paramref name="path"/> on macOS using Finder.
        /// </summary>
        /// <param name="path">Path.</param>
		public void OpenAndSelectMac(string path)
		{
            var openInsidesOfFolder = false;

            // try mac
            var macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes

			if (Directory.Exists(macPath)) // if path requested is a folder, automatically open insides of that folder
			{
				openInsidesOfFolder = true;
			}

			if (!macPath.StartsWith("\"", StringComparison.Ordinal))
			{
				macPath = "\"" + macPath;
			}
			if (!macPath.EndsWith("\"", StringComparison.Ordinal))
			{
				macPath += "\"";
			}

			var arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;

			try
			{
				System.Diagnostics.Process.Start("open", arguments);
			}
			catch(System.ComponentModel.Win32Exception e)
			{
				// tried to open mac finder in windows
				// just silently skip error
				// we currently have no platform define for the current OS we are in, so we resort to this
				e.HelpLink = ""; // do anything with this variable to silence warning about not using it
			}
		}

        /// <summary>
        /// Opens and selects the given <paramref name="path"/> on Windows using Explorer.
        /// </summary>
        /// <param name="path">Path.</param>
		public void OpenAndSelectWindows(string path)
		{
            var openInsidesOfFolder = false;

            // try windows
            var winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes

			if (Directory.Exists(winPath)) // if path requested is a folder, automatically open insides of that folder
			{
				openInsidesOfFolder = true;
			}
			try
			{
				System.Diagnostics.Process.Start("explorer.exe", (openInsidesOfFolder ? "/root," : "/select,") + winPath);
			}
			catch(System.ComponentModel.Win32Exception e)
			{
				// tried to open win explorer in mac
				// just silently skip error
				// we currently have no platform define for the current OS we are in, so we resort to this
				e.HelpLink = ""; // do anything with this variable to silence warning about not using it
			}
		}
	}
}

