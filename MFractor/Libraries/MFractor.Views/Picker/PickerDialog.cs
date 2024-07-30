using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Views.Branding;
using MFractor.Work.WorkUnits;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.Picker
{
    public class PickerDialog : Dialog
    {
        VBox container;

        Label messagesLabel;

        ComboBox selections;

        Button confirmSelectionButton;

        IReadOnlyDictionary<string, object> Choices { get; }
        public string Message { get; }
        public string Confirm { get; }
        public string Cancel { get; }
        public string HelpUrl { get; }
        public ProjectSelectorMode Mode { get; }

        public event EventHandler<PickerSelectionEventArgs> OnItemSelected;

        public PickerDialog(IReadOnlyDictionary<string, object> choices,
                            string title,
                            string message,
                            string confirm,
                            string cancel,
                            string helpUrl = "")
        {
            Title = title;
            Icon = Image.FromResource("mfractor_logo.png");

            Choices = choices;
            Message = message;
            Confirm = confirm;
            Cancel = cancel;
            HelpUrl = helpUrl;
            container = new VBox();

            Build();

            container.PackStart(new HSeparator());
            container.PackStart(new BrandedFooter(HelpUrl));

            this.Content = container;

            selections.SelectedItem = choices.Values.First();
        }

        void Build()
        {
            container = new VBox();

            container.PackStart(new HSeparator());

            if (!string.IsNullOrEmpty(Message))
            {
                messagesLabel = new Label();
                messagesLabel.Text = Message;
                container.PackStart(messagesLabel);
                container.PackStart(new HSeparator());
            }

            selections = new ComboBox();
            foreach (var choice in Choices)
            {
                selections.Items.Add(choice.Value, choice.Key);
            }
            container.PackStart(selections);

            confirmSelectionButton = new Button()
            {
                Label = Confirm,
                HeightRequest = 30,
                Font = Font.SystemFont.WithSize(20).WithWeight(FontWeight.Bold),
            };
            confirmSelectionButton.Clicked += (sender, e) =>
            {
                ConfirmSelection();
            };

            container.PackStart(confirmSelectionButton);
        }

        bool ConfirmSelection()
        {
            var choice = Choices.FirstOrDefault(c => c.Value == selections.SelectedItem);

            var args = new PickerSelectionEventArgs(choice.Key, choice.Value);

            this.OnItemSelected?.Invoke(this, args);

            this.Close();
            return true;
        }
    }
}
