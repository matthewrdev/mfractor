using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Text;
using MFractor.Utilities;

namespace MFractor.Work
{
    /// <summary>
    /// The base class for a new workUnit engine implementation; the engine that takes MFractor <see cref="IWorkUnit"/> objects and applies them onto the IDE.
    /// </summary>
    public abstract class WorkEngine : IWorkEngine
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IWorkUnitHandlerRepository> workUnitHandlerRepository;
        protected IWorkUnitHandlerRepository WorkUnitHandlerRepository => workUnitHandlerRepository.Value;

        readonly Lazy<ITextReplacementService> textReplacementService;
        public ITextReplacementService TextReplacementService => textReplacementService.Value;

        protected WorkEngine(Lazy<IWorkUnitHandlerRepository> workUnitHandlerRepository,
                             Lazy<ITextReplacementService> textReplacementService)
        {
            this.workUnitHandlerRepository = workUnitHandlerRepository;
            this.textReplacementService = textReplacementService;
        }

        
        /// <summary>
        /// Applies the workUnit to the IDE.
        /// </summary>
        /// <returns>The workUnit async.</returns>
        /// <param name="workUnit">WorkUnit.</param>
        public async Task<bool> ApplyAsync(IWorkUnit workUnit, string description, IProgressMonitor progressMonitor = null)
        {
            return await ApplyAsync(workUnit.AsList(), description, progressMonitor);
        }

        /// <summary>
        /// Applies the workUnit to the IDE.
        /// </summary>
        /// <returns>The workUnit async.</returns>
        /// <param name="workUnit">WorkUnit.</param>
        public async Task<bool> ApplyAsync(IWorkUnit workUnit, IProgressMonitor progressMonitor = null)
        {
            return await ApplyAsync(workUnit.AsList(), string.Empty, progressMonitor);
        }

        /// <summary>
        /// Applies the workUnits to the IDE.
        /// </summary>
        /// <returns>The workUnit async.</returns>
        /// <param name="workUnits">WorkUnit.</param>
        public async Task<bool> ApplyAsync(IReadOnlyList<IWorkUnit> workUnits, IProgressMonitor progressMonitor = null)
        {
            return await ApplyAsync(workUnits, string.Empty, progressMonitor);
        }

        protected virtual async Task<WorkExecutionResult> ExecuteWorkUnitAsync(IReadOnlyList<IWorkUnit> workUnits, IProgressMonitor progressMonitor)
        {
            var result = new WorkExecutionResult();

            try
            {
                foreach (var workUnit in workUnits)
                {
                    if (workUnit.IsPostProcessed)
                    {
                        workUnit.IsPostProcessed = false;
                        result.AddPostProcessedWorkUnit(workUnit);
                    }
                    else
                    {
                        var workUnitType = workUnit.GetType();
                        var handler = WorkUnitHandlerRepository.GetWorkUnitHandler(workUnitType);

                        if (handler != null)
                        {
                            var task = handler.Execute(workUnit, progressMonitor);

                            if (task == null)
                            {
                                log?.Warning("The work unit handler for " + workUnitType + " returned a null task. Was this intended?");
                                continue;
                            }

                            var workUnitResult = await task;
                            result.MergeWith(workUnitResult);
                        }
                        else
                        {
                            log?.Warning("The work unit engine failed to retrieve a workUnit handler for " + workUnitType + ". Has a IWorkUnitHandler implementation been declared for this workUnit?");
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Suppress operation cancellations.
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return result;
        }

        protected async Task<bool> CommitResultAsync(IWorkExecutionResult result, IProgressMonitor progressMonitor)
        {
            try
            {
                if (result.TextReplacements != null && result.TextReplacements.Any())
                {
                    TextReplacementService.ApplyTextReplacements(result.TextReplacements, progressMonitor);
                }

                await ApplyChanges(result.ProjectsToSave, result.FilesToCreate, progressMonitor);

                return true;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return false;
        }

        protected abstract Task ApplyChanges(IReadOnlyList<ProjectIdentifier> projectsToSave, IReadOnlyList<FileCreation> filesToCreate, IProgressMonitor progressMonitor);

        protected abstract void NotifyChangedFiles(WorkExecutionResult workUnitResult);

        protected abstract Task OpenCreatedFiles(WorkExecutionResult workUnitResult);

        public async Task<bool> ApplyAsync(IReadOnlyList<IWorkUnit> workUnits, string description, IProgressMonitor progressMonitor = null)
        {
            var monitor = progressMonitor ?? new StubProgressMonitor();

            try
            {
                var result = await ExecuteWorkUnitAsync(workUnits, monitor);

                var success = await CommitResultAsync(result, monitor);

                //
                // This was changed from an Task.Run implementation that was causing a deadlock on Windows
                await Task.Delay(200);
                NotifyChangedFiles(result);
                await OpenCreatedFiles(result);

                if (result.PostProcessedWorkUnits.Any())
                {
                    var postProcessed = new List<IWorkUnit>();
                    postProcessed.AddRange(result.PostProcessedWorkUnits);

                    foreach (var r in postProcessed)
                    {
                        r.IsPostProcessed = false;
                    }

                    await ApplyAsync(postProcessed, description, progressMonitor);
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
            finally
            {
                if (monitor != null)
                {
                    monitor.Dispose();
                }
            }

            return true;
        }
    }
}
