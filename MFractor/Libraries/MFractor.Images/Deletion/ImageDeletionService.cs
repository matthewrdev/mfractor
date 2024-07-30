using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.WorkUnits;
using MFractor.MSBuild;
using MFractor.Text;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.Deletion
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IImageDeletionService))]
    class ImageDeletionService : IImageDeletionService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [ImportingConstructor]
        public ImageDeletionService(Lazy<IXmlSyntaxParser> xmlSyntaxParser,
                                    Lazy<IProjectFileEntryFinder> projectFileEntryFinder,
                                    Lazy<ITextProviderService> textProviderService)
        {
            this.xmlSyntaxParser = xmlSyntaxParser;
            this.projectFileEntryFinder = projectFileEntryFinder;
            this.textProviderService = textProviderService;
        }

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        public IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        readonly Lazy<IProjectFileEntryFinder> projectFileEntryFinder;
        public IProjectFileEntryFinder ProjectFileEntryFinder => projectFileEntryFinder.Value;

        readonly Lazy<ITextProviderService> textProviderService;
        public ITextProviderService TextProviderService => textProviderService.Value;

        public IReadOnlyList<IWorkUnit> Delete(IImageAsset imageAsset)
        {
            var workUnits = new List<IWorkUnit>();

            foreach (var project in imageAsset.Projects)
            {
                var result = Delete(project, imageAsset);

                if (result != null && result.Any())
                {
                    workUnits.AddRange(result);
                }
            }

            return workUnits;
        }

        public IReadOnlyList<IWorkUnit> Delete(Project project, IImageAsset imageAsset)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (imageAsset is null)
            {
                throw new ArgumentNullException(nameof(imageAsset));
            }

            var files = imageAsset.GetAssetsFor(project);

            if (!files.Any())
            {
                return Array.Empty<IWorkUnit>();
            }

            var textProvider = TextProviderService.GetTextProvider(project.FilePath);

            if (textProvider == null)
            {
                log?.Warning($"Cannot remove the image asset '{imageAsset.Name}' from {project.Name} as the MSBuild contents could not be loaded.");
                return null;
            }

            var syntaxTree = XmlSyntaxParser.ParseText(textProvider.GetText());

            if (syntaxTree == null)
            {
                log?.Warning($"Cannot remove the image asset '{imageAsset.Name}' from {project.Name} as the MSBuild contents could not be parsed.");
                return null;
            }

            var workUnits = new List<IWorkUnit>();
            var elements = new List<XmlNode>();

            foreach (var file in files)
            {
                var entry = ProjectFileEntryFinder.FindProjectFile(syntaxTree, file);

                if (entry != null)
                {
                    elements.Add(entry);
                }

                workUnits.Add(new DeleteFileWorkUnit()
                {
                    FilePath = file.FilePath,
                });
            }

            workUnits.Add(new DeleteXmlSyntaxWorkUnit()
            {
                FilePath = project.FilePath,
                Syntaxes = elements,
            });

            return workUnits;
        }


        public IReadOnlyList<IWorkUnit> Delete(IEnumerable<IProjectFile> files)
        {
            if (files is null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            var groupedFiles = files.GroupBy(f => f.CompilationProject).ToDictionary(f => f.Key, f => f.ToList());

            var workUnits = new List<IWorkUnit>();

            foreach (var group in groupedFiles)
            {
                var result = Delete(group.Key, group.Value);

                if (result != null && result.Any())
                {
                    workUnits.AddRange(result);
                }
            }

            return workUnits;

        }

        public IReadOnlyList<IWorkUnit> Delete(Project project, IEnumerable<IProjectFile> files)
        {
            if (project is null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (files is null)
            {
                throw new ArgumentNullException(nameof(files));
            }

            var textProvider = TextProviderService.GetTextProvider(project.FilePath);

            if (textProvider == null)
            {
                log?.Warning($"Cannot remove image asset entries from {project.Name} as the MSBuild contents could not be loaded.");
                return null;
            }

            var syntaxTree = XmlSyntaxParser.ParseText(textProvider.GetText());

            if (syntaxTree == null)
            {
                log?.Warning($"Cannot remove image asset entries from from {project.Name} as the MSBuild contents could not be parsed.");
                return null;
            }

            var workUnits = new List<IWorkUnit>();
            var elements = new List<XmlNode>();

            foreach (var file in files)
            {
                var entry = ProjectFileEntryFinder.FindProjectFile(syntaxTree, file);

                if (entry != null)
                {
                    elements.Add(entry);
                }

                workUnits.Add(new DeleteFileWorkUnit()
                {
                    FilePath = file.FilePath,
                });
            }

            workUnits.Add(new DeleteXmlSyntaxWorkUnit()
            {
                FilePath = project.FilePath,
                Syntaxes = elements,
            });

            return workUnits;
        }
    }
}