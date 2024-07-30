using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Code.Formatting;
using MFractor.Code.WorkUnits;
using MFractor.Images.Importing;
using MFractor.MSBuild;
using MFractor.Text;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.VS.Windows.Workspace
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(Images.Importing.IWorkspaceProjectService))]
    class VSWinWorkspaceProjectService : Images.Importing.IWorkspaceProjectService
    {
        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        readonly Lazy<IXmlFormattingPolicyService> formattingPolicyService;
        readonly Lazy<IItemGroupFinder> itemGroupFinder;
        readonly Lazy<ITextProviderService> textProviderService;
        readonly Lazy<IWorkEngine> workEngine;
        readonly Lazy<IProjectFileEntryFinder> projectFileEntryFinder;
        readonly Lazy<IProjectService> projectService;

        IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        IXmlFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        IItemGroupFinder ItemGroupFinder => itemGroupFinder.Value;

        ITextProviderService TextProviderService => textProviderService.Value;

        IWorkEngine WorkEngine => workEngine.Value;

        IProjectService ProjectService => projectService.Value;

        IProjectFileEntryFinder ProjectFileEntryFinder => projectFileEntryFinder.Value;

        [ImportingConstructor]
        public VSWinWorkspaceProjectService(
            Lazy<IXmlSyntaxWriter> xmlSyntaxWriter,
            Lazy<IXmlSyntaxParser> xmlSyntaxParser,
            Lazy<IXmlFormattingPolicyService> formattingPolicyService,
            Lazy<IItemGroupFinder> itemGroupFinder,
            Lazy<ITextProviderService> textProviderService,
            Lazy<IWorkEngine> workEngine,
            Lazy<IProjectFileEntryFinder> projectFileEntryFinder,
            Lazy<IProjectService> projectService)
        {
            this.xmlSyntaxWriter = xmlSyntaxWriter;
            this.xmlSyntaxParser = xmlSyntaxParser;
            this.formattingPolicyService = formattingPolicyService;
            this.itemGroupFinder = itemGroupFinder;
            this.textProviderService = textProviderService;
            this.workEngine = workEngine;
            this.projectFileEntryFinder = projectFileEntryFinder;
            this.projectService = projectService;
        }

        public async Task<bool> AddProjectFilesAsync(Project project, IEnumerable<CreateProjectFileWorkUnit> workUnits)
        {
            await DeleteFilesAsync(project, workUnits.Select(w => w.FilePath).ToArray());

            var itemGroup = new XmlNode("ItemGroup");
            foreach (var workUnit in workUnits)
            {
                var virtualPath = workUnit.VirtualFilePath;
                var buildAction = workUnit.BuildAction;
                
                var child = new XmlNode(buildAction);
                child.IsSelfClosing = true;
                child.HasClosingTag = false;
                child.AddAttribute("Include", virtualPath.Replace("@", "%40"));

                itemGroup.AddChildNode(child);

                // TODO: Is it the best place to put this code?
                WriteFileContent(workUnit);
            }

            var content = XmlSyntaxWriter.WriteNode(itemGroup, "  ", FormattingPolicyService.GetXmlFormattingPolicy(), true, true, true) + Environment.NewLine;
            var anchor = ItemGroupFinder.FindItemGroups(project).LastOrDefault();
            var insertionPosition = 0;

            if (anchor == null || !anchor.HasClosingTag)
            {
                var provider = TextProviderService.GetTextProvider(project.FilePath);
                if (provider == null)
                {
                    // Log
                    return false;
                }

                var syntaxTree = XmlSyntaxParser.ParseText(await provider.GetTextAsync());
                if (syntaxTree.Root == null || syntaxTree.Root.IsSelfClosing)
                {
                    // Log
                    return false;
                }

                insertionPosition = syntaxTree.Root.ClosingTagSpan.End;
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

        void WriteFileContent(CreateProjectFileWorkUnit workUnit)
        {
            var filePath = workUnit.FilePath;
            var file = new FileInfo(filePath);
            file.Directory.Create();

            if (workUnit.IsBinary)
            {
                if (workUnit.WriteContentAction != null)
                {
                    using var result = File.OpenWrite(filePath);
                    workUnit.WriteContentAction(result);
                }
            }
            else
            {
                var fileContent = workUnit.FileContent;
                if (workUnit.WriteContentAction != null)
                {
                    using var stream = new MemoryStream();
                    workUnit.WriteContentAction(stream);
                    stream.Seek(0, SeekOrigin.Begin);

                    using var result = new StreamReader(stream);
                    fileContent = result.ReadToEnd();
                }

                File.WriteAllText(filePath, fileContent);
            }
        }

        public Task DeleteFileAsync(Project project, string filePath) => DeleteFilesAsync(project, filePath);

        public Task DeleteFilesAsync(Project project, params string[] filePaths)
        {
            var textProvider = TextProviderService.GetTextProvider(project.FilePath);
            if (textProvider == null)
            {
                // TODO: Log
                return Task.CompletedTask;
            }

            var syntaxTree = XmlSyntaxParser.ParseText(textProvider.GetText());
            if (syntaxTree == null)
            {
                // TODO: Log
                return Task.CompletedTask;
            }

            var workUnits = new List<IWorkUnit>();
            var elements = new List<XmlNode>();

            foreach (var filePath in filePaths)
            {
                var projectFile = ProjectService.GetProjectFileWithFilePath(project, filePath);
                if (projectFile is null)
                {
                    continue;
                }

                var entry = ProjectFileEntryFinder.FindProjectFile(syntaxTree, projectFile);
                if (entry != null)
                {
                    elements.Add(entry);
                }

                workUnits.Add(new DeleteFileWorkUnit
                {
                    FilePath = filePath,
                });
            }

            workUnits.Add(new DeleteXmlSyntaxWorkUnit
            {
                FilePath = project.FilePath,
                Syntaxes = elements,
            });

            WorkEngine.ApplyAsync(workUnits).ConfigureAwait(false);
            return Task.CompletedTask;
        }
        
        public bool IsFileExistsInProject(Project project, string filePath)
        {
            return false;
        }
    }
}
