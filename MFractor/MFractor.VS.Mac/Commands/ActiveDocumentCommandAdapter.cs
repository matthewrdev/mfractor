using System;
using MFractor.Commands;
using MFractor.Ide;
using MFractor.Ide.Commands;
using MFractor.IOC;

namespace MFractor.VS.Mac.Commands
{
    /// <summary>
    /// An <see cref="IdeCommandAdapter{TCommand}"/> implementation that specifies the command context as <see cref="IDocumentCommandContext"/>.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public class ActiveDocumentCommandAdapter<TCommand> : IdeCommandAdapter<TCommand> where TCommand : class, ICommand
    {
        readonly Lazy<IActiveDocument> activeDocument = new Lazy<IActiveDocument>(() => Resolver.Resolve<IActiveDocument>());
        public IActiveDocument ActiveDocument => activeDocument.Value;

        protected sealed override ICommandContext GetCommandContext()
        {
            if (!ActiveDocument.IsAvailable)
            {
                return DefaultCommandContext.Instance;
            }

            if (ActiveDocument.CompilationProject is null
                || string.IsNullOrEmpty(ActiveDocument.FilePath))
            {
                return DefaultCommandContext.Instance;
            }

            return new DocumentCommandContext(ActiveDocument.FilePath,
                                              ActiveDocument.CompilationProject,
                                              ActiveDocument.CaretOffset,
                                              ActiveDocument.GetInteractionLocation());
        }
    }
}
