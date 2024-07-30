using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using MFractor.IOC;
using MFractor.VS.Windows.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.VS.Windows.Adornments
{
    public class ColorAdornment : Border
    {
        public Color Color { get; private set; }
        public ColorEditedDelegate ColorEditedDelegate { get; private set; }

        public ColorAdornment() : base()
        {
            Build();
        }

        public ColorAdornment(Color color, ColorEditedDelegate colorEditedDelegate) : base()
        {
            Build();
            Update(color, colorEditedDelegate);
        }

        void Build()
        {
            BorderThickness = new System.Windows.Thickness(1);
            BorderBrush = System.Windows.Media.Colors.Black.ToBrush();
            Background = System.Windows.Media.Colors.Transparent.ToBrush();
            Width = 20;
            Height = 10;
        }

        public void Update(Color color, ColorEditedDelegate colorEditedDelegate)
        {
            Background = color.ToMediaColor().ToBrush();
            Color = color;
            ColorEditedDelegate = colorEditedDelegate;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.ChangedButton == MouseButton.Left && ColorEditedDelegate != null)
            {
                Resolver.Resolve<IWorkEngine>().ApplyAsync(new ColorEditorWorkUnit(this.Color, ColorEditedDelegate)).ConfigureAwait(false);
            }
        }
    }
}
