using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Licensing.Activation;
using MFractor.Views.Controls;
using Xwt;

namespace MFractor.Views.Licensing
{
    class SerialKeyConfirmedEventArgs : EventArgs
    {
        public SerialKeyConfirmedEventArgs(string serialKey)
        {
            SerialKey = serialKey;
        }

        public string SerialKey { get; }
    }

    class SerialKeyActivationWidget : VBox
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        EmailEntry emailEntry;
        SerialKeyEntry serialKeyEntry;

        Spinner busyIndicator;

        Button activateButton;

        [Import]
        ILicenseActivationService LicenseActivationService { get; set; }

        [Import]
        IMutableLicensingService LicensingService { get; set; }

        [Import]
        IDialogsService DialogsService { get; set; }

        public bool CanActivate => emailEntry.IsValid && serialKeyEntry.IsValid;

        public SerialKeyActivationWidget()
        {
            Resolver.ComposeParts(this);

            Build();

            BindEvents();

            Validate();
        }

        void BindEvents()
        {
            UnbindEvents();

            serialKeyEntry.TextChanged += OnTextChanged;
            emailEntry.TextChanged += OnTextChanged;
            activateButton.Clicked += ActivateButton_Clicked;
        }

        void ActivateButton_Clicked(object sender, EventArgs e)
        {
            Activate().ConfigureAwait(false);
        }

        void OnTextChanged(object sender, EventArgs e)
        {
            Validate();
        }

        void Validate()
        {
            this.activateButton.Sensitive = serialKeyEntry.IsValid && emailEntry.IsValid;
        }

        async Task Activate()
        {
            ILicenseRequestResult result = null;
            try
            {
                busyIndicator.Visible = true;
                serialKeyEntry.Sensitive = false;
                emailEntry.Sensitive = false;
                activateButton.Sensitive = false;

                result = await LicenseActivationService.ActivateSerialKey(emailEntry.Email, serialKeyEntry.SerialKey);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
                result = new LicenseRequestResult(false, string.Empty, "License activation failed.", "An unknown error occured while activating the license. Please contact matthew@mfractor.com for support");
            }
            finally
            {
                busyIndicator.Visible = false;
                busyIndicator.Visible = false;
                serialKeyEntry.Sensitive = true;
                emailEntry.Sensitive = true;
                this.activateButton.Sensitive = serialKeyEntry.IsValid && emailEntry.IsValid;
            }

            if (result.Success)
            {
                if (!LicensingService.ImportLicense(result.LicenseContent, out var issues))
                {
                    var message = $"MFractor could not activate with the serial key provided.\n\nReason:\n";
                    if (issues != null && issues.Any())
                    {
                        message += string.Join("\n", issues);
                    }

                    DialogsService.ShowMessage(message, "Ok");

                    return;
                }
                else
                {
                    DialogsService.ShowMessage("Your MFractor license has been succesfully loaded!", "Ok");
                }
            }
            else
            {
                DialogsService.ShowError(result.StatusMessage + "\n" + result.StatusDetail);
            }
        }

        void UnbindEvents()
        {
            serialKeyEntry.TextChanged -= OnTextChanged;
            emailEntry.TextChanged -= OnTextChanged;
            activateButton.Clicked -= ActivateButton_Clicked;
        }

        void Build()
        {
            emailEntry = new EmailEntry()
            {
                Email = LicensingService.ActivationEmail,
            };

            PackStart(emailEntry);

            serialKeyEntry = new SerialKeyEntry();
            PackStart(serialKeyEntry);

            busyIndicator = new Spinner()
            {
                HorizontalPlacement = WidgetPlacement.Center,
                Visible = false,
                Animate = true,
            };
            activateButton = new Button("Activate");

            PackStart(busyIndicator);
            PackStart(activateButton);
        }

        public void Show() // HACK: Workaround for visual display bug on Windows.
        {
            serialKeyEntry.Visible = true;
        }

        public void Hide() // HACK: Workaround for visual display bug on Windows.
        {
            serialKeyEntry.Visible = false;
        }
    }
}
