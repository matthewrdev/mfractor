using System.ComponentModel.Composition;
using MFractor.IOC;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.About
{
    public class AboutDialog : Xwt.Dialog
    {
        VBox root;

        Label welcomeText;
        Label versionText;
        Label copyrightText;
        Button copyBuildInformationButton;

        ImageView image;

        [Import]
        IProductInformation ProductInformation { get; set; }

        [Import]
        IEnvironmentDetailsService EnvironmentDetailsService { get; set; }

        [Import]
        IClipboard Clipboard { get; set; }

        public AboutDialog()
        {
            Resolver.ComposeParts(this);

            Resizable = false;

            Title = "About MFractor";
            Icon = Image.FromResource("mfractor_logo.png");

            Build();
        }

        void Build()
        {
            root = new VBox();

            image = new ImageView(Xwt.Drawing.Image.FromResource("mfractor_logo.png").WithSize(250, 247))
            {
                HorizontalPlacement = WidgetPlacement.Center
            };

            welcomeText = new Label("MFractor For " + ProductInformation.ProductVariant);
            welcomeText.TextAlignment = Alignment.Center;
            welcomeText.Font = Font.SystemFont.WithSize(14).WithWeight(FontWeight.Bold);

            versionText = new Label("Version " + ProductInformation.Version.ToShortString());
            copyrightText = new Label("Copyright © Matthew Robbins 2015 - 2022");

            copyBuildInformationButton = new Button();
            copyBuildInformationButton.Label = "Copy Build Details Into Clipboard";
            copyBuildInformationButton.Clicked += async (sender, e) =>
            {
                var buildInfo = "";

                buildInfo = string.Join("\n", EnvironmentDetailsService.Details);

                Clipboard.Text = buildInfo;

                copyBuildInformationButton.Label = "Copied!";
                await System.Threading.Tasks.Task.Delay(3000);
                copyBuildInformationButton.Label = "Copy Build Details Into Clipboard";
            };

            root.PackStart(image);
            root.PackStart(welcomeText);
            root.PackStart(versionText);
            root.PackStart(copyrightText);
            root.PackStart(copyBuildInformationButton);

            Content = root;
        }
    }
}
