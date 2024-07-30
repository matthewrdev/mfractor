using System;
using System.Drawing;
using MFractor.Views.Branding;
using Xwt;

namespace MFractor.Views.Colors
{
    public class ColorEditorDialog : Xwt.Dialog
	{
        VBox root;
        ColorPickerControl colorPicker;
        Button confirmButton;
        Button cancelButton;

        public event EventHandler Confirmed;
        public event EventHandler Cancelled;

        public Color Color
        {
            get => colorPicker.Color;
            set => colorPicker.Color = value;
        }

        public ColorEditorDialog()
            : this(Color.Empty)
        {
        }

        public ColorEditorDialog(Color color)
            : this(color.R, color.G, color.B, color.A)
        {
        }

        public ColorEditorDialog(byte r, byte g, byte b)
            : this(r, g, b, byte.MaxValue)
        {
        }

        public ColorEditorDialog(byte r, byte g, byte b, byte a)
        {
            Title = "Color Picker";

            Build(r, g, b, a);

            BindEvents();
        }

        void BindEvents()
        {
            UnbindEvents();

            confirmButton.Clicked += ConfirmButton_Clicked;
            cancelButton.Clicked += CancelButton_Clicked;
        }

        void CancelButton_Clicked(object sender, EventArgs e)
        {
            Cancelled?.Invoke(this, new EventArgs());

            this.Close();
            this.Dispose();
        }

        void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            Confirmed?.Invoke(this, new EventArgs());

            this.Close();
            this.Dispose();
        }

        void UnbindEvents()
        {
            confirmButton.Clicked -= ConfirmButton_Clicked;
            cancelButton.Clicked -= CancelButton_Clicked;
        }

        void Build(byte r, byte g, byte b, byte a)
        {
            root = new VBox();

            colorPicker = new ColorPickerControl(r, g, b, a);

            root.PackStart(colorPicker);

            var buttonsBox = new HBox();

            confirmButton = new Button("Choose");
            cancelButton = new Button("Cancel");

            buttonsBox.PackStart(cancelButton, true, true);
            buttonsBox.PackStart(confirmButton, true, true);

            root.PackStart(buttonsBox);

            root.PackStart(new HSeparator());
            root.PackStart(new BrandedFooter());

            Content = root;
        }
	}
}

