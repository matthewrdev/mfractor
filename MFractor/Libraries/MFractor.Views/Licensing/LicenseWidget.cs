using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using MFractor.IOC;
using MFractor.Licensing;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.Licensing
{
    public class LicenseWidget : VBox
    {
        Label licenseTitle;
        ImageView image;
        Label licenseeName;
        Label licenseeEmail;
        Label licenseExpiry;
        Label licenseTypeInformation;

        Button importLicenseButton;
        Button activateWithSerialKeyButton;
        Button removeActiveLicenseButton;

        Frame serialKeyFrame;

        SerialKeyActivationWidget serialKeyWidget;
        WindowFrame windowFrame;

        [Import]
        IMutableLicensingService LicensingService { get; set; }

        [Import]
        IDialogsService DialogsService { get; set; }

        [Import]
        IDispatcher Dispatcher { get; set; }

        public LicenseWidgetOptions Options { get; }

        public LicenseWidget(LicenseWidgetOptions options)
        {
            Resolver.ComposeParts(this);

            LicensingService.OnLicenseActivated += LicensingService_OnLicenseActivated;

            Options = options;

            Build();

            removeActiveLicenseButton.Visible = options.AllowDeactivation;
            activateWithSerialKeyButton.Visible = options.AllowActivation && LicensingService.IsPaid;
            importLicenseButton.Visible = options.AllowActivation;

            ToggleSerialKeyFrameDisplay(options.AllowActivation && !LicensingService.IsPaid);
        }

        private void ToggleSerialKeyFrameDisplay(bool isAvailable)
        {
            serialKeyFrame.Visible = isAvailable;
            serialKeyFrame.Content = isAvailable ? serialKeyWidget : null;
        }

        void LicensingService_OnLicenseActivated(object sender, LicenseActivatedEventArgs e)
        {
            Dispatcher.InvokeOnMainThread(() =>
             {
                 PopulateLicenseInformation();
                 ToggleSerialKeyFrameDisplay(false);
                 activateWithSerialKeyButton.Visible = true;
             });
        }

        void Build()
        {
            licenseTitle = new Label("Licensing Information:")
            {
                TextAlignment = Alignment.Center,
                Font = Font.SystemFont.WithSize(20).WithWeight(FontWeight.Bold)
            };

            if (Options.ShowBranding)
            {
                image = new ImageView(Xwt.Drawing.Image.FromResource("mfractor_logo.png").WithSize(250, 247))
                {
                    HorizontalPlacement = WidgetPlacement.Center,
                    VerticalPlacement = WidgetPlacement.Center,
                };

                PackStart(image);
            }

            licenseeName = new Label()
            {
                TextAlignment = Alignment.Center,
            };
            licenseeEmail = new Label()
            {
                TextAlignment = Alignment.Center,
            };
            licenseExpiry = new Label()
            {
                TextAlignment = Alignment.Center,
            };
            licenseTypeInformation = new Label()
            {
                TextAlignment = Alignment.Center,
            };

            PopulateLicenseInformation();

            activateWithSerialKeyButton = new Button("Activate Using Serial Key");
            activateWithSerialKeyButton.Clicked += ActivateWithSerialKeyButton_Clicked;

            PrepareRemoveLicenseButton();
            PrepareImportLicenseButton();

            PackStart(licenseTitle);
            PackStart(licenseeName);
            PackStart(licenseeEmail);
            PackStart(licenseExpiry);
            PackStart(licenseTypeInformation);
            PackStart(new HSeparator());

            serialKeyFrame = new Frame();
            serialKeyWidget = new SerialKeyActivationWidget();
            PackStart(serialKeyFrame);

            PackStart(new HSeparator());

            PackStart(importLicenseButton);
            PackStart(activateWithSerialKeyButton);
            PackStart(removeActiveLicenseButton);
        }

        void ActivateWithSerialKeyButton_Clicked(object sender, EventArgs e)
        {
            ToggleSerialKeyFrameDisplay(true);
            removeActiveLicenseButton.Visible = true;
            activateWithSerialKeyButton.Visible = false;
        }

        public void SetWindowFrame(WindowFrame windowFrame)
        {
            this.windowFrame = windowFrame;
        }

        void PrepareImportLicenseButton()
        {
            importLicenseButton = new Button
            {
                Label = "Import License File",
                TooltipText = "Import an MFractor license file. If you have a serial key, please use the serial key activation.",
            };

            importLicenseButton.Clicked += (object sender, EventArgs e) =>
            {
                var licenseFilter = new FileDialogFilter("License Files", "*.lic");

                var chooser = new Xwt.OpenFileDialog("Select your MFractor license file");
                chooser.Filters.Add(licenseFilter);
                chooser.ActiveFilter = licenseFilter;
                chooser.Multiselect = false;
                chooser.CurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                var result = chooser.Run();

                var licenseFilePath = "";
                if (result)
                {
                    licenseFilePath = chooser.FileName;
                }

                chooser.Dispose();

                if (string.IsNullOrEmpty(licenseFilePath))
                {
                    return;
                }

                var licenseMessage = "Your MFractor license has been succesfully loaded!";
                if (!LicensingService.ImportLicense(new FileInfo(licenseFilePath), out var issues))
                {
                    var message = $"MFractor could not activate using the license file'{licenseFilePath}'.\n\nReason:\n";
                    if (issues != null && issues.Any())
                    {
                        message += string.Join("\n", issues);
                    }

                    licenseMessage = message;
                }
                else
                {
                    PopulateLicenseInformation();
                }

                ShowLicenseMessage(licenseMessage);
            };
        }

        void ShowLicenseMessage(string message)
        {
            // Slight delay to allow the chooser to dismiss.
            Task.Run(async () =>
            {
                await Task.Delay(750);
                Application.Invoke(() =>
                {
                    DialogsService.ShowMessage(message, "Ok", windowFrame);
                });
            });
        }

        void PrepareRemoveLicenseButton()
        {
            removeActiveLicenseButton = new Button
            {
                Label = "Deactivate License"
            };
            removeActiveLicenseButton.Clicked += (sender, e) =>
            {
                var choice = DialogsService.AskQuestion("Are you sure you want to deactivate your current license?\nDeactivating a license is permanent and non-recoverable.", "Yes", "No");

                if (choice == "Yes")
                {
                    LicensingService.RemoveActiveLicense(out var issues);
                }
                PopulateLicenseInformation();
            };
        }

        void PopulateLicenseInformation()
        {
            var details = LicensingService.LicensingDetails;

            licenseeName.Text = "Licensed To: " + (details.HasName ? details.Name : "NA");
            licenseeEmail.Text = "License Email: " + (details.HasEmail ? details.Email : "NA");

            licenseTypeInformation.Text = details.Description;

            if (LicensingService.IsPaid && details.Expiry.HasValue)
            {
                licenseExpiry.Text = "License Expiry: " + details.Expiry.Value.ToLongDateString();
            }
            else
            {
                licenseExpiry.Text = "License Expiry: NA";
            }
        }
    }
}
