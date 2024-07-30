using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.PlatformUI;

namespace MFractor.VS.Windows.UI.Dialogs
{
    public class BaseDialogWindow : DialogWindow
    {
        public BaseDialogWindow()
        {
            HasMaximizeButton = true;
            HasMinimizeButton = true;
        }
    }
}
