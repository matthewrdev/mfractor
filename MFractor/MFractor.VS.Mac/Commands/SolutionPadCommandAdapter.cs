using System;
using MFractor.Commands;
using MFractor.Ide;
using MFractor.Ide.Commands;
using MFractor.IOC;

namespace MFractor.VS.Mac.Commands
{
    /// <summary>
    /// An <see cref="IdeCommandAdapter{TCommand}"/> implementation that specifies the command context as <see cref="ISolutionPadCommandContext"/>.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public class SolutionPadCommandAdapter<TCommand> : IdeCommandAdapter<TCommand> where TCommand : class, ICommand
    {
        readonly Lazy<ISolutionPad> solutionPad = new Lazy<ISolutionPad>(() => Resolver.Resolve<ISolutionPad>());
        public ISolutionPad SolutionPad => solutionPad.Value;

        protected sealed override ICommandContext GetCommandContext()
        {
            return SolutionPadCommandContext.Create();
        }
    }
}
