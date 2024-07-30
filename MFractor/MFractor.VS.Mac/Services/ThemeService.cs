using System;
using System.ComponentModel.Composition;
using Preferences = MonoDevelop.Ide.IdePreferences;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IThemeService))]
    class ThemeService : IThemeService
    {
        Preferences IdePreferences => MonoDevelop.Ide.IdeApp.Preferences;

        public Theme CurrentTheme
        {
            get
            {
                if (IdePreferences.UserInterfaceTheme == MonoDevelop.Ide.Theme.Dark)
                {
                    return Theme.Dark;
                }

                return Theme.Light;
            }
        }
    }
}