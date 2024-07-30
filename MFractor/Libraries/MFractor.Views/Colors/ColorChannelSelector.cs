using System;
using Xwt;

namespace MFractor.Views.Colors
{
    public class ColorChannelValueChangedEventArgs : EventArgs
    {
        public byte Value { get; }

        public ColorChannelValueChangedEventArgs(byte value)
        {
            Value = value;
        }
    }

    public class ColorChannelSelector : HBox
    {
        Label label;
        TextEntry entry;
        Slider slider;

        public ColorChannelSelector(string label, byte value, string tooltipText)
        {
            Build();

            Label = label;
            Value = value;
            TooltipText = tooltipText;

            BindEvents();
        }

        public event EventHandler<ColorChannelValueChangedEventArgs> ChannelValueChanged;

        public string Label
        {
            get => label.Text;
            set => label.Text = value.ToString();

        }
        public byte Value
        {
            get => byte.Parse(entry.Text);
            set
            {
                try
               {
                   UnbindEvents();

                   entry.Text = value.ToString();
                    slider.Value = value;

                   NotifyValueChanged(entry.Text);
               }
               finally
               {
                   BindEvents();
               }
            }
        }

        void NotifyValueChanged(string newValue)
        {
            NotifyValueChanged(byte.Parse(newValue));
        }

        void NotifyValueChanged(byte newValue)
        {
            ChannelValueChanged?.Invoke(this, new ColorChannelValueChangedEventArgs(newValue));
        }

        void BindEvents()
        {
            UnbindEvents();

            entry.Changed += Entry_Changed;
            entry.TextInput += Entry_TextInput;
            slider.ValueChanged += Slider_ValueChanged;
        }

        void UnbindEvents()
        {
            entry.Changed -= Entry_Changed;
            entry.TextInput -= Entry_TextInput;
            slider.ValueChanged -= Slider_ValueChanged;
        }

        void Entry_Changed(object sender, EventArgs e)
        {
            try
            {
                UnbindEvents();

                var value = (byte)slider.Value;
                slider.Value = Value;

                NotifyValueChanged(value);
            }
            finally
            {
                BindEvents();
            }
        }

        void Slider_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                UnbindEvents();

                Value = (byte)slider.Value; ;
            }
            finally
            {
                BindEvents();
            }
        }

        void Entry_TextInput(object sender, TextInputEventArgs e)
        {
            try
            {
                UnbindEvents();

                if (!byte.TryParse(e.Text, out var value))
                {
                    e.Handled = true;
                }
            }
            finally
            {
                BindEvents();
            }
        }

        void Build()
        {
            label = new Label();
            label.Font = Xwt.Drawing.Font.SystemFont.WithSize(14).WithWeight(Xwt.Drawing.FontWeight.Bold);

            PackStart(label);

            entry = new TextEntry();
            entry.WidthRequest = 40;

            PackStart(entry);

            slider = new HSlider();
            slider.MinimumValue = 0;
            slider.MaximumValue = 255;
            slider.SnapToTicks = true;
            slider.StepIncrement = 1;

            PackStart(slider, true, true);
        }
    }
}
