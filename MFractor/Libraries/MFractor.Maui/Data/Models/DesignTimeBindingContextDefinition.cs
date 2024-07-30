using System;
using MFractor.Data.Models;
using MFractor.Workspace.Data.Models;

namespace MFractor.Maui.Data.Models
{
    /// <summary>
    /// When a user applies the DesignTimeBindingContextAttribute onto a code behind class, the <see cref="DesignTimeBindingContextDefinition"/> records the mapping for other subsystems to use.
    /// </summary>
    public class DesignTimeBindingContextDefinition : ProjectFileOwnedEntity
    {
        /// <summary>
        /// The fully qualified meta-data name of the binding context.
        /// </summary>
        /// <value>The binding context symbol.</value>
        public string BindingContextSymbol { get; set; }

        /// <summary>
        /// The fully qualified meta-data name of the code behind symbol.
        /// </summary>
        /// <value>The code behind symbol.</value>
        public string CodeBehindSymbol { get; set; }
    }
}
