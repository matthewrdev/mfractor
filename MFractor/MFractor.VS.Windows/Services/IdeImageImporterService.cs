using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Images;
using MFractor.Images.Importing;
using MFractor.Images.Utilities;
using MFractor.MSBuild;
using MFractor.Progress;
using MFractor.Text;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.VS.Windows.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IImageImporterService))]
    class IdeImageImporterService : ImageImporterService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        readonly Lazy<IXmlFormattingPolicyService> formattingPolicyService;

        IXmlFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        readonly Lazy<IItemGroupFinder> itemGroupFinder;
        IItemGroupFinder ItemGroupFinder => itemGroupFinder.Value;

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        public IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        readonly Lazy<ITextProviderService> textProviderService;
        public ITextProviderService TextProviderService => textProviderService.Value;

        readonly Lazy<IWorkEngine> workEngine;
        public IWorkEngine WorkEngine => workEngine.Value;

        [ImportingConstructor]
        public IdeImageImporterService(Lazy<IDialogsService> dialogsService, 
            Lazy<IXmlSyntaxWriter> xmlSyntaxWriter,
            Lazy<IXmlFormattingPolicyService> formattingPolicyService,
            Lazy<IItemGroupFinder> itemGroupFinder,
                                       Lazy<IImageUtilities> imageUtil, 
                                       Lazy<IXmlSyntaxParser> xmlSyntaxParser,
                                       Lazy<IWorkEngine> workEngine,
                                      Lazy<ITextProviderService> textProviderService)
            : base(dialogsService, imageUtil)
        {
            this.xmlSyntaxParser = xmlSyntaxParser;
            this.workEngine = workEngine;
            this.textProviderService = textProviderService;
            this.xmlSyntaxWriter = xmlSyntaxWriter;
            this.formattingPolicyService = formattingPolicyService;
            this.itemGroupFinder = itemGroupFinder;
        }

        protected override async Task<bool> AddProjectFiles(ImportImageOperation operation, Project project, IEnumerable<CreateProjectFileWorkUnit> workUnits, IProgressMonitor progressMonitor)
        {
            var itemGroup = new XmlNode("ItemGroup");

            foreach (var density in operation.Densities)
            {
                var virtualPath = ImageDownsamplingHelper.GetVirtualFilePath(operation, density);

                var buildAction = GetBuildAction(operation.TargetProject, operation.ResourceType);

                var child = new XmlNode(buildAction);
                child.IsSelfClosing = true;
                child.HasClosingTag = false;
                child.AddAttribute("Include", virtualPath.Replace("@", "%40"));

                itemGroup.AddChildNode(child);
            }
            
            if (operation.ResourceType == ImageResourceType.AssetCatalog)
            {
                AddImageSetEntry(itemGroup, operation);
            }

            var content = XmlSyntaxWriter.WriteNode(itemGroup, "  ", FormattingPolicyService.GetXmlFormattingPolicy(), true, true, true) + Environment.NewLine;

            var anchor = ItemGroupFinder.FindItemGroups(project).LastOrDefault();

            var insertionPosition = 0;
            if (anchor == null || !anchor.HasClosingTag)
            {
                var provider = TextProviderService.GetTextProvider(project.FilePath);

                if (provider == null)
                {
                    log?.Warning("Failed to insert the entrties for the new image asset; unable to load the projet MSBuild.");
                    return false;
                }

                var syntaxTree = XmlSyntaxParser.ParseText(await provider.GetTextAsync());

                if (syntaxTree.Root == null || syntaxTree.Root.IsSelfClosing)
                {
                    log?.Warning("Failed to insert the entries for the new image asset; unable to locate the root project node in the MSBUILD.");
                    return false;
                }

                insertionPosition = syntaxTree.Root.ClosingTagSpan.Start;
            }
            else
            {
                insertionPosition = anchor.ClosingTagSpan.End;
                content = Environment.NewLine + content;
            }

            var insertionWorkUnit = new InsertTextWorkUnit(content, insertionPosition, project.FilePath);

            WorkEngine.ApplyAsync(insertionWorkUnit).ConfigureAwait(false);

                return true;
        }

        private void AddImageSetEntry(XmlNode itemGroup, ImportImageOperation operation)
        {
            if (operation.ResourceType != ImageResourceType.AssetCatalog)
            {
                return;
            }

            var imageSetPath = ImageDownsamplingHelper.GetIOSAssetCatalogImageSetVirtualPath(operation.ImageName);
            var virtualFilePath = Path.Combine(imageSetPath, "Contents.json");

            var itemEntry = new XmlNode("ImageAsset");
            itemEntry.AddAttribute("Include", virtualFilePath);
            itemEntry.AddChildNode(new XmlNode("Visible")
            {
                Value = "False",
                IsSelfClosing = true,
                HasClosingTag = false,
            });

            itemGroup.AddChildNode(itemEntry);
        }
    }
}
