using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Utilities;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.CodeGeneration.ResourceDictionaries
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IImportResourceDictionaryGenerator))]
    class ImportResourceDictionaryGenerator : XamlCodeGenerator, IImportResourceDictionaryGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.xaml.import_resource_dictionary";

        public override string Name => "Import Resource Dictionary";

        public override string Documentation => "Generates a resource dictionary import.";

        public IReadOnlyList<IWorkUnit> ImportResourceDictionary(ParsedXamlDocument parsedXamlDocument, INamedTypeSymbol resourceDictionarySymbol)
        {
            if (parsedXamlDocument == null)
            {
                throw new ArgumentNullException(nameof(parsedXamlDocument));
            }

            if (resourceDictionarySymbol == null)
            {
                throw new ArgumentNullException(nameof(resourceDictionarySymbol));
            }

            return ImportResourceDictionary(parsedXamlDocument, resourceDictionarySymbol.ToString());
        }

        public IReadOnlyList<IWorkUnit> ImportResourceDictionary(ParsedXamlDocument parsedXamlDocument, string resourceDictionaryType)
        {
            if (parsedXamlDocument == null)
            {
                throw new ArgumentNullException(nameof(parsedXamlDocument));
            }

            if (string.IsNullOrEmpty(resourceDictionaryType))
            {
                throw new ArgumentException("message", nameof(resourceDictionaryType));
            }

            if (!SymbolHelper.ExplodeTypeName(resourceDictionaryType, out var @namespace, out var typeName))
            {
                return Array.Empty<IWorkUnit>();
            }

            // TODO: Does it have a resources dictionary node?
            // TODO: Does it have a resources dictionary node?
            // TODO: Does it have a ResourceDictionary.MergedDictionaries node?


            return null;
        }
    }
}