using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MFractor.VS.Windows.Views
{
    class ClickableLabel : TextBlock
    {
        public ClickableLabel(string label, Func<Task> OnClickedAction)
        {
            this.OnClickedAction = OnClickedAction;

            var hyperlink = new Hyperlink();
            hyperlink.Inlines.Add(label);
            hyperlink.Click += (sender, args) =>
            {
                args.Handled = true;
                OnClickedAction?.Invoke()?.ConfigureAwait(false);
            };
            
            Inlines.Add(hyperlink);
        }

        public Func<Task> OnClickedAction { get; }
    }
}
