using System;
using MFractor.Code;
using MFractor.Code.Analysis;
using MFractor.Code.Documents;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    /// <summary>
    /// Instances of <see cref="CodeAnalyserExecutionFilter"/> for XAML files.
    /// </summary>
    public static class XamlCodeAnalysisExecutionFilters
    {
        /// <summary>
        /// A <see cref="CodeAnalyserExecutionFilter"/> for filtering/detecting XAML files.
        /// </summary>
        public static readonly CodeAnalyserExecutionFilter XamlDocumentFilter = new CodeAnalyserExecutionFilter("XAML");

        public static readonly CodeAnalyserExecutionFilter ExpressionExecutionFilter = new CodeAnalyserExecutionFilter("XAML - Expression", IsExpression);

        public static readonly CodeAnalyserExecutionFilter PropertySetterNodeExecutionFilter = new CodeAnalyserExecutionFilter("XAML - Property Setter Node", IsPropertySetterNode);

        public static readonly CodeAnalyserExecutionFilter EventHandlerExecutionFilter = new CodeAnalyserExecutionFilter("XAML - Event Handler", IsEventHandler);

        public static readonly CodeAnalyserExecutionFilter GridContainerExecutionFilter = new CodeAnalyserExecutionFilter("XAML - Grid Contained", IsGridContained);

        public static readonly CodeAnalyserExecutionFilter ThicknessExecutionFilter = new CodeAnalyserExecutionFilter("XAML - Thickness", IsThickness);

        public static readonly CodeAnalyserExecutionFilter ColorExecutionFilter = new CodeAnalyserExecutionFilter("XAML - Color", IsColor);

        static bool IsThickness(IFeatureContext featureContext, XmlSyntax syntax)
        {
            var xamlFeatureContext = featureContext as IXamlFeatureContext;
            if (xamlFeatureContext is null)
            {
                return false;
            }

            var attribute = syntax as XmlAttribute;
            if (attribute is null)
            {
                return false;
            }

            var property = xamlFeatureContext.XamlSemanticModel.GetSymbol(attribute) as IPropertySymbol;
            if (property is null)
            {
                return false;
            }

            return SymbolHelper.DerivesFrom(property.Type, xamlFeatureContext.Platform.Thickness.MetaType);
        }

        static bool IsColor(IFeatureContext featureContext, XmlSyntax syntax)
        {
            var xamlFeatureContext = featureContext as IXamlFeatureContext;
            if (xamlFeatureContext is null)
            {
                return false;
            }

            var attribute = syntax as XmlAttribute;
            if (attribute is null)
            {
                return false;
            }

            var property = xamlFeatureContext.XamlSemanticModel.GetSymbol(attribute) as IPropertySymbol;
            if (property is null)
            {
                return false;
            }

            return FormsSymbolHelper.IsColor(property.Type, xamlFeatureContext.Platform);
        }

        static bool IsGridContained(IFeatureContext featureContext, XmlSyntax syntax)
        {
            var xamlFeatureContext = featureContext as IXamlFeatureContext;
            if (xamlFeatureContext is null)
            {
                return false;
            }

            var attribute = syntax as XmlAttribute;
            if (attribute is null)
            {
                return false;
            }

            var gridContainer = attribute?.Parent?.Parent;

            if (gridContainer == null)
            {
                return false;
            }

            var gridType = xamlFeatureContext.Compilation.GetTypeByMetadataName(xamlFeatureContext.Platform.Grid.MetaType);

            var gridSymbol = xamlFeatureContext.XamlSemanticModel.GetSymbol(gridContainer) as INamedTypeSymbol;
            if (gridSymbol == null || !SymbolHelper.DerivesFrom(gridSymbol, gridType))
            {
                return false;
            }

            return true;
        }


        static bool IsEventHandler(IFeatureContext featureContext, XmlSyntax syntax)
        {
            var xamlFeatureContext = featureContext as IXamlFeatureContext;
            if (xamlFeatureContext is null)
            {
                return false;
            }

            var attribute = syntax as XmlAttribute;
            if (attribute is null)
            {
                return false;
            }

            return xamlFeatureContext.XamlSemanticModel.GetSymbol(attribute) is IEventSymbol;
        }

         static bool IsPropertySetterNode(IFeatureContext featureContext, XmlSyntax syntax)
        {
            var xamlFeatureContext = featureContext as IXamlFeatureContext;
            if (xamlFeatureContext is null)
            {
                return false;
            }

            var node = syntax as XmlNode;
            if (node is null)
            {
                return false;
            }

            return XamlSyntaxHelper.IsPropertySetter(node);
        }

        static bool IsExpression(IFeatureContext featureContext, XmlSyntax syntax)
        {
            var xamlFeatureContext = featureContext as IXamlFeatureContext;
            if (xamlFeatureContext is null)
            {
                return false;
            }

            var attribute = syntax as XmlAttribute;
            if (attribute is null)
            {
                return false;
            }

            return xamlFeatureContext.XamlSemanticModel.GetExpression(attribute) != null;
        }
    }
}
