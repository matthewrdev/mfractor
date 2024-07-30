using System;
using Xwt;

namespace MFractor.Views.Settings
{
    public class PreferencesDialog : Xwt.Dialog
    {
        readonly IOptionsWidget optionsWidget;

        public PreferencesDialog(IOptionsWidget optionsWidget)
        {
            this.optionsWidget = optionsWidget;

            this.Width = 1024;
            this.Height = 640;

            this.Title = optionsWidget.Title;

            var rootContainer = new VBox();

            rootContainer.PackStart(optionsWidget.Widget, expand:true);

            rootContainer.PackStart(new HSeparator());

            var buttonsContainer = new HBox();
            var cancelButton = new Button("Cancel");
            cancelButton.Clicked += CancelButton_Clicked;
            var applyButton = new Button("Apply");
            applyButton.Clicked += ApplyButton_Clicked;

            buttonsContainer.PackStart(cancelButton, true);
            buttonsContainer.PackStart(applyButton, true);

            rootContainer.PackStart(buttonsContainer);
            this.Content = rootContainer;
        }

        private void ApplyButton_Clicked(object sender, EventArgs e)
        {
            this.optionsWidget.ApplyChanges();
            this.Hide();
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            this.Hide();
        }
    }

}

