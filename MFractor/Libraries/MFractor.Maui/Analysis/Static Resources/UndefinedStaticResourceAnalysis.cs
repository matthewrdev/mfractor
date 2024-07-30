using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Data;
using MFractor.Maui.Semantics;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Workspace.Data;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.StaticResources
{
    class UndefinedStaticResourceAnalysis : ExpressionTreeAnalysisRoutine<StaticResourceExpression>
    {
        readonly IResourcesDatabaseEngine resourcesDatabaseEngine;

        public override string Documentation => "Validates that the element referenced by a `StaticResource` expression resource lookup resolves to a resource defined in the xaml file.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.undefined_static_resource";

        public override string Name => "Undefined Static Resource Usage";

        public override string DiagnosticId => "MF1058";

        [ImportingConstructor]
        public UndefinedStaticResourceAnalysis(IResourcesDatabaseEngine resourcesDatabaseEngine)
        {
            this.resourcesDatabaseEngine = resourcesDatabaseEngine;
        }

        public override IReadOnlyList<ICodeIssue> AnalyseExpression(StaticResourceExpression expression, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (expression.Value == null || !expression.Value.HasValue)
            {
                return Array.Empty<ICodeIssue>();
            }

            var database = resourcesDatabaseEngine.GetProjectResourcesDatabase(context.Project);

            if (database == null || !database.IsValid)
            {
                return Array.Empty<ICodeIssue>();
            }

            if (!TryGetPreprocessor<StaticResourceAnalysisPreprocessor>(context, out var preprocessor))
            {
                return Array.Empty<ICodeIssue>();
            }

            var resourceName = expression.Value.Value;

            var resources = preprocessor.FindNamedStaticResources(resourceName);

            if (resources != null && resources.Any())
            {
                return Array.Empty<ICodeIssue>();
            }

            // Walk up the syntax tree and find any nested static resources

            var exists = DoesResourceExistInSyntaxTree(expression.Value.Value, expression.ParentAttribute.Parent, context.XamlSemanticModel, context.Platform);

            if (exists)
            {
                return Array.Empty<ICodeIssue>();
            }

            var message = $" A resource named '{expression.Value.Value}' is not defined in this document, the application resources or any referenced resource dictionaries.";

            var bestSuggestion = SuggestionHelper.FindBestSuggestion(expression.Value.Value, preprocessor.AllAvailableStaticResourceNames);

            if (!string.IsNullOrEmpty(bestSuggestion))
            {
                message += "\n\nDid you mean " + bestSuggestion + "?";
            }

            var bundle = new UndefinedStaticResourceBundle(expression, default, bestSuggestion);

            return CreateIssue(message, expression.ParentAttribute, expression.Value.Span, bundle).AsList();
        }

        bool DoesResourceExistInSyntaxTree(string resourceName, XmlNode node, IXamlSemanticModel semanticModel, IXamlPlatform platform)
        {
            if (node == null)
            {
                return false;
            }

            var resourcesSetter = node.GetChildNode(n =>
            {
                var property = semanticModel.GetSymbol(n) as IPropertySymbol;

                if (property == null)
                {
                    return false;
                }

                if (property.Name != "Resources")
                {
                    return false;
                }

                if (!SymbolHelper.DerivesFrom(property.Type, platform.ResourceDictionary.MetaType))
                {
                    return false;
                }

                return true;
            });

            if (resourcesSetter == null)
            {
                return DoesResourceExistInSyntaxTree(resourceName, node.Parent, semanticModel, platform);
            }

            var resourceDictionary = resourcesSetter.GetChildNode(n =>
            {
                var type = semanticModel.GetSymbol(n) as INamedTypeSymbol;

                return SymbolHelper.DerivesFrom(type, platform.ResourceDictionary.MetaType);
            });

            var exists = false;
            if (resourceDictionary != null)
            {
                exists = HasResourceNamed(resourceName, resourceDictionary);
            }
            else
            {
                exists = HasResourceNamed(resourceName, resourcesSetter);
            }

            if (!exists)
            {
                return DoesResourceExistInSyntaxTree(resourceName, node.Parent, semanticModel, platform);
            }

            return true;
        }

        bool HasResourceNamed(string resourceName, XmlNode resourcesNode)
        {
            if (resourcesNode == null)
            {
                return false;
            }

            var match = resourcesNode.GetChildNode(n =>
            {
                return n.HasAttribute(a => a.Name.FullName == "x:Key" && a.Value?.Value == resourceName);
            });

            return match != null;
        }

        public override bool ShouldInspectExpressionChildren(Expression expression, int expressionDepth)
        {
            if (expression is StaticBindingExpression
                || expression is ReferenceExpression)
            {
                return false;
            }

            return true;
        }
    }
}