using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Maui.Configuration;
using MFractor.Code.TypeInferment;
using MFractor.Utilities;
using Microsoft.CodeAnalysis;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Maui
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IValueConverterTypeInferenceService))]
    class ValueConverterTypeInferenceService : IValueConverterTypeInferenceService
    {
        enum ConversionDirection
        {
            LeftToRight,
            RightToLeft,
        }

        const string converterSuffix = "converter";

        readonly string[] leftToRightPrepositions = { "to"};

        readonly string[] rightToLeftPrepositions = { "from" };

        readonly string[] booleanPhrases = { "is", "has", "can", "did" };

        readonly Lazy<ITypeInfermentService> typeInfermentService;
        public ITypeInfermentService TypeInfermentService => typeInfermentService.Value;

        [ImportingConstructor]
        public ValueConverterTypeInferenceService(Lazy<ITypeInfermentService> typeInfermentService)
        {
            this.typeInfermentService = typeInfermentService;
        }

        public ValueConverterTypeInferance InferTypes(string valueConverterName, IXamlPlatform platform, IValueConverterTypeInfermentConfiguration typeInfermentConfiguration, Compilation compilation = null)
        {
            if (typeInfermentConfiguration == null)
            {
                throw new ArgumentNullException(nameof(typeInfermentConfiguration));
            }

            return InferTypes(valueConverterName, platform.Color.MetaType, platform.ImageSource.MetaType, typeInfermentConfiguration.DefaultType, compilation);
        }

        public ValueConverterTypeInferance InferTypes(string valueConverterName, string defaultColorType, string defaultImageType, string defaultType, Compilation compilation = null)
        {
            if (string.IsNullOrEmpty(valueConverterName))
            {
                return ValueConverterTypeInferance.Default;
            }

            IReadOnlyList<string> elements = valueConverterName.SeparateUpperLettersBySpace()
                                          .Split(' ')
                                          .Select(s => s.Trim())
                                          .Where(s => !string.IsNullOrEmpty(s))
                                          .ToList();

            if (!elements.Any())
            {
                return ValueConverterTypeInferance.Default;
            }

            elements = RemoveConverterSuffix(elements);

            var direction = GetConversionDirection(elements);

            if (direction == ConversionDirection.RightToLeft)
            {
                elements.Reverse();
            }

            elements = RemoveConversionDirectionPrepositions(elements);

            ResolveTypeFlow(elements, defaultColorType, defaultImageType, defaultType, compilation, out var inputType, out var outputType);

            return new ValueConverterTypeInferance(true, inputType, outputType);
        }

        void ResolveTypeFlow(IReadOnlyList<string> elements, string defaultColorType, string defaultImageType, string defaultType, Compilation compilation, out string inputType, out string outputType)
        {
            inputType = defaultType;
            outputType = defaultType;

            foreach (var element in elements)
            {
                var resolvedType = TypeInfermentService.InferTypeFromNameAndValue(element, null, defaultColorType, defaultImageType, defaultType, compilation);

                if (!string.IsNullOrEmpty(resolvedType) && resolvedType != defaultType)
                {
                    if (inputType == defaultType)
                    {
                        inputType = resolvedType;
                    }
                    else if (outputType == defaultType)
                    {
                        outputType = resolvedType;
                        break;
                    }
                }
            }
        }

        IReadOnlyList<string> RemoveConversionDirectionPrepositions(IReadOnlyList<string> elements)
        {
            var result = new List<string>();
            foreach (var element in elements)
            {
                if (leftToRightPrepositions.Any(preposition => preposition.Equals(element, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (rightToLeftPrepositions.Any(preposition => preposition.Equals(element, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                result.Add(element);
            }

            return result;
        }

        IReadOnlyList<string> RemoveConverterSuffix(IReadOnlyList<string> elements)
        {
            if (converterSuffix.StartsWith(elements.Last(), StringComparison.OrdinalIgnoreCase))
            {
                return elements.Take(elements.Count - 1).ToList();
            }

            return elements;
        }

        ConversionDirection GetConversionDirection(IEnumerable<string> elements)
        {
            if (booleanPhrases.Any(phrase => phrase.Equals(elements.First(), StringComparison.OrdinalIgnoreCase)))
            {
                return ConversionDirection.LeftToRight;
            }

            foreach (var element in elements)
            {
                if (leftToRightPrepositions.Any(preposition => preposition.Equals(element, StringComparison.OrdinalIgnoreCase)))
                {
                    return ConversionDirection.LeftToRight;
                }

                if (rightToLeftPrepositions.Any(preposition => preposition.Equals(element, StringComparison.OrdinalIgnoreCase)))
                {
                    return ConversionDirection.RightToLeft;
                }
            }

            return ConversionDirection.LeftToRight;
        }
    }
}