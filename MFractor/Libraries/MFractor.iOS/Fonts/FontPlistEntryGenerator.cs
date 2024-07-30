using System;
using MFractor.iOS.Fonts;
using MFractor;
using MFractor.Code.CodeGeneration;
using MFractor.Work.WorkUnits;
using System.IO;
using Microsoft.CodeAnalysis;
using System.ComponentModel.Composition;
using MFractor.Xml;
using MFractor.Work;

namespace MFractor.iOS.Fonts
{
    /// <summary>
    /// The base class for all MFractor code generators.
    /// </summary>
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IFontPlistEntryGenerator))]
    class FontPlistEntryGenerator : CodeGenerator, IFontPlistEntryGenerator
    {
        [ImportingConstructor]
        public FontPlistEntryGenerator(Lazy<IXmlSyntaxParser> xmlSyntaxParser)
        {
            this.xmlSyntaxParser = xmlSyntaxParser;
        }

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        public override string[] Languages { get; } = new string[] { "plist" };

        public override string Identifier => "com.mfractor.code_gen.plist.font_entry";

        public override string Name => "Font PList Entry Generation";

        public override string Documentation => "Generate a new plist entry for a font file entry";

        public string GenerateCode(string fontAssetName, FontPlistEntryKind plistEntryKind)
        {
            switch (plistEntryKind)
            {
                case FontPlistEntryKind.Declaration:
                    return  $"    <string>{fontAssetName}</string>\n";
                case FontPlistEntryKind.ArrayDeclaration:
                    return "\n  <array>\n" +
                            $"    <string>{fontAssetName}</string>\n" +
                             "  </array>\n";
                case FontPlistEntryKind.UIAppFontsDeclaration:
                    return "\n  <key>UIAppFonts</key>\n" +
                             "  <array>\n" +
                            $"    <string>{fontAssetName}</string>\n" +
                             "  </array>\n";
            }

            return string.Empty;
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


            var mode = FontPlistEntryKind.UIAppFontsDeclaration;

            var exists = false;

            if (uiappFonts != null)
            {
                var index = dict.Children.IndexOf(uiappFonts);
                var arrayNode = dict.GetChildAt(index + 1);

                if (arrayNode != null && arrayNode.Name.FullName == "array")
                {
                    var clash = arrayNode.GetChildNode(c => c.Name.FullName == "string" && c.Value == fontAssetName);

                    if (clash == null)
                    {
                        mode = FontPlistEntryKind.Declaration;
                        insertionLocation = arrayNode.ClosingTagSpan.Start;
                    }
                    else
                    {
                        exists = true;
                    }
                }
                else
                {
                    insertionLocation = uiappFonts.NameSpan.End;
                }
            }

            if (!exists)
            {
                return null;
            }

            var insertion = GenerateCode(fontAssetName, mode);

            return new InsertTextWorkUnit(insertion, insertionLocation, plist.FilePath);
        }
    }
}