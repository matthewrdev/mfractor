using System;
using Xwt;

namespace MFractor.Views.Licensing
{
    class SerialKeyEntry : VBox
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Xwt.Drawing.Image validationIcon = Xwt.Drawing.Image.FromResource("exclamation.png").WithSize(4.5, 15.5);

        public event EventHandler TextChanged;

        TextEntry serialKeyEntry;
        ImageView serialKeyValidationIcon;

        HBox container;

        public SerialKeyEntry()
        {
            Build();
        }

        public string SerialKey
        {
            get => serialKeyEntry.Text;
            set => serialKeyEntry.Text = value;
        }

        public string Placeholder
        {
            get => serialKeyEntry.PlaceholderText;
            set => serialKeyEntry.PlaceholderText = value;
        }

        public bool IsValid => Guid.TryParse(serialKeyEntry.Text ?? string.Empty, out _);

        void Build()
        {
            container = new HBox();

            serialKeyEntry = new TextEntry();

            serialKeyEntry.PlaceholderText = "Paste your serial key here...";
            serialKeyEntry.Changed += (object sender, EventArgs e) =>
            {
                serialKeyValidationIcon.Visible = !IsValid;
                try
                {
                    TextChanged?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            };
            container.PackStart(serialKeyEntry, true, true);

            serialKeyValidationIcon = new ImageView(validationIcon);
            serialKeyValidationIcon.TooltipText = "Please enter your 36 digit serial key.";
            container.PackEnd(serialKeyValidationIcon);

            PackStart(new Label("Serial Key:"));
            PackStart(container);
        }
    }
}
