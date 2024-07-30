using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using MFractor.Maui.Syntax.Expressions;
using MFractor.Maui.Utilities;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    class UnknownStaticPropertyValue : XamlCodeAnalyser
    {
        public override string Documentation => "For attributes that accept a class object, inspects that the literal value maps to a static property or field in the class type. For example, the `LayoutOptions` class has the static fields `Fill` or `CentreAndExpand` that can be used a literal attribute values. If `CentreAndFill` was provided (an invalid value), this analyser would inspect the `LayoutOptions` class for a static field named `CentreAndFill` and trigger an analysis error when it couldn't be found.";

        public override IssueClassification Classification => IssueClassification.Error;

        public override string Identifier => "com.mfractor.code.analysis.xaml.unknown_static_property_value";

        public override string Name => "Unknown Static Property Value";

        public override XmlSyntaxKind TargetSyntax => XmlSyntaxKind.Attribute;

        public override string DiagnosticId => "MF1072";

        protected override IReadOnlyList<ICodeIssue> Analyse(XmlAttribute syntax, IParsedXamlDocument document, IXamlFeatureContext context)
        {
            if (!context.Platform.SupportsIndicatorView)
            {
                return null;
            }

            var expression = context.XamlSemanticModel.GetExpression(syntax);
            if (expression != null)
            {
                return null;
            }

            var symbol = context.XamlSemanticModel.GetSymbol(syntax) as IPropertySymbol;
            if (symbol == null)
            {
                return null;
            }

            if (symbol.Type.SpecialType != SpecialType.None)
            {
                return null;
            }

            if (syntax.HasValue == false)
            {
                return null;
            }

            var searchName = syntax.Value.Value;

            if (searchName.Contains('.'))
            {
                var components = searchName.Split('.');
                if (components.Length != 2)
                {
                    return null;
                }

                searchName = components[1];
            }

            var parentElementType = context.XamlSemanticModel.GetSymbol(syntax.Parent) as ITypeSymbol;
            if (parentElementType != null)
            {
                if (SymbolHelper.DerivesFrom(parentElementType, context.Platform.Setter.MetaType)
                    || SymbolHelper.DerivesFrom(parentElementType, context.Platform.Style.MetaType))
                {
                    return null;
                }
            }

            if (SymbolHelper.DerivesFrom(symbol.Type, context.Platform.ImageSource.MetaType)
                || SymbolHelper.DerivesFrom(symbol.Type, context.Platform.Thickness.MetaType)
                || SymbolHelper.DerivesFrom(symbol.Type, context.Platform.GridLength.MetaType))
            {
                return null;
            }

            var staticFields = symbol.Type.GetMembers()
                                          .Where(m => m is IFieldSymbol || m is IPropertySymbol)
                                          .Where(fs => fs.IsStatic)
                                          .ToList();

            if (staticFields == null || staticFields.Count == 0)
            {
                return null;
            }

            var resolvedField = staticFields.FirstOrDefault(sf => sf.Name == searchName);

            if (resolvedField != null)
            {
                return null;
            }

            var nearestMatch = SymbolHelper.ResolveNearestSymbol(searchName, staticFields);
            var typeAttributes = symbol.Type.GetAttributes();

            if (typeAttributes.Any(ad => ad.AttributeClass.ToString() == context.Platform.TypeConverterAttribute.MetaType) && nearestMatch == null)
            {
                return null;
            }

            if (SymbolHelper.DerivesFrom(symbol.Type, "System.Uri")
                || SymbolHelper.DerivesFrom(symbol.Type, "System.Type")
                || SymbolHelper.DerivesFrom(symbol.Type, context.Platform.IndicatorView.MetaType))
            {
                return null;
            }

            var message = $"{syntax.Value} is not a member of {symbol.Type}.";

            if (nearestMatch != null)
            {
                message += $"\n\nDid you mean '{nearestMatch.Name}'?";
            }

            // Find the nearest and pass to fix.
            return CreateIssue(message, syntax, syntax.Value.Span, nearestMatch).AsList();
        }
    }
}

