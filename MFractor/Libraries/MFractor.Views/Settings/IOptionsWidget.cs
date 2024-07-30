using System;

namespace MFractor.Views.Settings
{
    public interface IOptionsWidget
    {
        void ApplyChanges();

        Xwt.Widget Widget { get; }

        string Title { get; }
    }
}
