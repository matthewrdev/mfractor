using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MFractor.IOC;
using MFractor.Maui.Semantics;
using MFractor.Maui.Symbols;
using MFractor.Maui.XamlPlatforms;
using MFractor.Maui.Xmlns;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Utilities
{
    public static class FormsSymbolHelper
    {
        /// <summary>
        /// Is the given <paramref name="typeSymbol"/> a color?
        /// </summary>
        public static bool IsColor(ITypeSymbol typeSymbol, IXamlPlatform platform)
        {
            if (typeSymbol == null)
            {
                return false;
            }

            return SymbolHelper.DerivesFrom(typeSymbol, "System.Drawing.Color") || SymbolHelper.DerivesFrom(typeSymbol, platform.Color.MetaType);
        }

        public static bool HasTypeConverterAttribute(ISymbol symbol, IXamlPlatform platform)
        {
            var attributes = symbol.GetAttributes().ToList();

            if (symbol is IPropertySymbol)
            {
                attributes.AddRange((symbol as IPropertySymbol).Type.GetAttributes().ToList());
            }
            else if (symbol is IFieldSymbol)
            {
                attributes.AddRange((symbol as IFieldSymbol).Type.GetAttributes().ToList());
            }

            if (attributes.Any(ad => ad.AttributeClass.ToString() == platform.TypeConverterAttribute.MetaType))
            {
                return true;
            }

            return false;
        }

        public static INamedTypeSymbol ResolveNearlyNamedTypeFromAssemblies(IEnumerable<IAssemblySymbol> assemblies, string elementName, Compilation compilation)
        {
            if (assemblies is null)
            {
                return null;
            }

            foreach (var assembly in assemblies)
            {
                var symbol = ResolveNearlyNamedTypeInAssembly(assembly, elementName, compilation);

                if (symbol != null)
                {
                    return symbol;
                }
            }

            return null;
        }

        public static INamedTypeSymbol ResolveNearlyNamedTypeInAssembly(IAssemblySymbol assembly, string elementName, Compilation compilation)
        {
            INamedTypeSymbol similiarSymbol = null;

            similiarSymbol = compilation.GetSymbolsWithName((name) =>
            {
                if (name == elementName)
                {
                    return true;
                }

                var distance = LevenshteinDistanceHelper.Compute(name, elementName);

                return distance <= 3;
            }, SymbolFilter.Type).OfType<INamedTypeSymbol>()
                                        .FirstOrDefault(symbol => symbol.ContainingAssembly.Name == assembly.Name);

            return similiarSymbol;
        }

        public static INamedTypeSymbol ResolveNearlyNamedTypeInAssembly(string assemblyName, string elementName, Compilation compilation)
        {
            INamedTypeSymbol similiarSymbol = null;

            similiarSymbol = compilation.GetSymbolsWithName((name) =>
            {
                if (name == elementName)
                {
                    return true;
                }

                var distance = LevenshteinDistanceHelper.Compute(name, elementName);

                return distance <= 3;
            }, SymbolFilter.Type).OfType<INamedTypeSymbol>()
                                        .FirstOrDefault(symbol => symbol.ContainingAssembly.Name == assemblyName);

            return similiarSymbol;
        }

        public static bool IsTypeMismatch(ITypeSymbol expectedType,
                                          ITypeSymbol actualType,
                                          XmlNode parentNode,
                                          IXamlNamespaceCollection namespaces,
                                          IXmlnsDefinitionCollection xmlnsDefinitions, 
                                          Project project,
                                          IXamlSemanticModel semanticModel,
                                          IXamlPlatform platform,
                                          bool allowImplicitToString = true)
        {
        	var expected = expectedType;
        	var nodeSymbol = semanticModel.GetSymbol(parentNode) as INamedTypeSymbol;
        	if (nodeSymbol != null && nodeSymbol.IsGenericType)
        	{
        		// Resolve the type for the generic...
        		if (parentNode.HasAttributes)
        		{
                    var microsoftSchema = namespaces.ResolveNamespaceForSchema(XamlSchemas.MicrosoftSchemaUrl);
        			var typeArgs = parentNode.GetAttributeByName(microsoftSchema.Prefix + ":" + Keywords.MicrosoftSchema.TypeArguments);
        			if (typeArgs != null
                        && typeArgs.HasValue)
        			{
                        var hasNamespace = typeArgs.Value.Value.Contains(':');
                        var components = typeArgs.Value.Value.Split(':');
        				var type = !hasNamespace ? components[0] : "";
                        var xamlNamespace = namespaces.ResolveNamespace(hasNamespace ? components[0] : "");

        				if (xamlNamespace != null)
        				{
                            var typeResolver = Resolver.Resolve<IXamlTypeResolver>();

                            var resolvedType = typeResolver.ResolveType(type, xamlNamespace, project, xmlnsDefinitions);
                            if (resolvedType != null)
        					{
        						expected = resolvedType;
        					}
        				}
        			}
        		}
        	}

        	if (allowImplicitToString)
        	{
        		if (expected.SpecialType == SpecialType.System_String)
        		{
        			return false; // Will always cause an implict to string operation, warning not relevant.
        		}
        	}

            // Check for formatted strings
            if (SymbolHelper.DerivesFrom(expectedType, platform.FormattedString.MetaType))
            {
                if (actualType.SpecialType == SpecialType.System_String
                   || SymbolHelper.DerivesFrom(actualType, platform.FormattedString.MetaType))
                {
                    return false;
                }
            }

            if (SymbolHelper.DerivesFrom(actualType, expected))
        	{
        		return false;
        	}

            if (SymbolHelper.IsNumber(actualType) && SymbolHelper.IsNumber(expected))
            {
                return false;
            }

            if (actualType.TypeKind == TypeKind.Enum && SymbolHelper.IsInteger(expected))
            {
                return false;
            }

            if (expected.IsValueType
                && expected.NullableAnnotation == NullableAnnotation.Annotated
                && expected is INamedTypeSymbol namedTypeSymbol
                && namedTypeSymbol.TypeArguments != null
                && namedTypeSymbol.TypeArguments.Any())
            {
                var innerExpected = namedTypeSymbol.TypeArguments.FirstOrDefault();

                return IsTypeMismatch(innerExpected, actualType, parentNode, namespaces, xmlnsDefinitions, project, semanticModel, platform, allowImplicitToString);
            }

        	return true;
        }

        public static bool ResolveValueConverterConstraints(INamedTypeSymbol valueConverter, out ITypeSymbol converterInput, out ITypeSymbol converterOuput, out ITypeSymbol converterParameter)
        {
        	converterInput = converterOuput = converterParameter = null;

        	var attributes = valueConverter.GetAttributes();
        	if (attributes.Length == 0)
        	{
        		return false;
        	}

            // ValueConversionAttribute is XAML platform agnostic and does not require platform handling. 
            var conversionAttribute = attributes.FirstOrDefault(a => a.AttributeClass.Name == "ValueConversionAttribute");

        	if (conversionAttribute == null)
        	{
        		return false;
        	}

        	// Resolve the input.
        	if (conversionAttribute.ConstructorArguments.Length < 2) // 2 == must have input and output
        	{
        		return false;
        	}

        	converterInput = conversionAttribute.ConstructorArguments[0].Value as INamedTypeSymbol;
        	converterOuput = conversionAttribute.ConstructorArguments[1].Value as INamedTypeSymbol;

        	if (conversionAttribute.NamedArguments.Length > 0)
        	{
        		var arg = conversionAttribute.NamedArguments[0];
        		if (arg.Key == "ParameterType")
        		{
        			converterParameter = arg.Value.Type;
        		}
        	}

        	return true;
        }
    }
}

