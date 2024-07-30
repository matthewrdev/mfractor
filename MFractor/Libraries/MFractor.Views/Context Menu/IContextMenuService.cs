using System;
using Xwt;

namespace MFractor.Views.ContextMenu
{
    public interface IContextMenuService
    {
        void Show(Widget parent, ContextMenuDescription contextMenu, int x, int y);
    }
}
