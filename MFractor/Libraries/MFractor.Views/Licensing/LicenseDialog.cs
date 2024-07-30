using System.ComponentModel.Composition;
using MFractor.IOC;
using MFractor.Licensing;
using Xwt.Drawing;

namespace MFractor.Views.Licensing
{
    class LicenseDialog : Xwt.Dialog
    {
        readonly LicenseWidget licenseWidget;

        [Import]
        ILicensingService LicensingService { get; set; }

        public LicenseDialog()
        {
            Resolver.ComposeParts(this);

            Width = 400;

            Title = "MFractor Licensing";
            Icon = Image.FromResource("mfractor_logo.png");

            var options = new LicenseWidgetOptions(true, LicensingService.HasLicense, true);

            licenseWidget = new LicenseWidget(options);
            licenseWidget.SetWindowFrame(this);

            Content = licenseWidget;
        }
    }
}
