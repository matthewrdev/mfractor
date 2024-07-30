using System;
using System.ComponentModel.Composition;
using MFractor.IOC;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Xwt;

namespace MFractor.Views.NameValueInput
{
    public class NameValueInputDialog : Dialog
    {
        NameValueInputControl control;
        NameValueInputDelegate nameValueInputDelegate;

        [Import]
        IWorkEngine WorkEngine { get; set; }

        public NameValueInputDialog()
        {
            Resolver.ComposeParts(this);

            control = new NameValueInputControl();
            Content = control;
        }

        public void SetWorkUnit(NameValueInputWorkUnit workUnit)
        {
            control.SetWorkUnit(workUnit);

            nameValueInputDelegate = workUnit.NameValueInputDelegate;

            BindEvents();
        }

        void BindEvents()
        {
            UnbindEvents();

            control.OnCancelled += Control_OnCancelled;
            control.OnConfirmed += Control_OnConfirmed;
        }

        void UnbindEvents()
        {
            control.OnCancelled -= Control_OnCancelled;
            control.OnConfirmed -= Control_OnConfirmed;
        }

        void Control_OnConfirmed(object sender, EventArgs e)
        {
            var work = nameValueInputDelegate?.Invoke(control.NameInput, control.ValueInput);

            WorkEngine.ApplyAsync(work).ConfigureAwait(false);

            this.Close();
            this.Dispose();
        }

        void Control_OnCancelled(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
    }
}
