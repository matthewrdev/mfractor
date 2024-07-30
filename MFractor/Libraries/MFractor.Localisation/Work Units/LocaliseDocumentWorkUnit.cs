using System;
using System.Collections.Generic;
using MFractor.Code.Documents;
using MFractor.Work;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Localisation.WorkUnits
{
    /// <summary>
    /// Opens the localisation wizard.
    /// </summary>
    public class LocaliseDocumentWorkUnit : WorkUnit
    {
        public LocaliseDocumentWorkUnit(IParsedDocument document,
                                        Project project,
                                        IReadOnlyList<Project> projects,
                                        object semanticModel,
                                        TextSpan? targetSpan = null,
                                        string defaultFile = "")
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Project = project ?? throw new ArgumentNullException(nameof(project));
            Projects = projects;
            SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
            TargetSpan = targetSpan;
            DefaultFile = defaultFile;
        }

        public Project Project { get; }
        public IReadOnlyList<Project> Projects { get; }
        public object SemanticModel { get; }
        public IParsedDocument Document { get; }

        public string DefaultFile { get; }

        public TextSpan? TargetSpan { get; }
    }
}
