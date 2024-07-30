using System;

namespace MFractor.Configuration.Attributes
{
    /// <summary>
    /// Marks a configurable implementation as ignoring configuration.
    /// <para/>
    /// Use this to:
    /// <para/>
    ///  * Suppress the configuration id from being exported in the final documentation (for instance, code analysers).
    /// <para/>
    ///  * Inform end-users that a valid configurable will not consume configurations applied to it via the config engine as a *.mfc.xml warning.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SuppressConfigurationAttribute : Attribute 
    {
    }
}
