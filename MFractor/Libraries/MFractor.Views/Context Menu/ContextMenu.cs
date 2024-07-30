using System;
using System.Collections.Generic;

namespace MFractor.Views.ContextMenu
{
    public class ContextMenuDescription
    {
        readonly List<ContextMenuElement> elements = new List<ContextMenuElement>();
        public IReadOnlyList<ContextMenuElement> Elements => elements;

        public void AddSeparator()
        {
            elements.Add(new ContextMenuElement(ContextMenuElementKind.Separator));
        }

        public void AddLabel(string label, Xwt.Drawing.Image image = null)
        {
            elements.Add(new ContextMenuElement(label)
            {
                Image = image
            });
        }

        public void AddAction(string label, Action action)
        {
            elements.Add(new ContextMenuElement(label, action));
        }

        public void AddAction(ContextMenuElement element)
        {
            elements.Add(element);
        }

        public void AddSubMenu(string label, ContextMenuDescription contextMenu)
        {
            elements.Add(new ContextMenuElement(label, contextMenu));
        }
    }
}
