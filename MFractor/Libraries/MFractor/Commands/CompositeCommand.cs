using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.IOC;

namespace MFractor.Commands
{
    /// <summary>
    /// An <see cref="ICommand"/> implementation that is constructed from 1 to many nested <see cref="ICommand"/>'s.
    /// </summary>
    [PartNotDiscoverable]
    public class CompositeCommand : ICommand
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        public CompositeCommand(string label, string description)
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
        }

        public CompositeCommand(string label, string description, IEnumerable<ICommand> commands)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentException("message", nameof(label));
            }

            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentException("message", nameof(description));
            }

            if (commands == null)
            {
                throw new ArgumentNullException(nameof(commands));
            }

            Label = label;
            Description = description;
            this.commands = commands.ToList();
        }

        readonly List<ICommand> commands = new List<ICommand>();
        public IReadOnlyList<ICommand> Commands => commands;

        public string Label { get; }

        public string Description { get; }

        public virtual void Execute(ICommandContext commandContext)
        {
            // Do nothing. Composite commands are here to allow sub-menus.
        }

        /// <summary>
        /// Adds the exported <typeparamref name="TCommand"/> implementation to this composite command.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <returns></returns>
        public CompositeCommand With<TCommand>() where TCommand : class, ICommand
        {
            return With(Resolver.Resolve<TCommand>());
        }

        /// <summary>
        /// Adds all exported <typeparamref name="TCommand"/> implementations to this composite command.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <returns></returns>
        public CompositeCommand WithMany<TCommand>() where TCommand : class, ICommand
        {
            return WithMany(Resolver.ResolveAll<TCommand>());
        }

        /// <summary>
        /// Adds the <paramref name="command"/> to this composite command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public CompositeCommand With(ICommand command)
        {
            if (command != null)
            {
                commands.Add(command);
            }
            else
            {
                log?.Warning("A null command was provided to the composite command");
                log?.Warning(Environment.StackTrace);
            }

            return this;
        }

        /// <summary>
        /// Adds the <paramref name="commands"/> to this composite command.
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        public CompositeCommand WithMany(IEnumerable<ICommand> commands)
        {
            if (commands is null)
            {
                commands = Enumerable.Empty<ICommand>();
            }

            foreach (var command in commands)
            {
                With(command);
            }

            return this;
        }

        public virtual ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState(true, true, Label, Description, commands);
        }

        public void ExecuteChild<TCommand>(ICommandContext commandContext, Func<ICommand, bool> predicate = null) where TCommand : class, ICommand
        {
            predicate = predicate ?? new Func<ICommand, bool>((ICommand c) => true);

            var child = this.commands.OfType<TCommand>().FirstOrDefault(predicate);

            if (child != null)
            {
                child.Execute(commandContext);
            }
        }

        public void ExecuteLink(string label, ICommandContext commandContext)
        {
            var child = this.commands.OfType<LinkCommand>().FirstOrDefault(lc => lc.Label == label);

            if (child != null)
            {
                child.Execute(commandContext);
            }
        }
    }
}
