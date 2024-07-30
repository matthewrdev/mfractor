using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Commands.Attributes;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Utilities;
using MonoDevelop.Components.Commands;

namespace MFractor.VS.Mac.Commands
{
    /// <summary>
    /// Adapts an MFractor <see cref="ICommand"/> into a Visual Studio Mac <see cref="CommandHandler"/>.
    /// <para/>
    /// This allows MFractor <see cref="ICommand"/>'s to be consumed natively within Visual Studio Mac.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public class IdeCommandAdapter<TCommand> : CommandHandler where TCommand : class, ICommand
    {
        readonly Logging.ILogger log = Logging.Logger.Create(typeof(TCommand).Name + ".cs");

        readonly Lazy<TCommand> commandImpl = new Lazy<TCommand>(() => Resolver.Resolve<TCommand>());
        protected TCommand Command => commandImpl.Value;

        readonly Lazy<IAnalyticsService> analyticsService = new Lazy<IAnalyticsService>(Resolver.Resolve<IAnalyticsService>);
        IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<ILicensingService> licensingService = new Lazy<ILicensingService>(Resolver.Resolve<ILicensingService>);
        ILicensingService LicensingService => licensingService.Value;

        protected virtual bool RequiresActivation { get; } = true;

        protected virtual ICommandContext GetCommandContext()
        {
            return DefaultCommandContext.Instance;
        }

        void Track(ICommand command)
        {
            if (command is IAnalyticsFeature feature)
            {
                AnalyticsService.Track(feature);
            }
        }

        protected sealed override void Run()
        {
            try
            {
                Execute(Command);
            }
            catch (Exception ex)
            {
                Debugger.Break();
                log?.Exception(ex);
            }
        }

        protected sealed override void Run(object dataItem)
        {
            try
            {
                var command = dataItem as ICommand ?? Command;

                Execute(command);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
                Debugger.Break();
            }
        }

        void Execute(ICommand command)
        {
            try
            {
                var context = GetCommandContext();

                if (context == null)
                {
                    return;
                }

                var state = command.GetExecutionState(context);

                if (AttributeHelper.HasAttribute<RequiresLicenseAttribute>(command.GetType()))
                {
                    if (!LicensingService.IsPaid)
                    {
                        return;
                    }
                }

                command.Execute(context);

                Track(command);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
                Debugger.Break();
            }
        }

        protected sealed override void Update(CommandArrayInfo info)
        {
            try
            {
                var context = GetCommandContext();

                if (context == null)
                {
                    return;
                }

                ICommandState state = null;

                try
                {
                    state = Command.GetExecutionState(context);
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                    log?.Exception(ex);
                }

                if (state != null)
                {
                    info.Bypass = !state.BlockSubsequentCommands;

                    Build(info, state, context);
                }
            }
            catch (Exception ex)
            {
                Debugger.Break();
                log?.Exception(ex);
            }
        }

        void Build(CommandArrayInfo commandArrayInfo, ICommandState state, ICommandContext context)
        {
            var infoSet = new CommandInfoSet()
            {
                Text = state.Label,
                Description = state.Description,
                Enabled = IsExecutionAllowed(state),
                Visible = state.IsVisible,
                Bypass = !state.BlockSubsequentCommands
            };

            Build(infoSet, state, context);

            if (infoSet.CommandInfos.Any())
            {
                infoSet.Visible = infoSet.CommandInfos.Any(ci => ci.Visible);
                commandArrayInfo.Add(infoSet);
            }
            else
            {
                infoSet.Visible = false;
            }
        }

        protected virtual bool IsExecutionAllowed(ICommandState state)
        {
            if (!RequiresActivation)
            {
                return state.CanExecute;
            }

            if (RequiresActivation && !LicensingService.HasActivation)
            {
                return false;
            }

            if (state == null)
            {
                return false;
            }

            return state.CanExecute;
        }

        void Build(CommandInfoSet commandInfoSet, ICommandState state, ICommandContext context)
        {
            foreach (var command in state.NestedCommands)
            {
                try
                {
                    var commandState = command.GetExecutionState(context);

                    if (commandState == null)
                    {
                        continue;
                    }

                    if (commandState.IsSeparator)
                    {
                        commandInfoSet.CommandInfos.AddSeparator();
                    }
                    else
                    {
                        var info = new CommandInfo();

                        Apply(info, commandState);

                        commandInfoSet.CommandInfos.Add(info, command);
                    }
                }
                catch (Exception ex)
                {
                    Debugger.Break();
                    log?.Exception(ex);
                }
            }
        }

        protected sealed override Task UpdateAsync(CommandArrayInfo info, CancellationToken cancelToken)
        {
            return base.UpdateAsync(info, cancelToken);
        }

        protected sealed override void Update(CommandInfo info)
        {
            info.Visible = false;
            info.Enabled = false;

            try
            {
                var context = GetCommandContext();

                ICommandState state = null;

                if (context != null)
                {
                    try
                    {
                        state = Command.GetExecutionState(context);
                    }
                    catch (Exception ex)
                    {
                        //Debugger.Break();
                        log?.Exception(ex);
                    }

                    if (state != null && state.IsSeparator)
                    {
                        throw new InvalidOperationException("Separator command states are not supported for non-array commands");
                    }
                }

                Apply(info, state);
            }
            catch (Exception ex)
            {
                Debugger.Break();
                log?.Exception(ex);
            }
        }

        void Apply(CommandInfo info, ICommandState state)
        {
            if (state == null)
            {
                info.Visible = false;
                info.Enabled = false;
                return;
            }

            if (!string.IsNullOrEmpty(state.Label))
            {
                info.Text = state.Label;
            }
            info.Description = state.Description;
            info.Visible = state.IsVisible;
            info.Enabled = state.CanExecute;
            info.Bypass = !state.BlockSubsequentCommands;
        }

        protected sealed override Task UpdateAsync(CommandInfo info, CancellationToken cancelToken)
        {
            Update(info);

            return Task.CompletedTask;
        }
    }
}
