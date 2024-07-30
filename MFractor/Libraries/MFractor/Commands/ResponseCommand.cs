using System;
using System.ComponentModel.Composition;
using MFractor.Work;

namespace MFractor.Commands
{
    /// <summary>
    /// A command that triggers the <see cref="WorkUnit"/> in the products <see cref="IWorkEngine""/>.
    /// </summary>
    [PartNotDiscoverable]
    public class WorkUnitCommand : ICommand
    {
        public virtual string Name => string.Empty;

        public virtual string Identifier => string.Empty;

        readonly Lazy<IWorkEngine> workEngine;
        protected IWorkEngine  WorkEngine => workEngine.Value;

        public WorkUnitCommand(string label,
                               string description,
                               IWorkUnit workUnit,
                               Lazy<IWorkEngine> workEngine)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("message", nameof(label));
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentException("message", nameof(description));
            }

            Label = label;
            Description = description;
            WorkUnit = workUnit ?? throw new ArgumentNullException(nameof(workUnit));
            this.workEngine = workEngine ?? throw new ArgumentNullException(nameof(workEngine));
        }

        /// <summary>
        /// The label for this command.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// A description of this command.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The workUnit to execute for this command.
        /// </summary>
        public IWorkUnit WorkUnit { get; }

        /// <summary>
        /// Execute the command given the provided <paramref name="commandContext"/>.
        /// </summary>
        /// <param name="commandContext"></param>
        public virtual void Execute(ICommandContext commandContext)
        {
            WorkEngine.ApplyAsync(WorkUnit).ConfigureAwait(false);
        }

        /// <summary>
        /// The execution state of this command, that is, can this command execute, what is it's label/description and any child commands it should show.
        /// <para/>
        /// It is safe to return null from this method.
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        public virtual ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState(true, true, Label, Description);
        }
    }
}
