using System;
namespace MFractor.Views.Licensing
{
    public class LicenseWidgetOptions
    {
        public LicenseWidgetOptions(bool allowActivation,
                                    bool allowDeactivation,
                                    bool showBranding)
        {
            AllowActivation = allowActivation;
            AllowDeactivation = allowDeactivation;
            ShowBranding = showBranding;
        }

        public bool AllowActivation { get; }

        public bool AllowDeactivation { get; }

        public bool ShowBranding { get; }
    }

}
