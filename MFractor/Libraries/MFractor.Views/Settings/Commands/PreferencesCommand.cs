using System;
using MFractor.Commands;
using MFractor.IOC;

namespace MFractor.Views.Settings.Commands
{
    public abstract class PreferencesCommand<TOptionsWidget> : ICommand where TOptionsWidget : IOptionsWidget, new()
    {
        readonly Lazy<IDispatcher> dispatcher = new Lazy<IDispatcher>(Resolver.Resolve<IDispatcher>);
        IDispatcher Dispatcher => dispatcher.Value;

        protected abstract string Title { get; }

        public void Execute(ICommandContext commandContext)
        {
            Dispatcher.InvokeOnMainThread(() =>
            {
                var dialog = new PreferencesDialog(new TOptionsWidget());

                dialog.Show();
            });
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState(Title, string.Empty)
            {
                CanExecute = true,
                IsVisible = true,
            };
        }
    }
}

