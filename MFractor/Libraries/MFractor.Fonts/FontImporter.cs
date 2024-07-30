using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Fonts
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFontImporter))]
    class FontImporter : IFontImporter
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IProjectService> projectService;
        IProjectService ProjectService => projectService.Value;

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        [ImportingConstructor]
        public FontImporter(Lazy<IProjectService> projectService,
                            Lazy<IXmlSyntaxParser> xmlSyntaxParser)
        {
            this.projectService = projectService;
            this.xmlSyntaxParser = xmlSyntaxParser;
        }

        public IReadOnlyList<IWorkUnit> ImportFont(Project project, string fontFilePath, string fontAssetName)
        {
            if (project.IsAndroidProject())
            {
                return ImportFontIntoAndroid(project, fontFilePath, fontAssetName);
            }
            else if (project.IsAppleUnifiedProject())
            {
                return ImportFontIntoIOS(project, fontFilePath, fontAssetName);
            }
            else if (project.IsUWPProject())
            {
                return ImportFontIntoUWP(project, fontFilePath, fontAssetName);
            }

            return Array.Empty<IWorkUnit>();
        }

        public IReadOnlyList<IWorkUnit> ImportFontIntoAndroid(Project project, string fontFilePath, string fontAssetName)
        {
            var copyFileWorkUnit = new CreateProjectFileWorkUnit()
            {
                BuildAction = "AndroidAsset",
                FilePath = "Assets/" + fontAssetName,
                WriteContentAction = (Stream stream) =>
                {
                    using (var fileStream = File.OpenRead(fontFilePath))
                    {
                        fileStream.CopyTo(stream);
                    }
                },
                IsBinary = true,
                TargetProject = project,
                ShouldOverWrite = true,
            };

            return copyFileWorkUnit.AsList();
        }

        public IReadOnlyList<IWorkUnit> ImportFontIntoUWP(Project project, string fontFilePath, string fontAssetName)
        {
            var copyFileWorkUnit = new CreateProjectFileWorkUnit()
            {
                BuildAction = "Content",
                FilePath = "Assets/Fonts/" + fontAssetName,
                WriteContentAction = (Stream stream) =>
                {
                    using (var fileStream = File.OpenRead(fontFilePath))
                    {
                        fileStream.CopyTo(stream);
                    }
                },
                IsBinary = true,
                TargetProject = project,
                ShouldOverWrite = true,
            };
            return copyFileWorkUnit.AsList();
        }

        public IReadOnlyList<IWorkUnit> ImportFontIntoIOS(Project project, string fontFilePath, string fontAssetName)
        {
            var workUnits = new List<IWorkUnit>();

            var copyFileWorkUnit = new CreateProjectFileWorkUnit()
            {
                BuildAction = "BundleResource",
                FilePath = "Resources/" + fontAssetName,
                WriteContentAction = (Stream stream) =>
                {
                    using (var fileStream = File.OpenRead(fontFilePath))
                    {
                        fileStream.CopyTo(stream);
                    }
                },
                IsBinary = true,
                TargetProject = project,
                ShouldOverWrite = true,
            };

            workUnits.Add(copyFileWorkUnit);

            var createPlistEntry = CreateIOSPlistEntry(project, fontAssetName);

            if (createPlistEntry != null)
            {
                workUnits.Add(createPlistEntry);
            }

            return workUnits;
        }

        public IWorkUnit CreateIOSPlistEntry(Project project, string fontAssetName)
        {
            var plist = ProjectService.FindProjectFile(project, (filePath) =>
            {
                return Path.GetFileName(filePath) == "Info.plist";
            });

            if (plist == null)
            {
                return null;
            }

            var syntax = XmlSyntaxParser.ParseFile(plist.FileInfo);
            var root = syntax.Root;

            var dict = root.GetChildNode(child =>
            {
                return child.Name.FullName == "dict";
            });

            if (dict == null)
            {
                return null;
            }

            var insertionLocation = dict.ClosingTagSpan.Start;
            var uiappFonts = dict.GetChildNode(child =>
            {
                if (child.Name.FullName != "key")
                {
                    return false;
                }

                if (!child.HasValue)
                {
                    return false;
                }

                return child.Value == "UIAppFonts";
            });

            var insertion = "";

            if (uiappFonts != null)
            {
                var index = dict.Children.IndexOf(uiappFonts);
                var arrayNode = dict.GetChildAt(index + 1);

                if (arrayNode != null && arrayNode.Name.FullName == "array")
                {
                    var clash = arrayNode.GetChildNode(c => c.Name.FullName == "string" && c.Value == fontAssetName);

                    if (clash == null)
                    {
                        insertionLocation = arrayNode.ClosingTagSpan.Start;
                        insertion = $"    <string>{fontAssetName}</string>\n";
                    }
                    else
                    {
                        log?.Info("Ignoring adding " + fontAssetName + " to info.plist as an entry for it already exists.");
                    }
                }
                else
                {
                    insertionLocation = uiappFonts.NameSpan.End;
                    insertion =
                        "\n  <array>\n" +
                         $"    <string>{fontAssetName}</string>\n" +
                          "  </array>\n";
                }
            }
            else
            {
                insertion = "\n  <key>UIAppFonts</key>\n" +
                              "  <array>\n" +
                             $"    <string>{fontAssetName}</string>\n" +
                              "  </array>\n";
            }


            if (string.IsNullOrEmpty(insertion))
            {
                return null;
            }

            return new InsertTextWorkUnit(insertion, insertionLocation, plist.FilePath);
        }
    }
}