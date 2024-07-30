using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Code.CodeActions;
using MFractor.Configuration;
using MFractor.Editor.XAML.Completion;
using MFractor.Maui.Analysis;
using MFractor.Maui.CodeActions;
using MFractor.IOC;
using MFractor.Licensing;
using Xwt;

namespace MFractor.Views.Onboarding.Panels
{
    public class GettingStartedPanel : VBox, IOnboardingPanel
    {
        [Import]
        public ILicensingService LicensingService { get; set; }

        [Import]
        public IUrlLauncher UrlLauncher { get; set; }

        [Import]
        public IConfigurationEngine ConfigurationEngine { get; set; }

        [Import]
        public IProductInformation ProductInformation { get; set; }

        public GettingStartedPanel()
        {
            VerticalPlacement = WidgetPlacement.Center;
            HorizontalPlacement = WidgetPlacement.Center;
            ExpandHorizontal = true;
            WidthRequest = 660;

            Resolver.ComposeParts(this);

            PackStart(new Label("Thanks for activating MFractor")
            {
                Font = Xwt.Drawing.Font.SystemFont.WithScaledSize(1.5).WithWeight(Xwt.Drawing.FontWeight.Bold)
            });

            PackStart(new HSeparator()
            {
                MarginTop = 10,
                MarginBottom = 10,
            });

            var xamlActions = Resolver.ResolveAll<ICodeAction>().OfType<XamlCodeAction>().Count();
            var analysers = Resolver.ResolveAll<IXmlSyntaxCodeAnalyser>().OfType<XamlCodeAnalyser>().Count();
            var completions = Resolver.ResolveAll<IXamlCompletionService>().Count();


            PackStart(new Label("Powerful XAML Tooling")
            {
                Font = Xwt.Drawing.Font.SystemFont.WithScaledSize(1.5).WithWeight(Xwt.Drawing.FontWeight.Bold)
            });

            PackStart(new Label($"At it's core, MFractor includes a suite of tools that make it faster, easier and safer to write beatiful XAML.\n\nThis includes {xamlActions} refactorings, {analysers} XAML inspections, {completions}+ IntelliSense completions plus detailed tooltips, navigation shortcuts and more.")
            {
                Wrap = WrapMode.Word,
            });
            PackStart(new LinkLabel("Learn more...") { Uri = new Uri($"https://docs.mfractor.com/xamarin-forms/overview/?utm_source={ProductInformation.UtmSource}") });
            PackStart(new HSeparator());


            PackStart(new Label("Image Asset Tooling")
            {
                Font = Xwt.Drawing.Font.SystemFont.WithScaledSize(1.5).WithWeight(Xwt.Drawing.FontWeight.Bold)
            });

            PackStart(new Label($"MFractor includes image managment tooling designed for the challenges of cross platform app development.\n\nUse the image manager pad to visually explore all images across your Android and iOS projects, quickly import new image assets with the image importer and use the image deletion to remove all density versions of a particular image plus much more.")
            {
                Wrap = WrapMode.Word,
            });
            PackStart(new LinkLabel("Learn more...") { Uri = new Uri($"https://docs.mfractor.com/image-management/overview/?utm_source={ProductInformation.UtmSource}") });
            PackStart(new HSeparator());


            PackStart(new Label("Plus Much Much More")
            {
                Font = Xwt.Drawing.Font.SystemFont.WithScaledSize(1.5).WithWeight(Xwt.Drawing.FontWeight.Bold)
            });

            PackStart(new Label("This only scratches the surface of what MFractor can do.\nFor a full overview of our feature suite, and for help and support, please see our documentation:")
            {
                Wrap = WrapMode.Word,
            });
            PackStart(new LinkLabel("Go to docs.mfractor.com") { Uri = new Uri($"https://docs.mfractor.com/?utm_source={ProductInformation.UtmSource}") });
            PackStart(new HSeparator());

            closeButton = new Button("Finish");
            closeButton.Clicked += CloseButton_Clicked;


            PackStart(closeButton);
        }

        void CloseButton_Clicked(object sender, EventArgs e)
        {
            OnNext?.Invoke(this, EventArgs.Empty);
        }

        public Widget Widget => this;

        public string Title => "Getting Started";

        bool isComplete;

        readonly Button closeButton;

        public bool IsComplete
        {
            get => isComplete;
            set
            {
                isComplete = value;
                OnCompletedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler OnNext;

        public event EventHandler OnPrevious;

        public event EventHandler OnCompletedChanged;

        public void Activated()
        {
            IsComplete = true;
        }

        public void Deactivated()
        {
        }

        public void SetWindowFrame(WindowFrame windowFrame)
        {
        }
    }
}
