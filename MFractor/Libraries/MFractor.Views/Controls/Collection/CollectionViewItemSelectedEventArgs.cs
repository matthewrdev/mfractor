using System;

namespace MFractor.Views.Controls.Collection
{
    public class CollectionViewItemSelectedEventArgs : EventArgs
    {
        public CollectionViewItemSelectedEventArgs(ICollectionItem collectionItem)
        {
            CollectionItem = collectionItem;
        }

        /// <summary>
        /// The selected collection item.
        /// <para/>
        /// May be null if the previous item was unselected.
        /// </summary>
        public ICollectionItem CollectionItem { get; }
    }
}
