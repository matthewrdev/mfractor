using System;
using MFractor.Code.Scaffolding;

namespace MFractor.Views.Scaffolding
{
    public class ScaffolderDialog : Xwt.Dialog
    {
        ScaffolderControl scaffolderControl;

        public ScaffolderDialog()
        {
            Title = "Scaffolder";

            Height = 640;
            Width = 960;

            Build();
        }

        public event EventHandler<ScaffoldingResultEventArgs> Confirmed;

        void Build()
        {
            scaffolderControl = new ScaffolderControl();
            scaffolderControl.Confirmed += ScaffolderControl_Confirmed;

            Content = scaffolderControl;
        }

        void ScaffolderControl_Confirmed(object sender, ScaffoldingResultEventArgs e)
        {
            Confirmed?.Invoke(this, e);
        }

        public void SetScaffoldingContext(IScaffoldingContext ScaffoldingContext, string folderPath)
        {
            scaffolderControl.SetScaffoldingContext(ScaffoldingContext, folderPath);
        }
    }
}
