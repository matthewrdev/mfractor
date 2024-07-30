using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.CodeSnippets;
using MFractor.Configuration;
using MFractor.Code.Documents;
using MFractor.Localisation.WorkUnits;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.LocaliserRefactorings
{
    [InheritedExport]
    public interface ILocaliserRefactoring : IConfigurable
    {
        ICodeSnippet LocalisationCodeSnippet { get; set; }

        string DefaultLocalisationFilePath { get; set; }

        bool IsAvailable(Project project, string filePath);

        string CreateLocalisationExpression(ICodeSnippet localisationSnippet, LocalisationOperation operation, Project project, IParsedDocument document);

        IReadOnlyList<IWorkUnit> CreateLocalisationValues(LocalisationOperation operation, Project project, IParsedDocument document);

        IReadOnlyList<IWorkUnit> CreateLocalisationExpressionImportStatement(ICodeSnippet localisationSnippet, LocalisationOperation operation, Project project, IParsedDocument document);
    }
}