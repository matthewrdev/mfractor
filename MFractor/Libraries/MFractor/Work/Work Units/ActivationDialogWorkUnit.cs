using System;

namespace MFractor.Work.WorkUnits
{
    /// <summary>
    /// Launches MFractors activation dialog.
    /// </summary>
    public class  ActivationDialogWorkUnit : WorkUnit
    {
        /// <summary>
        /// An <see cref="Action"/> to invoke after the user succesfully activates MFractor.
        /// </summary>
        public Action OnSuccessfulActivation { get; set; }
    }
}
