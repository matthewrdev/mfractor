using System;
using System.Collections.Generic;
using MFractor.Text;

namespace MFractor.Work
{
    public interface IWorkExecutionResult
    {
        IReadOnlyList<string> ChangedFiles { get; }

        IReadOnlyList<IWorkUnit> AppliedWorkUnits { get; }

        IReadOnlyList<IWorkUnit> PostProcessedWorkUnits { get; }

        IReadOnlyList<ITextReplacement> TextReplacements { get; }

        IReadOnlyList<ProjectIdentifier> ProjectsToSave { get; }

        IReadOnlyList<FileCreation> FilesToCreate { get; }

        void AddChangedFile(string file);

        void RemoveChangedFile(string file);

        void AddChangedFiles(IEnumerable<string> files);

        void AddAppliedWorkUnit(IWorkUnit workUnit);

        void RemoveAppliedWorkUnit(IWorkUnit workUnit);

        void AddAppliedWorkUnits(params IWorkUnit[] workUnits);

        void AddAppliedRespones(IReadOnlyList<IWorkUnit> workUnits);

        void AddTextReplacement(ITextReplacement replacement);

        void AddTextReplacements(IEnumerable<ITextReplacement> replacements);

        void AddPostProcessedWorkUnit(IWorkUnit workUnit);

        void AddPostProcessedWorkUnits(IReadOnlyList<IWorkUnit> workUnits);

        void AddProjectToSave(ProjectIdentifier projectIdentifier);

        void AddProjectsToSave(IEnumerable<ProjectIdentifier> projectIdentifiers);

        void AddFileCreation(FileCreation fileCreation);

        void AddFileCreations(IEnumerable<FileCreation> fileCreations);

        void MergeWith(IWorkExecutionResult other);
    }
}
