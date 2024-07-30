using System;
using System.Collections.Generic;
using Xwt;

namespace MFractor.Views.Settings
{
    /// <summary>
    /// The SettingsDialog should only be used as a in-between step in IDEs where integrate settings natively is harder than just showing this dialog.
    /// </summary>
    public class SettingsDialog : Dialog
    {
        VBox root;

        List<IOptionsWidget> options;
        Notebook noteBook;
        Button saveButton;

        public SettingsDialog()
        {
            Title = "MFractor Preferences";

            Build();
        }

        void Build()
        {
            root = new VBox();

            options = new List<IOptionsWidget>()
            {
                new SettingsWidget(),
                new FormattingOptionsWidget(),
            };

            noteBook = new Notebook();

            foreach (var option in options)
            {
                noteBook.Add(option.Widget, option.Title);
            }

            root.PackStart(noteBook, true, true);

            root.PackStart(new HSeparator());

            saveButton = new Button()
            {
                Label = "Save",
                HorizontalPlacement = WidgetPlacement.End,
            };
            saveButton.Clicked += SaveButton_Clicked;

            root.PackStart(saveButton);

            Content = root;
        }

        void SaveButton_Clicked(object sender, EventArgs e)
        {
            foreach (var option in options)
            {
                option.ApplyChanges();
            }

            this.Hide();
            this.Dispose();
        }
    }
}
