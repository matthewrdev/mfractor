using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Commands;
using MFractor.Ide.WorkUnits;
using MFractor.Progress;
using MFractor.Work;

namespace MFractor.Images.ImageManager.Commands
{
    class SearchForUsagesCommand : ImageManagerCommand
    {
        public override string AnalyticsEvent => "Search For Image Usages";

        readonly CancellationTokenSource searchTokenSource = new CancellationTokenSource();

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        readonly Lazy<ISearchProgressService> searchProgressService;
        public ISearchProgressService SearchProgressService => searchProgressService.Value;

        readonly Lazy<IImageAssetUsageService> imageAssetUsageService;
        public IImageAssetUsageService ImageAssetUsageService => imageAssetUsageService.Value;

        readonly Lazy<IDialogsService> dialogsService;
        public IDialogsService DialogsService => dialogsService.Value;

        readonly Lazy<IProductInformation> productInformation;
        public IProductInformation ProductInformation => productInformation.Value;

        [ImportingConstructor]
        public SearchForUsagesCommand(Lazy<IWorkEngine> workEngine,
                                      Lazy<ISearchProgressService> searchProgressService,
                                      Lazy<IImageAssetUsageService> imageAssetUsageService,
                                      Lazy<IDialogsService> dialogsService,
                                      Lazy<IProductInformation> productInformation)
        {
            this.workEngine = workEngine;
            this.searchProgressService = searchProgressService;
            this.imageAssetUsageService = imageAssetUsageService;
            this.dialogsService = dialogsService;
            this.productInformation = productInformation;
        }

        protected override void OnExecute(IImageManagerCommandContext commandContext)
        {
            if (ProductInformation.Product == Product.VisualStudioWindows)
            {
                DialogsService.ShowMessage("The 'Search for usages' feature is coming soon to Visual Studio Windows.", "Ok");
                return;
            }

            var asset = commandContext.ImageAsset;

            Search(asset).ConfigureAwait(false);
        }

        protected override ICommandState OnGetExecutionState(IImageManagerCommandContext commandContext)
        {
            if (commandContext.ImageAsset == null)
            {
                return default;
            }

            return default;
            //return new CommandState(true, true, "Find Usages", "Search for all usages of this image asset in your solution.");
        }

        IProgressMonitor searchSession;

        async Task Search(IImageAsset imageAsset)
        {
            if (searchSession != null)
            {
                DialogsService.Confirm("There is a search already in progress. Please wait for the current search to finish.", "Ok");
                return;
            }

            searchTokenSource.Cancel();

            searchSession = SearchProgressService.StartSearch($"Searching for all usages of {imageAsset.Name}...");

            IReadOnlyList<IImageAssetUsage> usages;
            try
            {
                usages = (await ImageAssetUsageService.FindUsages(imageAsset, searchSession)).ToList();
            }
            finally
            {
                searchSession.Dispose();
                searchSession = null;
            }

            var navigationTargets = usages.Select(u => new NavigateToFileSpanWorkUnit(u.Span, u.ProjectFile.FilePath, u.ProjectFile.CompilationProject)).ToList();

            var result = new NavigateToFileSpansWorkUnit(navigationTargets, false);

            await WorkEngine.ApplyAsync(result);
        }
    }
}
