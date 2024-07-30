using System;
using System.Threading.Tasks;
using MFractor.Views.Branding;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.TextInput
{
    public class TextInputDialog : Xwt.Dialog
	{
        VBox root;

        Label messageLabel;

        TextEntry entry;

        HBox buttonsContainer;
        Button buttonOk;
        Button buttonCancel;

        public TextInputDialog (string title, string message, string value, string confirmMessage, string cancelMessage)
		{
            Width = 300;
			Build ();

			Init (title, message, value, confirmMessage, cancelMessage);
		}

        void Build()
        {
            root = new VBox();

            messageLabel = new Label();
            root.PackStart(messageLabel);

            entry = new TextEntry()
            {
                ExpandHorizontal = true,
            };

            root.PackStart(entry);
            buttonsContainer = new HBox()
            {
                ExpandHorizontal = true
            };

            buttonOk = new Button();
            buttonCancel = new Button();

            buttonsContainer.PackStart(buttonCancel, true, true);
            buttonsContainer.PackStart(buttonOk, true, true);

            root.PackStart(buttonsContainer);

            root.PackStart(new HSeparator());
            root.PackStart(new BrandedFooter());

            Content = root;
        }

        void Init (string title, string message, string value, string confirmMessage, string cancelMessage)
		{
			this.Title = title;
            messageLabel.Text = message;
            Icon = Image.FromResource("mfractor_logo.png");

            entry.Text = value ?? "";

			entry.Changed += OnEntryChanged;
			entry.Activated += OnEntryActivated;

			buttonOk.Label = confirmMessage;
			buttonOk.Clicked += OnOKClicked;
			buttonCancel.Label = cancelMessage;
            buttonCancel.Clicked += OnCancelClicked; 
			entry.Changed += delegate { buttonOk.Sensitive = !string.IsNullOrEmpty(entry.Text); };
		}

        void OnCancelClicked(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        public event EventHandler<TextEntryCompleteEventArgs> OnEntryConfirmed;

		void OnEntryChanged (object sender, EventArgs e)
		{
			// Don't allow the user to click OK unless there is a new name
			buttonOk.Sensitive = entry.Text.Length > 0;
		}

		void OnEntryActivated (object sender, EventArgs e)
		{
            if (buttonOk.Sensitive)
            {
                OnEntryConfirmed?.Invoke(this, new TextEntryCompleteEventArgs(entry.Text));
            }
		}

		void OnOKClicked (object sender, EventArgs e)
		{
			OnEntryConfirmed?.Invoke(this, new TextEntryCompleteEventArgs(entry.Text));
		}
	}
}

