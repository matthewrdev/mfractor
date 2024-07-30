using System;
using System.Diagnostics;

namespace MFractor.Views.ContextMenu
{
    [DebuggerDisplay("{ContextMenuElementKind} - {Label}")]
    public class ContextMenuElement
    {
        public ContextMenuElement(ContextMenuElementKind contextMenuElementKind)
        {
            ContextMenuElementKind = contextMenuElementKind;
        }

        public ContextMenuElement(string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("message", nameof(label));
            }

            ContextMenuElementKind = ContextMenuElementKind.Label;
            Label = label;
        }

        public ContextMenuElement(string label, Action action)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("message", nameof(label));
            }

            ContextMenuElementKind = ContextMenuElementKind.Action;
            Label = label;
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public ContextMenuElement(string label, ContextMenuDescription subMenu)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("message", nameof(label));
            }

            ContextMenuElementKind = ContextMenuElementKind.SubMenu;
            Label = label;
            SubMenu = subMenu ?? throw new ArgumentNullException(nameof(subMenu));
        }

        public ContextMenuElementKind ContextMenuElementKind { get; }

        public string Label { get; }

        public Xwt.Drawing.Image Image { get; set; }

        public ContextMenuDescription SubMenu { get; }

        public Action Action { get; }
    }
}
