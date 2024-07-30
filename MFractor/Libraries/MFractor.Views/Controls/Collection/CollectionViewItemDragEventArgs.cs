using System;
using Xwt;

namespace MFractor.Views.Controls.Collection
{
    public class CollectionViewItemDragEventArgs : EventArgs
    {
        public CollectionViewItemDragEventArgs(ICollectionItem collectionItem, DragOperation dragOperation)
        {
            CollectionItem = collectionItem ?? throw new ArgumentNullException(nameof(collectionItem));
            DragOperation = dragOperation ?? throw new ArgumentNullException(nameof(dragOperation));
        }

        public ICollectionItem CollectionItem { get; }
        public DragOperation DragOperation { get; }
    }
}
