using System;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace MFractor
{
    /// <summary>
    /// The location information for where a user interaction (such as a tooltip, code completion or code action) is taking place.
    /// </summary>
    [DebuggerDisplay("Position: {Position}, Selection: {Selection}")]
    public class InteractionLocation
    {
        /// <summary>
        /// The position of the interaction. This is typically the users cursor.
        /// </summary>
        /// <value>The position of the interaction.</value>
        public int Position { get; }

        /// <summary>
        /// The current selection span.
        /// </summary>
        /// <value>The selection.</value>
        public TextSpan? Selection { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.InteractionLocation"/> class.
        /// </summary>
        /// <param name="position">The position of the interaction.</param>
        /// <param name="selection">If applicable, the interactions selection.</param>
        public InteractionLocation(int position, TextSpan? selection = null)
        {
            Position = position;
            Selection = selection;
        }
    }
}
