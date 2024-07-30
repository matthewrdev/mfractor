using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using MFractor.Text;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.Importing
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IResXLocalisationImporter))]
    sealed class ResXLocalisationImporter : IResXLocalisationImporter
    {
        readonly Lazy<IProjectService> projectService;
        IProjectService ProjectService => projectService.Value;

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        public LocalisationImportSource[] SupportedImportSources { get; } = new LocalisationImportSource[]
        {
            LocalisationImportSource.Project,
            LocalisationImportSource.Directory,
            LocalisationImportSource.File,
        };

        public string DisplayName => "ResX";

        [ImportingConstructor]
        public ResXLocalisationImporter(Lazy<IProjectService> projectService,
                                        Lazy<IXmlSyntaxParser> xmlSyntaxParser)
        {
            this.projectService = projectService;
            this.xmlSyntaxParser = xmlSyntaxParser;
        }

        public ILocalisationFile GetDefaultFile(IEnumerable<ILocalisationFile> choices, string preference)
        {
            if (!string.IsNullOrEmpty(preference))
            {
                var choice = choices.FirstOrDefault(f => f.DisplayPath == preference);

                if (choice != null)
                {
                    return choice;
                }
            }

            // Locate the default resx file
            return choices.FirstOrDefault(f => Path.GetFileName(f.FullPath).Split('.').Length == 2);
        }

        public bool CanProvide(Project project, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (project == null)
            {
                errorMessage = "No project was provided";
                return false;
            }

            return true;
        }

        public bool CanProvide(DirectoryInfo directory, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (directory == null)
            {
                errorMessage = "No directory was provided to import";
                return false;
            }

            return true;
        }

        public bool CanProvide(FileInfo file, out string errorMessage)
        {
            errorMessage = String.Empty;
            if (file == null)
            {
                errorMessage = "No file was provided to import";
                return false;
            }

            if (!file.Name.EndsWith(".resx", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = file.Name + " is not a .resx file.";
                return false;
            }

            return true;
        }

        public IEnumerable<ILocalisationFile> RetrieveLocalisationFiles(DirectoryInfo directory)
        {
            if (directory == null || !directory.Exists)
            {
                return Enumerable.Empty<ILocalisationFile>();
            }

            var resxFiles = directory.GetFiles("*.resx");

            if (resxFiles == null || !resxFiles.Any())
            {
                return Enumerable.Empty<ILocalisationFile>();
            }

            return resxFiles.Select(fi => new LocalisationFile(fi.Name, fi.FullName)).ToList();
        }

        public IEnumerable<ILocalisationFile> RetrieveLocalisationFiles(Project project)
        {
            if (project == null)
            {
                return Enumerable.Empty<ILocalisationFile>();
            }

            var files = ProjectService.GetProjectFiles(project);

            return files.Where(projectFile => Path.GetExtension(projectFile.FilePath) == ".resx")
                        .Select(projectFile =>
                        {

                            var displayPath = Path.GetFileName(projectFile.FilePath);

                            if (projectFile.ProjectFolders.Any())
                            {
                                displayPath = string.Join("/", projectFile.ProjectFolders) + "/" + displayPath;
                            }

                            return new LocalisationFile(displayPath, projectFile.FilePath);
                        })
                        .ToList();
        }


        public IReadOnlyList<ILocalisationValue> ProvideLocalisationValues(ITextProvider textProvider, CultureInfo culture)
        {
            if (textProvider is null)
            {
                throw new ArgumentNullException(nameof(textProvider));
            }

            return ProvideLocalisationValues(textProvider.GetText(), culture);
        }

        public IReadOnlyList<ILocalisationValue> ProvideLocalisationValues(string content, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(content))
            {
                return default;
            }

            if (culture is null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var syntax = XmlSyntaxParser.ParseText(content);

            var values = new List<ILocalisationValue>();

            if (syntax.Root != null && syntax.Root.HasChildren)
            {
                var dataNodes = syntax.Root.GetChildren(n => n.Name.LocalName == "data");

                if (dataNodes != null && dataNodes.Any())
                {
                    foreach (var dataNode in dataNodes)
                    {
                        var name = dataNode.GetAttribute(a => a.Name.LocalName == "name");
                        var value = dataNode.GetChildNode(a => a.Name.LocalName == "value");
                        var comment = dataNode.GetChildNode(a => a.Name.LocalName == "comment");

                        if (name != null
                            && value != null
                            && name.HasValue
                            && value.HasValue)
                        {

                            var entry = new LocalisationValue(name.Value.Value,
                                                              name.Value.Span,
                                                              value.Value,
                                                              value.ValueSpan,
                                                              comment?.Value,
                                                              comment?.ValueSpan,
                                                              culture);

                            values.Add(entry);
                        }
                    }
                }
            }

            return values;
        }

        public IReadOnlyList<ILocalisationValue> ProvideLocalisationValues(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!file.Exists)
            {
                return default;
            }

            var content = File.ReadAllText(file.FullName);
            var culture = GetCultureInfo(file);

            return ProvideLocalisationValues(content, culture);
        }

        public CultureInfo GetCultureInfo(string filePath)
        {
            return GetCultureInfo(new FileInfo(filePath));
        }

        public CultureInfo GetCultureInfo(FileInfo file)
        {
            var culture = CultureInfo.GetCultureInfoByIetfLanguageTag("en");

            var components = file.Name.Split('.');

            if (components.Length > 2)
            {
                var cultureCode = components[components.Length - 2];
                try
                {
                    culture = CultureInfo.GetCultureInfoByIetfLanguageTag(cultureCode);
                }
                catch
                { }
            }

            return culture;
        }

        public IReadOnlyDictionary<ILocalisationFile, IReadOnlyList<ILocalisationValue>> ProvideLocalisationValues(Project project)
        {
            var files = RetrieveLocalisationFiles(project);

            return ProvideLocalisationValues(files);
        }

        public IReadOnlyDictionary<ILocalisationFile, IReadOnlyList<ILocalisationValue>> ProvideLocalisationValues(DirectoryInfo directory)
        {
            var files = RetrieveLocalisationFiles(directory);

            return ProvideLocalisationValues(files);
        }

        public IReadOnlyDictionary<ILocalisationFile, IReadOnlyList<ILocalisationValue>> ProvideLocalisationValues(IEnumerable<ILocalisationFile> files)
        {
            var result = new Dictionary<ILocalisationFile, IReadOnlyList<ILocalisationValue>>();

            foreach (var file in files)
            {
                var values = ProvideLocalisationValues(new FileInfo(file.FullPath));

                if (values != null && values.Any())
                {
                    result[file] = values;
                }
            }

            return result;
        }
    }
}
