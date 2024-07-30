using System;
using MFractor.Views.Branding;
using MFractor.Work.WorkUnits;
using Xwt;

namespace MFractor.Views.NameValueInput
{
    public class NameValueInputControl : VBox
    {
        Label messageLabel;
        Label nameLabel;
        TextEntry nameEntry;
        Label valueLabel;
        TextEntry valueEntry;
        Button cancelButton;
        Button confirmButton;
        BrandedFooter footer;

        public event EventHandler OnCancelled;
        public event EventHandler OnConfirmed;

        public NameValueInputControl()
        {
            Build();

            BindEvents();
        }

        public void SetWorkUnit(NameValueInputWorkUnit workUnit)
        {
            messageLabel.Text = workUnit.Message;
            nameLabel.Text = workUnit.NameLabel;
            nameEntry.Text = workUnit.Name;
            nameEntry.PlaceholderText = workUnit.NamePlaceholder;
            valueLabel.Text = workUnit.ValueLabel;
            valueEntry.Text = workUnit.Value;
            valueEntry.PlaceholderText = workUnit.ValuePlaceholder;
            confirmButton.Label = workUnit.ConfirmLabel;
            cancelButton.Label = workUnit.CancelLabel;
            footer.HelpUrl = workUnit.HelpUrl;
        }

        public string NameInput
        {
            get => nameEntry.Text;
            set => nameEntry.Text = value;
        }

        public string ValueInput
        {
            get => valueEntry.Text;
            set => valueEntry.Text = value;
        }

        void BindEvents()
        {
            UnbindEvents();

            confirmButton.Clicked += ConfirmButton_Clicked;
            cancelButton.Clicked += CancelButton_Clicked;
        }

        void CancelButton_Clicked(object sender, EventArgs e)
        {
            OnCancelled?.Invoke(this, EventArgs.Empty);
        }

        void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            OnConfirmed?.Invoke(this, EventArgs.Empty);
        }

        void UnbindEvents()
        {
            confirmButton.Clicked -= ConfirmButton_Clicked;
            cancelButton.Clicked -= CancelButton_Clicked;
        }

        void Build()
        {
            messageLabel = new Label();

            PackStart(messageLabel);

            nameLabel = new Label();
            nameEntry = new TextEntry();

            PackStart(nameLabel);
            PackStart(nameEntry);

            valueLabel = new Label();
            valueEntry = new TextEntry();


            PackStart(valueLabel);
            PackStart(valueEntry);

            cancelButton = new Button();
            confirmButton = new Button();

            var buttonsContainer = new HBox();

            buttonsContainer.PackStart(cancelButton);
            buttonsContainer.PackStart(confirmButton);


            PackStart(buttonsContainer);

            footer = new BrandedFooter();

            PackStart(new HSeparator());
            PackStart(footer);
        }
    }
}
