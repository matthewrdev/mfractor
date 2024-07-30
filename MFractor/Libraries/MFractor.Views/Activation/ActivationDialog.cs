using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Analytics;

using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Utilities;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.Activation
{
    public class ActivationDialog : Xwt.Dialog
    {
        [Import]
        IMutableLicensingService LicensingService { get; set; }

        [Import]
        IAnalyticsService AnalyticsService { get; set; }

        [Import]
        IMailingListService MailingListService { get; set; }

        [Import]
        IDialogsService DialogsService { get; set; }
        
        [Import]
        IUrlLauncher UrlLauncher { get; set; }

        Image validationIcon = Image.FromResource("exclamation.png").WithSize(4.5, 15.5);

        HBox emailContainer;
        HBox nameContainer;

        TextEntry nameEntry;
        ImageView nameValidationIcon;

        TextEntry companyEntry;

        TextEntry emailEntry;
        ImageView emailValidationIcon;

        Label welcomeText;

        VBox container;
        ImageView image;

        Button activateButton;
        Button importLicenseButton;

        string email;
        string name;
        string company;

        public event EventHandler<EventArgs> OnDismissed;
        public event EventHandler<EventArgs> OnSuccessfulActivation;

        public ActivationDialog()
        {
            Resolver.ComposeParts(this);

            this.Resizable = false;

            this.Title = "Activate MFractor";
            Icon = Image.FromResource("mfractor_logo.png");

            Build();
        }

        void Build()
        {
            container = new VBox()
            {
                WidthRequest = 320,
            };

            BuildWelcomeHeading();

            BuildNameInput();

            BuildEmailInput();

            BuildCompanyInput();

            activateButton = new Button();
            activateButton.Label = "Activate MFractor";
            activateButton.Clicked += (object sender, EventArgs e) =>
            {
                email = emailEntry.Text;
                name = nameEntry.Text;
                company = companyEntry.Text;

                var emailValidation = new EmailValidationHelper();
                if (emailValidation.IsValidEmail(email)
                    && !string.IsNullOrEmpty(name))
                {
                    var options = new LicensedUserInformation(email, name, company);
                    MailingListService.RegisterForMailingList(options.Email);
                    LicensingService.Activate(options);

                    DialogsService.ShowMessage("Thanks for activating MFractor Lite.\n\nTo learn more about MFractor, please visit our documentation at docs.mfractor.com.", "Ok");
                    UrlLauncher.OpenUrl("https://docs.mfractor.com");
                    AnalyticsService.Track("Activation");

                    this.Close();

                    OnDismissed?.Invoke(this, new EventArgs());
                    OnSuccessfulActivation?.Invoke(this, new EventArgs());
                }
            };

            container.PackStart(activateButton);

            container.PackStart(new HSeparator());

            BuildImportLicenseSection();

            this.Content = container;
        }

        void BuildCompanyInput()
        {
            companyEntry = new TextEntry();
            companyEntry.PlaceholderText = "What company do you work for?";
            companyEntry.Changed += (object sender, EventArgs e) =>
            {
                company = companyEntry.Text;
            };

            container.PackStart(new Label("Company"));
            container.PackStart(companyEntry);
        }

        private void BuildNameInput()
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

            nameValidationIcon = new ImageView(validationIcon);
            nameValidationIcon.TooltipText = "Please enter a name.";
            nameValidationIcon.Visible = string.IsNullOrEmpty(activationName);

            nameContainer.PackEnd(nameValidationIcon);

            container.PackStart(new Label("Full Name:"));
            container.PackStart(nameContainer);
        }

        private void BuildImportLicenseSection()
        {
            importLicenseButton = new Button();
            importLicenseButton.Label = "Import Professional License";
            importLicenseButton.Clicked += async (object sender, EventArgs e) =>
            {
                var licenseFilter = new FileDialogFilter("License Files", "*.lic");

                var chooser = new Xwt.OpenFileDialog("Select your MFractor license file");
                chooser.Filters.Add(licenseFilter);
                chooser.ActiveFilter = licenseFilter;
                chooser.Multiselect = false;
                chooser.CurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                var result = chooser.Run(this);

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

                this.Close();
                DialogsService.ShowMessage("Your MFractor license has been succesfully loaded!", "Ok");
            };

            container.PackStart(importLicenseButton);
        }

        void BuildWelcomeHeading()
        {
            image = new ImageView(Xwt.Drawing.Image.FromResource("mfractor_logo.png").WithSize(250, 247))
            {
                HorizontalPlacement = WidgetPlacement.Center
            };

            container.PackStart(image);

            welcomeText = new Label()
            {
                Markup = "<b>Thanks for using MFractor!</b>\n\nTo use our powerful tools, you'll need to activate this installation.",
                TextAlignment = Alignment.Center,
                Wrap = WrapMode.Word,
                Font = Xwt.Drawing.Font.SystemFont.WithScaledSize(1.4),
            };

            container.PackStart(welcomeText);

            container.PackStart(new HSeparator());
        }

        void BuildEmailInput()
        {
            emailContainer = new HBox();

            emailEntry = new TextEntry();

            var activationEmail = LicensingService.ActivationEmail;
#if DEBUG
            activationEmail = activationEmail ?? "matthew@mfractor.com";
#endif

            var emailValidation = new EmailValidationHelper();

            emailEntry.Text = activationEmail;
            emailEntry.PlaceholderText = "What is your email address?";
            emailEntry.Changed += (object sender, EventArgs e) =>
            {
                email = emailEntry.Text;
                emailValidationIcon.Visible = !emailValidation.IsValidEmail(email);

            };
            emailContainer.PackStart(emailEntry, true, true);

            emailValidationIcon = new ImageView(validationIcon);
            emailValidationIcon.TooltipText = "Please enter a valid email address";
            emailValidationIcon.Visible = !emailValidation.IsValidEmail(activationEmail);
            emailContainer.PackEnd(emailValidationIcon);

            container.PackStart(new Label("Email Address:"));
            container.PackStart(emailContainer);
        }

        protected override void OnClosed()
        {
            // Store options.
            if (this.OnDismissed != null)
            {
                OnDismissed(this, new EventArgs());
            }

            base.OnClosed();
        }
    }
}

