using System;
using MFractor.Utilities;
using Xwt;

namespace MFractor.Views.Controls
{
    public class EmailEntry : VBox
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Xwt.Drawing.Image validationIcon = Xwt.Drawing.Image.FromResource("exclamation.png").WithSize(4.5, 15.5);

        TextEntry emailEntry;
        ImageView emailValidationIcon;

        HBox emailContainer;
        EmailValidationHelper emailValidation;

        public event EventHandler TextChanged;

        public EmailEntry()
        {
            Spacing = 3;
            Build();
        }

        public string Email
        {
            get => emailEntry.Text;
            set => emailEntry.Text = value;
        }

        public string Placeholder
        {
            get => emailEntry.PlaceholderText;
            set => emailEntry.PlaceholderText = value;
        }

        public bool IsValid => emailValidation.IsValidEmail(Email);


        void Build()
        {
            emailContainer = new HBox()
            {
                MarginTop = 2,
                MarginBottom = 2,
            };

            emailEntry = new TextEntry();

            emailValidation = new EmailValidationHelper();

            emailEntry.PlaceholderText = "What is your email address?";
            emailEntry.Changed += (object sender, EventArgs e) =>
            {
                emailValidationIcon.Visible = !emailValidation.IsValidEmail(Email);

                try
                {
                    TextChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }

            };
            emailContainer.PackStart(emailEntry, true, true);

            emailValidationIcon = new ImageView(validationIcon)
            {
                VerticalPlacement = WidgetPlacement.Center,
            };
            emailValidationIcon.TooltipText = "Please enter a valid email address";
            emailContainer.PackEnd(emailValidationIcon);

            PackStart(new Label("Email Address:"));
            PackStart(emailContainer);
        }
    }
}
