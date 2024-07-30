using System;
using MFractor.Configuration;

namespace MFractor.Maui.Configuration
{
    /// <summary>
    /// Groups all configuration settings related to custom controls into a single place.
    /// </summary>
	public interface ICustomControlsConfiguration : IConfigurable
    {
        /// <summary>
        /// What is the folder that new controls should be placed into?
        /// </summary>
        /// <value>The controls folder.</value>
        string ControlsFolder { get; set; }

        /// <summary>
        /// What is the default namespace the new controls be placed into?
        /// </summary>
        /// <value>The controls namespace.</value>
        string ControlsNamespace { get; set; }
    }
}
