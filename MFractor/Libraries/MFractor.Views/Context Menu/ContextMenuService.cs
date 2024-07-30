using System;
using System.ComponentModel.Composition;
using Xwt;

namespace MFractor.Views.ContextMenu
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IContextMenuService))]
    class ContextMenuService : IContextMenuService
    {
        public void Show(Widget parent, ContextMenuDescription contextMenu, int x, int y)
        {
            var menu = CreateNativeContextMenu(contextMenu);

            menu.Popup(parent, x, y);
        }

        Menu CreateNativeContextMenu(ContextMenuDescription contextMenu)
        {
            var menu = new Menu();

            foreach (var item in contextMenu.Elements)
            {
                if (item.ContextMenuElementKind == ContextMenuElementKind.Separator)
                {
                    menu.Items.Add(new SeparatorMenuItem());
                    continue;
                }

                var label = item.Label;

                var menuItem = new MenuItem(label)
                {
                    Tag = item.Action,
                    Image = item.Image,
                };

                if (item.ContextMenuElementKind == ContextMenuElementKind.SubMenu)
                {
                    menuItem.SubMenu = CreateNativeContextMenu(item.SubMenu);
                }
                else
                {
                    menuItem.Clicked += (sender, e) =>
                    {
                        menu.Dispose();
                        ((Action)((MenuItem)sender).Tag)();
                    };
                }

                menu.Items.Add(menuItem);
            }
            return menu;
        }
    }
}
