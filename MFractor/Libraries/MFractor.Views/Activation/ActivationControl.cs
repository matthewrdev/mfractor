using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Analytics;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Licensing.Activation;
using MFractor.Utilities;
using MFractor.Views.Controls;
using Xwt;
using System.Threading.Tasks;

namespace MFractor.Views.Activation
{
    public class ActivationControl : VBox
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [Import]
        IMutableLicensingService LicensingService { get; set; }

        [Import]
        IAnalyticsService AnalyticsService { get; set; }

        [Import]
        IMailingListService MailingListService { get; set; }

        [Import]
        IDialogsService DialogsService { get; set; }

        [Import]
        ILicenseActivationService LicenseActivationService { get; set; }

        readonly Xwt.Drawing.Image validationIcon = Xwt.Drawing.Image.FromResource("exclamation.png").WithSize(4.5, 15.5);

        HBox nameContainer;

        TextEntry nameEntry;
        ImageView nameValidationIcon;

        Label welcomeText;

        ImageView image;

        TextEntry companyEntry;

        EmailEntry emailEntry;

        Button activateButton;
        Button importLicenseButton;

        Spinner busyIndicator;
        Label requestingTrialLabel;

        string email;
        string name;
        string company;

        readonly bool allowImportLicense;
        readonly bool includeBranding;
        readonly string buttonText;

        public event EventHandler<EventArgs> OnSuccessfulActivation;

        public ActivationControl(bool allowImportLicense, bool includeBranding, string buttonText = "Activate MFractor")
        {
            Resolver.ComposeParts(this);

            this.allowImportLicense = allowImportLicense;
            this.includeBranding = includeBranding;
            this.buttonText = buttonText;

            Build();
        }

        void Build()
        {
            if (includeBranding)
            {
                BuildWelcomeHeading();
            }

            BuildNameInput();

            BuildEmailInput();

            BuildCompanyInput();

            busyIndicator = new Spinner();
            busyIndicator.Visible = false;
            PackStart(busyIndicator);

            requestingTrialLabel = new Label("Requesting a 30 day trial...");
            requestingTrialLabel.TextAlignment = Alignment.Center;
            requestingTrialLabel.HorizontalPlacement = WidgetPlacement.Center;
            requestingTrialLabel.Visible = false;
            PackStart(requestingTrialLabel);

            activateButton = new Button();
            activateButton.Label = buttonText;
            activateButton.Clicked += async (object sender, EventArgs e) =>
            {
                email = emailEntry.Email;
                name = nameEntry.Text;
                company = companyEntry.Text;

                var emailValidation = new EmailValidationHelper();
                if (emailEntry.IsValid
                    && !string.IsNullOrEmpty(name))
                {
                    var options = new LicensedUserInformation(email, name, company);

                    MailingListService.RegisterForMailingList(options.Email);

                    var result = await ActivateTrial();

                    var message = "Thanks for activating MFractor.";

                    if (LicensingService.IsTrial)
                    {
                        message += "\nYou have now started a 30-day trial of MFractor Professional.";
                    }
                    else
                    {

                    }

                    LicensingService.Activate(options);
                    DialogsService.ShowMessage(message, "Ok");

                    AnalyticsService.Track("Activation");

                    OnSuccessfulActivation?.Invoke(this, new EventArgs());
                }
            };

            PackStart(activateButton);

            if (allowImportLicense)
            {
                PackStart(new HSeparator());

                BuildImportLicenseSection();
            }
        }

