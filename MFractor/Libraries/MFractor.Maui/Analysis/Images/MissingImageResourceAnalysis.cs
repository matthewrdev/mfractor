using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Images;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.Images
{
    class DetectMissingImageResource : XamlCodeAnalyser
    {
        public override IssueClassification Classification => IssueClassification.Error;

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string Identifier => "com.mfractor.code.analysis.xaml.missing_image_resource";

        public override string Name => "Detect Missing Image In Linked Projects";

        public override string Documentation => "This code analyser inspects the value provided into an `ImageSource` and validates that an image of that name exists within any iOS or Android projects that reference this shared project or PCL.";

        public override string DiagnosticId => "MF1035";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax,
                                                          IParsedXamlDocument document,
                                                          IXamlFeatureContext context)
        {
            if (!TryGetPreprocessor<ImageResourceAnalysisPreprocessor>(context, out var preprocessor))
            {
                return null;
            }

            var expression = context.XamlSemanticModel.GetExpression(syntax);
            if (expression != null)
            {
                return null;
            }

            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;

            if (symbol == null || !syntax.HasValue)
            {
                return null;
            }

            var imageType = context.Compilation.GetTypeByMetadataName(context.Platform.ImageSource.MetaType);

            if (imageType == null)
            {
                return null;
            }

            if (!SymbolHelper.DerivesFrom(symbol.Type, imageType))
            {
                return null;
            }

            var imageName = syntax.Value.Value;

            var projects = context.Project.GetDependentMobileProjects().ToList();

            if (!projects.Any())
            {
                return null;
            }

            var message = "";

            var imageAsset = preprocessor.GetImageAssetByName(imageName);

            var missingProjects = new List<Project>();
            if (imageAsset == null)
            {
                missingProjects.AddRange(projects);
            }
            else
            {
                missingProjects = projects.Except(imageAsset.Projects).ToList();
            }

            if (!missingProjects.Any())
            {
                return null;
            }

            if (missingProjects.Count == 1)
            {
                message = $"The image '{syntax.Value}' does not exist in {missingProjects.FirstOrDefault().Name}";
            }
            else
            {
                message = $"The image '{syntax.Value}' does not exist in the following projects:\n";

                message += string.Join("\n", missingProjects.Select(ap => " - " + ap.Name));
            }

            var suggestion = SuggestionHelper.FindBestSuggestion(imageName, preprocessor.ImageAssets.Keys);

            if (!string.IsNullOrEmpty(suggestion) && imageAsset == null)
            {
                message += "\n\nDid you mean " + suggestion + "?";
            }

            var missingImageBundle = new MissingImageResourceBundle(missingProjects, projects, suggestion);

            return CreateIssue(message, syntax, syntax.Value.Span, missingImageBundle).AsList();
        }
    }
}
