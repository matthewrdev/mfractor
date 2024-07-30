using System;
using System.ComponentModel.Design;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Commands.Attributes;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Work.WorkUnits;
using MFractor.Utilities;
using Microsoft.VisualStudio.Shell;
using MFractor.Work;
using MFractor.Ide;
using MFractor.Ide.Commands;

namespace MFractor.VS.Windows.Commands
{
    public static class IdeCommandAdapter
    {
        static readonly Logging.ILogger log = Logging.Logger.Create();

        static ILicensingService LicensingService => Resolver.Resolve<ILicensingService>();
        static IWorkEngine WorkEngine => Resolver.Resolve<IWorkEngine>();
        static IActiveDocument ActiveDocument => Resolver.Resolve<IActiveDocument>();

        public static void BindMenuCommand<TCommand>(OleMenuCommandService commandService, Guid menuGroup, int commandId) where TCommand : class, ICommand
        {
            var command = CreateMenuCommand<TCommand>(menuGroup, commandId);

            commandService.AddCommand(command);
        }

        public static void BindSolutionPadCommand<TCommand>(OleMenuCommandService commandService, Guid menuGroup, int commandId) where TCommand : class, ICommand
        {
            var command = CreateSolutionPadCommand<TCommand>(menuGroup, commandId);

            commandService.AddCommand(command);
        }

        public static void BindActiveDocumentCommand<TCommand>(OleMenuCommandService commandService, Guid menuGroup, int commandId) where TCommand : class, ICommand
        {
            var command = CreateActiveDocumentCommand<TCommand>(menuGroup, commandId);

            commandService.AddCommand(command);
        }

        /// <summary>
        /// Helper to Create a new Menu Command from the Command Set GUID and Command ID.
        /// </summary>
        /// <param name="menuGroup">The Command Set GUID where the command ID is declared.</param>
        /// <param name="commandId">The ID Symbol of the Command inside the Command Set.</param>
        /// <param name="queryStatusEventHandler">The event Handler that will be called before the command might appear to query its status.</param>
        /// <param name="commandEventHandler">The Event Handler that will be called when the command is invoked.</param>
        /// <returns>A MenuCommand object.</returns>
        public static OleMenuCommand CreateMenuCommand<TCommand>(Guid menuGroup, int commandId) where TCommand : class, ICommand
        {
            return CreateCommand<TCommand>(menuGroup, commandId, () => DefaultCommandContext.Instance);
        }

        public static OleMenuCommand CreateSolutionPadCommand<TCommand>(Guid menuGroup, int commandId) where TCommand : class, ICommand
        {
            return CreateCommand<TCommand>(menuGroup, commandId, () => SolutionPadCommandContext.Create());
        }

        public static OleMenuCommand CreateActiveDocumentCommand<TCommand>(Guid menuGroup, int commandId) where TCommand : class, ICommand
        {
            return CreateCommand<TCommand>(menuGroup, commandId, () =>
            {
                if (!ActiveDocument.IsAvailable)
                {
                    return default;
                }

                return new DocumentCommandContext(ActiveDocument.FilePath, ActiveDocument.CompilationProject, ActiveDocument.CaretOffset, ActiveDocument.GetInteractionLocation());
            });
        }

        public static OleMenuCommand CreateCommand<TCommand>(Guid menuGroup, int commandId, Func<ICommandContext> commandContextFactory = null) where TCommand : class, ICommand
        {
            try
            {
                log?.Info("   -> Binding " + typeof(TCommand));
                commandContextFactory = commandContextFactory ?? new Func<ICommandContext>(() => DefaultCommandContext.Instance);
                var command = Resolver.Resolve<TCommand>();

                if (command == null)
                {
                    log?.Error("No implementation for " + typeof(TCommand) + " was found.");
                    return default;
                }

                var commandIdentifier = new CommandID(menuGroup, commandId);

                void executeHandler(object sender, EventArgs args)
                {
                    try
                    {
                        var context = commandContextFactory.Invoke();

                        if (context == null)
                        {
                            return;
                        }

                        var state = command.GetExecutionState(context);

                        if (AttributeHelper.HasAttribute<RequiresLicenseAttribute>(command.GetType()))
                        {
                            if (!LicensingService.IsPaid)
                            {
                                WorkEngine.ApplyAsync(new RequestTrialPromptWorkUnit($"{state.Label} is a Professional-only MFractor feature. Please upgrade or request a trial.", (command as IAnalyticsFeature)?.AnalyticsEvent)).ConfigureAwait(false);
                                return;
                            }
                        }

                        command.Execute(context);

                        if (command is IAnalyticsFeature feature)
                        {
                            Resolver.Resolve<IAnalyticsService>().Track(feature);
                        }
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                }

                void updateHandler(object sender, EventArgs args)
                {
                    if (sender is OleMenuCommand oleMenuCommand)
                    {
                        Update(oleMenuCommand, command, commandContextFactory.Invoke());
                    }
                }

                var oleCommand = new OleMenuCommand(executeHandler, commandIdentifier);
                oleCommand.BeforeQueryStatus += updateHandler;

                return oleCommand;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return default;
        }

        static void Update(OleMenuCommand oleMenuCommand, ICommand command, ICommandContext context)
        {
            if (oleMenuCommand is null)
            {
                throw new ArgumentNullException(nameof(oleMenuCommand));
            }

            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            ICommandState state = default;

            if (context != null)
            {
                try
                {
                    state = command.GetExecutionState(context);
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }

            if (state != null)
            {
                oleMenuCommand.Enabled = state.CanExecute;
                oleMenuCommand.Visible = state.IsVisible;
                oleMenuCommand.Text = state.Label;
            }
            else
            {
                oleMenuCommand.Enabled = false;
                oleMenuCommand.Visible = false;
            }
        }
    }
}