        async Task<bool> ActivateTrial()
        {
            ILicenseRequestResult result = null;
            try
            {
                busyIndicator.Visible = true;
                requestingTrialLabel.Visible = true;
                this.Sensitive = false;

                result = await LicenseActivationService.ActivateTrialLicense(emailEntry.Email, nameEntry.Text);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
                result = new LicenseRequestResult(false, string.Empty, "Trial license request failed.", "An unknown error occured while requesting your 30-day trial license. Please contact matthew@mfractor.com for support.");
            }
            finally
            {
                requestingTrialLabel.Visible = false;
                busyIndicator.Visible = false;
                this.Sensitive = true;
            }

            if (!result.Success)
            {
                DialogsService.ShowMessage(result.StatusMessage + "\n" + result.StatusDetail, "Ok");
                return false;
            }

            if (!LicensingService.ImportLicense(result.LicenseContent, out var issues))
            {
                var message = $"MFractor could not activate your 30-day trial.\n\nReason:\n";
                if (issues != null && issues.Any())
                {
                    message += string.Join("\n", issues);
                }

                DialogsService.ShowMessage(message, "Ok");

                return false;
            }

            return true; ;
        }

        void BuildWelcomeHeading()
        {
            image = new ImageView(Xwt.Drawing.Image.FromResource("mfractor_logo.png").WithSize(250, 247))
            {
                HorizontalPlacement = WidgetPlacement.Center
            };

            PackStart(image);

            welcomeText = new Label()
            {
                Markup = "<b>Thanks for using MFractor!</b>\n\nPlease activate this installation.",
                TextAlignment = Alignment.Center,
                Wrap = WrapMode.Word,
                Font = Xwt.Drawing.Font.SystemFont.WithScaledSize(1.4),
            };

            PackStart(welcomeText);

            PackStart(new HSeparator());
        }

        void BuildCompanyInput()
        {
            companyEntry = new TextEntry
            {
                PlaceholderText = "What company do you work for?"
            };
            companyEntry.Changed += (object sender, EventArgs e) =>
            {
                company = companyEntry.Text;
            };

            PackStart(new Label("Company"));
            PackStart(companyEntry);
        }

        void BuildNameInput()
        {
            var activationName = LicensingService.ActivationName;
#if DEBUG
            activationName = activationName ?? "Matthew Robbins";
#endif

            nameContainer = new HBox();

            nameEntry = new TextEntry();
            nameEntry.PlaceholderText = "What is your full name?";
            nameEntry.Text = activationName;
            nameEntry.Changed += (object sender, EventArgs e) =>
            {
                name = nameEntry.Text;
                nameValidationIcon.Visible = string.IsNullOrEmpty(name);
            };
            nameContainer.PackStart(nameEntry, true, true);

            nameValidationIcon = new ImageView(validationIcon)
            {
                TooltipText = "Please enter a name.",
                Visible = string.IsNullOrEmpty(activationName)
            };

            nameContainer.PackEnd(nameValidationIcon);

            PackStart(new Label("Full Name:"));
            PackStart(nameContainer);
        }

        void BuildImportLicenseSection()
        {
            importLicenseButton = new Button
            {
                Label = "Import Professional License"
            };
            importLicenseButton.Clicked += async (object sender, EventArgs e) =>
            {
                var licenseFilter = new FileDialogFilter("License Files", "*.lic");

                var chooser = new Xwt.OpenFileDialog("Select your MFractor license file");
                chooser.Filters.Add(licenseFilter);
                chooser.ActiveFilter = licenseFilter;
                chooser.Multiselect = false;
                chooser.CurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                var result = chooser.Run(this.ParentWindow);

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

                if (!LicensingService.ImportLicense(new System.IO.FileInfo(licenseFilePath), out var issues))
                {
                    var message = $"MFractor could not activate using the license file'{licenseFilePath}'.\n\nReason:\n";
                    if (issues != null && issues.Any())
                    {
                        message += string.Join("\n", issues);
                    }

                    DialogsService.ShowMessage(message, "Ok");

                    return;
                }

                this.OnSuccessfulActivation?.Invoke(this, EventArgs.Empty);
                DialogsService.ShowMessage("Your MFractor license has been succesfully loaded!", "Ok");
            };

            PackStart(importLicenseButton);
        }

        void BuildEmailInput()
        {
            var activationEmail = LicensingService.ActivationEmail;
#if DEBUG
            activationEmail = activationEmail ?? "matthew@mfractor.com";
#endif

            emailEntry = new EmailEntry();
            emailEntry.Email = activationEmail;

            PackStart(emailEntry);
        }
    }
}
