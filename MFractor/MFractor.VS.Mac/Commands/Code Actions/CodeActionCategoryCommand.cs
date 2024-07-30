using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Analytics;
using MFractor.Code;
using MFractor.Code.CodeActions;
using MFractor.Ide;
using MFractor.IOC;
using MFractor.Licensing;
using MonoDevelop.Components.Commands;

namespace MFractor.VS.Mac.Commands.CodeActions
{
    class CodeActionsCommandAdapter : CommandHandler
    {
        public readonly IReadOnlyList<CodeActionCategory> Categories = new List<CodeActionCategory>()
        {
            CodeActionCategory.Fix,
            CodeActionCategory.Refactor,
            CodeActionCategory.Generate,
            CodeActionCategory.Organise,
            CodeActionCategory.Misc,
            CodeActionCategory.Find,
        };

        protected sealed override void Run()
        {
            // Sealed to prevent misuse.
        }

        protected sealed override void Update(CommandInfo info)
        {
            // Sealed to prevent misuse.
        }

        readonly Lazy<ICodeActionEngine> codeActionEngine = new Lazy<ICodeActionEngine>(() => Resolver.Resolve<ICodeActionEngine>());
        protected ICodeActionEngine CodeActionEngine => codeActionEngine.Value;

        readonly Lazy<ILicensingService> licensingService = new Lazy<ILicensingService>(() => Resolver.Resolve<ILicensingService>());
        protected ILicensingService LicensingService => licensingService.Value;

        readonly Lazy<IAnalyticsService> analyticsService = new Lazy<IAnalyticsService>(() => Resolver.Resolve<IAnalyticsService>());
        protected IAnalyticsService AnalyticsService => analyticsService.Value;

        readonly Lazy<IActiveDocument> activeDocument = new Lazy<IActiveDocument>(() => Resolver.Resolve<IActiveDocument>());
        protected IActiveDocument ActiveDocument => activeDocument.Value;

        readonly Lazy<IFeatureContextFactoryRepository> featureContextFactories = new Lazy<IFeatureContextFactoryRepository>(() => Resolver.Resolve<IFeatureContextFactoryRepository>());
        protected IFeatureContextFactoryRepository FeatureContextFactoryRepository => featureContextFactories.Value;

        protected sealed override Task UpdateAsync(CommandArrayInfo info, System.Threading.CancellationToken cancelToken)
        {
            Update(info);

            return Task.CompletedTask;
        }

        protected sealed override void Update(CommandArrayInfo info)
        {
            if (!ActiveDocument.IsAvailable)
            {
                info.Bypass = true;
                return;
            }

            if (!LicensingService.IsPaid)
            {
                info.Bypass = true;
                return;
            }

            var contextFactories = FeatureContextFactoryRepository.FeatureContextFactories
                                               .Where(e => e.SupportedExecutionTypes.Contains(CodeActionExecutionType.ContextMenuCommand))
                                               .Where(e => e.IsInterestedInDocument(ActiveDocument.CompilationProject, ActiveDocument.FilePath));

            foreach (var factory in contextFactories)
            {
                var context = factory.Retrieve(ActiveDocument.FilePath) ?? factory.CreateFeatureContext(ActiveDocument.CompilationProject, ActiveDocument.FilePath, ActiveDocument.CaretOffset);

                if (context == null)
                {
                    continue;
                }

                context.Syntax = factory.GetSyntaxAtLocation(context.Document.AbstractSyntaxTree, ActiveDocument.CaretOffset);

                if (context?.Syntax == null)
                {
                    continue;
                }

                var codeActionContext = ActiveDocument.GetInteractionLocation();
                var codeActions = CodeActionEngine.RetrieveCodeActions(context, codeActionContext, this.Categories);

                var infos = new CommandInfoSet();

                foreach (var codeAction in codeActions)
                {
                    var suggestions = codeAction.Suggest(context, codeActionContext);
                    foreach (var s in suggestions)
                    {
                        var enabled = true;
                        var description = s.Description;
                        var choice = new CodeActionChoice(s, codeAction, context);
                        var command = new CommandInfo(description, enabled, false);

                        infos.CommandInfos.Add(command, choice);
                    }
                }

                if (infos.CommandInfos.Count != 0)
                {
                    info.Add(infos);
                    info.Bypass = false;
                }
            }
        }

        protected async sealed override void Run(object dataItem)
        {
            var choice = dataItem as CodeActionChoice;

            if (!LicensingService.IsPaid)
            {
                return;
            }

            await ApplyCodeAction(choice);
        }

        protected async Task<bool> ApplyCodeAction(CodeActionChoice choice)
        {
            var interactionLocation = ActiveDocument.GetInteractionLocation();

            choice.CodeAction.ApplyConfiguration(choice.Context.ConfigurationId);

            var result = await CodeActionEngine.Execute(choice, interactionLocation);

            if (result)
            {
                AnalyticsService.Track(choice.CodeAction.AnalyticsEvent);
            }

            return result;
        }
    }
}
