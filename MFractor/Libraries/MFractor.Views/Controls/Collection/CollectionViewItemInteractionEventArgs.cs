using System;
using Xwt;

namespace MFractor.Views.Controls.Collection
{
    public class CollectionViewItemInteractionEventArgs : EventArgs
    {
        public CollectionViewItemInteractionEventArgs(ICollectionItem collectionItem,
                                               Widget hostWidget,
                                               ButtonEventArgs buttonEventArgs)
        {
            CollectionItem = collectionItem ?? throw new ArgumentNullException(nameof(collectionItem));
            HostWidget = hostWidget ?? throw new ArgumentNullException(nameof(hostWidget));
            ButtonEventArgs = buttonEventArgs ?? throw new ArgumentNullException(nameof(buttonEventArgs));
        }

        public ICollectionItem CollectionItem { get; }
        public Widget HostWidget { get; }
        public ButtonEventArgs ButtonEventArgs { get; }
    }
}
