using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;

namespace MFractor.Views.ProjectSelection
{
    class ProjectSelectorWorkUnitHandler : WorkUnitHandler<ProjectSelectorWorkUnit>
    {
        readonly Lazy<IWorkEngine> workEngine;
        IWorkEngine  WorkEngine => workEngine.Value;

        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        [ImportingConstructor]
        public ProjectSelectorWorkUnitHandler(Lazy<IWorkEngine> workEngine,
                                              Lazy<IRootWindowService> rootWindowService,
                                              Lazy<IDispatcher> dispatcher)
        {
            this.workEngine = workEngine;
            this.rootWindowService = rootWindowService;
            this.dispatcher = dispatcher;
        }

        public override async Task<IWorkExecutionResult> OnExecute(ProjectSelectorWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            var source = new TaskCompletionSource<IReadOnlyList<Microsoft.CodeAnalysis.Project>>();

            Dispatcher.InvokeOnMainThread(() =>
            {
                if (workUnit.Mode == ProjectSelectorMode.Multiple)
                {
                    LaunchMultiProjectSelector(workUnit, source);
                }
                else
                {
                    LaunchSingleProjectSelector(workUnit, source);
                }
                
            });

            IReadOnlyList<Microsoft.CodeAnalysis.Project> choices = null;
            try
            {
                choices = await source.Task;
            }
            catch
            {
            }

            var result = new WorkExecutionResult();

            if (choices == null || !choices.Any())
            {
                return result;
            }

            if (workUnit.ProjectSelectionCallback != null)
            {
                var workUnits = workUnit.ProjectSelectionCallback.Invoke(choices);

                if (workUnits != null && workUnits.Any())
                {
                    var nameFixResult = await WorkEngine.ApplyAsync(workUnits);
                }
            }

            return result;
        }

        void LaunchMultiProjectSelector(ProjectSelectorWorkUnit workUnit, TaskCompletionSource<IReadOnlyList<Project>> source)
        {
            var dialog = new MultiProjectSelectionDialog(workUnit.Choices, workUnit.EnabledChoices, workUnit.Title, workUnit.Description);
            dialog.OnProjectsSelected += (sender, e) =>
            {
                source.SetResult(e.Projects);
            };

            dialog.Closed += (sender, e) =>
            {
                source.TrySetCanceled();
            };

            dialog.InitialLocation = Xwt.WindowLocation.CenterParent;
            dialog.Run(RootWindowService.RootWindowFrame);
        }

        void LaunchSingleProjectSelector(ProjectSelectorWorkUnit workUnit, TaskCompletionSource<IReadOnlyList<Project>> source)
        {
            var dialog = new SingleProjectSelectionDialog(workUnit.Choices, workUnit.Title, workUnit.Description);
            dialog.OnProjectsSelected += (sender, e) =>
            {
                source.SetResult(e.Projects);
            };

            dialog.Closed += (sender, e) =>
            {
                source.TrySetCanceled();
            };

            dialog.InitialLocation = Xwt.WindowLocation.CenterParent;
            dialog.Run(RootWindowService.RootWindowFrame);
        }
    }
}
