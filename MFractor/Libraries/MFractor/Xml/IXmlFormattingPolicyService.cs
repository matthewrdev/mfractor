using System;

namespace MFractor.Xml
{
    public interface IXmlFormattingPolicyService
    {
        /// <summary>
        /// Get the default XML formatting policy. 
        /// </summary>
        IXmlFormattingPolicy GetXmlFormattingPolicy();

        /// <summary>
        /// Get the XML formatting policy for the provided project.
        /// </summary>
        /// <returns>The xml formatting policy.</returns>
        /// <param name="project">The project.</param>
        IXmlFormattingPolicy GetXmlFormattingPolicy(ProjectIdentifier projectIdentifier);
    }
}
