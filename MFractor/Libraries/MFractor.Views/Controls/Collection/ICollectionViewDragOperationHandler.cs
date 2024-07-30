using Xwt;

namespace MFractor.Views.Controls.Collection
{
    public interface ICollectionViewDragOperationHandler
    {
        void OnDrag(ICollectionItem item, DragOperation dragOperation);
    }
}
