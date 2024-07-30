using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Text;

namespace MFractor.Work
{
    /// <summary>
    /// The result of a workUnit executon.
    /// </summary>
    public class WorkExecutionResult : IWorkExecutionResult
    {
        readonly List<string> changedFiles = new List<string>();

        readonly List<IWorkUnit> appliedWorkUnits = new List<IWorkUnit>();

        readonly List<IWorkUnit> postProcessedWorkUnits = new List<IWorkUnit>();

        readonly List<ITextReplacement> textReplacements = new List<ITextReplacement>();

        readonly List<ProjectIdentifier> projectsToSave = new List<ProjectIdentifier>();

        readonly List<FileCreation> fileCreations = new List<FileCreation>();

        /// <summary>
        /// The files that were changed by this workUnit execution.
        /// <para/>
        /// Use to notify the IDE of file changes.
        /// </summary>
        public IReadOnlyList<string> ChangedFiles => changedFiles;

        public IReadOnlyList<IWorkUnit> AppliedWorkUnits => appliedWorkUnits;

        public IReadOnlyList<IWorkUnit> PostProcessedWorkUnits => postProcessedWorkUnits;

        public IReadOnlyList<ProjectIdentifier> ProjectsToSave => projectsToSave;

        public IReadOnlyList<ITextReplacement> TextReplacements => textReplacements;

        public IReadOnlyList<FileCreation> FilesToCreate => fileCreations;

        public bool SaveProject { get; set; }

        public void AddChangedFile(string file)
        {
            if (file != null)
            {
                changedFiles.Add(file);
            }
        }

        public void RemoveChangedFile(string file)
        {
            changedFiles.Remove(file);
        }

        public void AddChangedFiles(IEnumerable<string> files)
        {
            if (files != null && files.Any())
            {
                changedFiles.AddRange(files);
            }
        }

        public void AddAppliedWorkUnit(IWorkUnit workUnit)
        {
            if (workUnit != null)
            {
                appliedWorkUnits.Add(workUnit);
            }
        }

        public void RemoveAppliedWorkUnit(IWorkUnit workUnit)
        {
            appliedWorkUnits.Remove(workUnit);
        }

        public void AddAppliedWorkUnits(params IWorkUnit[] workUnits)
        {
            if (workUnits != null && workUnits.Any())
            {
                appliedWorkUnits.AddRange(workUnits);
            }
        }

        public void AddAppliedRespones(IReadOnlyList<IWorkUnit> workUnits)
        {
            if (workUnits != null && workUnits.Any())
            {
                appliedWorkUnits.AddRange(workUnits);
            }
        }

        public void AddTextReplacement(ITextReplacement replacement)
        {
            if (replacement != null)
            {
                textReplacements.Add(replacement);
            }
        }

        public void AddTextReplacements(IEnumerable<ITextReplacement> replacements)
        {
            if (replacements != null && replacements.Any())
            {
                textReplacements.AddRange(replacements);
            }
        }

        public void AddFileCreation(FileCreation creation)
        {
            if (creation != null)
            {
                fileCreations.Add(creation);
            }
        }

        public void AddFileCreations(IEnumerable<FileCreation> creations)
        {
            if (creations != null && creations.Any())
            {
                fileCreations.AddRange(creations);
            }
        }

        public void AddPostProcessedWorkUnit(IWorkUnit workUnit)
        {
            if (workUnit != null)
            {
                workUnit.IsPostProcessed = true;
                postProcessedWorkUnits.Add(workUnit);
            }
        }

        public void AddPostProcessedWorkUnits(IReadOnlyList<IWorkUnit> workUnits)
        {
            if (workUnits != null && workUnits.Any())
            {
                foreach (var workUnit in workUnits)
                {
                    workUnit.IsPostProcessed = true;
                    postProcessedWorkUnits.Add(workUnit);
                }
            }
        }

        public void AddProjectToSave(ProjectIdentifier projectIdentifier)
        {
            if (projectIdentifier != null)
            {
                projectsToSave.Add(projectIdentifier);
            }
        }

        public void AddProjectsToSave(IEnumerable<ProjectIdentifier> projectIdentifiers)
        {
            if (projectIdentifiers != null && projectIdentifiers.Any())
            {
                projectsToSave.AddRange(projectIdentifiers.Where(pi => pi != null));
            }
        }

        public void MergeWith(IWorkExecutionResult other)
        {
            if (other != null)
            {
                AddAppliedRespones(other.AppliedWorkUnits);
                AddChangedFiles(other.ChangedFiles);
                AddTextReplacements(other.TextReplacements);
                AddProjectsToSave(other.ProjectsToSave);
                AddFileCreations(other.FilesToCreate);
                AddPostProcessedWorkUnits(other.PostProcessedWorkUnits);

                OnMergeWith(other);
            }
        }

        public virtual void OnMergeWith(IWorkExecutionResult other)
        {

        }
    }
}
