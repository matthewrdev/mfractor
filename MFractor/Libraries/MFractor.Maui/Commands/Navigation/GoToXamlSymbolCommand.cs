using System;
using System.ComponentModel.Composition;
using MFractor.Analytics;
using MFractor.Commands;
using MFractor.Commands.Attributes;
using MFractor.Ide.Commands;
using MFractor.Navigation;
using MFractor.Work;

namespace MFractor.Maui.Commands.Navigation
{
    [RequiresLicense]
    [Export]
    class GoToXamlSymbolCommand : ICommand, IAnalyticsFeature
    {
        readonly Lazy<INavigationService> navigationService;
        public INavigationService NavigationService => navigationService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        public string AnalyticsEvent => "Go To Xaml Symbol";

        [ImportingConstructor]
        public GoToXamlSymbolCommand(Lazy<INavigationService> navigationService,
                                     Lazy<IWorkEngine> workEngine)
        {
            this.navigationService = navigationService;
            this.workEngine = workEngine;
        }

        public void Execute(ICommandContext commandContext)
        {
            var document = commandContext as IDocumentCommandContext;

            var context = new NavigationContext(document.FilePath, document.CompilationProject, document.CaretOffset, document.InteractionLocation);

            NavigationService.Suggest(context)
                             .ContinueWith(async t => await NavigationService.Navigate(context, t.Result))
                             .ContinueWith(t => WorkEngine.ApplyAsync(t.Result.Result)).ConfigureAwait(false);
        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            var activeDocument = commandContext as IDocumentCommandContext;
            if (activeDocument == null)
            {
                return default;
            }

            var document = commandContext as IDocumentCommandContext;

            var context = new NavigationContext(document.FilePath, document.CompilationProject, document.CaretOffset, document.InteractionLocation);

            var suggestion = NavigationService.Suggest(context).Result;

            if (suggestion is null)
            {
                return default;
            }

            return new CommandState()
            {
                BlockSubsequentCommands = true,
                Label = "Go To Xaml Symbol",
                Description = "Navigate to the XAML symbol under the cursor",
            };
        }
    }
}