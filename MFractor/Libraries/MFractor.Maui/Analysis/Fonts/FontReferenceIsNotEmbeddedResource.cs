using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Analysis.Preprocessors;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Xml;

namespace MFractor.Maui.Analysis.Fonts
{
    class FontReferenceIsNotEmbeddedResource : XamlCodeAnalyser
    {
        readonly Lazy<IProjectService> projectService;
        public IProjectService ProjectService => projectService.Value;

        public override string Documentation => "Inspects FontFamily values that resolve to an exported font and verifies that the font asset is an embedded resource.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.font_reference_is_not_embedded_resource";

        public override string Name => "Font Reference Is Not Embedded Resource";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1100";

        [ImportingConstructor]
        public FontReferenceIsNotEmbeddedResource(Lazy<IProjectService> projectService)
        {
            this.projectService = projectService;
        }

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (syntax.Name.LocalName != "FontFamily"
                || !syntax.HasValue)
            {
                return null;
            }

            if (ExpressionParserHelper.IsExpression(syntax.Value.Value))
            {
                return null;
            }

            if (!TryGetPreprocessor<ExportedFontPreprocessor>(context, out var preprocessor))
            {
                return null;
            }

            var fonts = preprocessor.EmbeddedFonts;

            var value = syntax.Value.Value;

            var fontReference = fonts.FirstOrDefault(f =>
            {
                if (f.HasAlias)
                {
                    return f.Alias == value;
                }

                return f.FontName == value;
            });

            if (fontReference is null
                || fontReference.CompilationProject is null
                || fontReference.Font is null)
            {
                return null;
            }

            var fontFile = ProjectService.GetProjectFileWithFilePath(fontReference.CompilationProject, fontReference.Font.FilePath);
            if (fontFile is null)
            {
                return null;
            }

            if (fontFile.BuildAction == "EmbeddedResource")
            {
                return null;
            }

            return CreateIssue($"The exported font's '{value}' build action is not set to EmbeddedResource.", syntax, syntax.Value.Span, fontReference).AsList();
        }
    }
}