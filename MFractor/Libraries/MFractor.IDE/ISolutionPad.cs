using System;
namespace MFractor.Ide
{
    /// <summary>
    /// The solution pad.
    /// </summary>
    public interface ISolutionPad
    {
        /// <summary>
        /// The currently selected item in the users solution pad.
        /// </summary>
        object SelectedItem { get; }
    }
}
