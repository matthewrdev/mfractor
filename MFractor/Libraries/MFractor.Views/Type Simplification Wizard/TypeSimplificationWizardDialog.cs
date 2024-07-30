using System;
using MFractor.Workspace;

namespace MFractor.Views.TypeSimplificationWizard
{
    public class TypeSimplificationWizardDialog : Xwt.Dialog
    {
        readonly TypeSimpificationWizardControl control;

        public TypeSimplificationWizardDialog()
        {
            Title = "Simplify Types";

            Width = 960;
            Height = 720;

            control = new TypeSimpificationWizardControl();
            control.TypeSimplicationConfirmed += Control_TypeSimplicationConfirmed;

            Content = control;
        }

        void Control_TypeSimplicationConfirmed(object sender, TypeSimplificationEventArgs e)
        {
            TypeSimplicationConfirmed?.Invoke(this, e);
        }

        public event EventHandler<TypeSimplificationEventArgs> TypeSimplicationConfirmed;

        public void SetProjectFile(IProjectFile projectFile)
        {
            control.SetProjectFile(projectFile);
        }
    }
}
