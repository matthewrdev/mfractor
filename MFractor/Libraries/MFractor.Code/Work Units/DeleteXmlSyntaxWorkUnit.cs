using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Code.WorkUnits
{
    /// <summary>
    /// A workUnit to delete a series of <see cref="XmlSyntax"/> elements from a given file.
    /// <para/>
    /// Assumes that <see cref="FilePath"/> is an xml file.
    /// </summary>
    public class DeleteXmlSyntaxWorkUnit : WorkUnit
    {
        /// <summary>
        /// The path of the file that contains the <see cref="Syntaxes"/> to delete.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// A series of xml syntax elements to delete.
        /// </summary>
        public IReadOnlyList<XmlSyntax> Syntaxes { get; set; }

        /// <summary>
        /// A single xml syntax to delete.
        /// </summary>
        public XmlSyntax Syntax
        {
            get => Syntaxes?.FirstOrDefault();
            set => Syntaxes = new List<XmlSyntax>() { value };
        }

        /// <summary>
        /// When deleting each syntax element, should MFractor cleanup any unnecessary remaining whitespace?
        /// </summary>
        public bool RemoveUnnecessaryWhitespace { get; set; } = true;
    }
}
