using System;
namespace MFractor.Views.Controls.Collection
{
    public interface ICollectionItem
    {
        Xwt.Drawing.Image Icon { get; }

        string DisplayText { get; }

        string SecondaryDisplayText { get; }

        string SearchText { get; }

        bool IsChecked { get; set; }
    }
}
