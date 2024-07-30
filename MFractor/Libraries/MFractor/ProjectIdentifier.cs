using System;
using System.Diagnostics;

namespace MFractor
{
    /// <summary>
    /// An identifier for a project in the IDEs project/solution model.
    /// </summary>
    [DebuggerDisplay("{Name} - {Guid}")]
    public class ProjectIdentifier
    {
        /// <summary>
        /// The unique project GUID.
        /// </summary>
        public string Guid { get; }

        /// <summary>
        /// The name of the project.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.ProjectIdentifier"/> class.
        /// </summary>
        public ProjectIdentifier(string guid, string name = "")
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentException("message", nameof(guid));
            }

            Guid = guid;
            Name = name;
        }

        internal string GetIdentifier()
        {
            throw new NotImplementedException();
        }
    }
}
