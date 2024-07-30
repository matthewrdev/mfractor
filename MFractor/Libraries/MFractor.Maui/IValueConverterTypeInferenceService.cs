using System;
using MFractor.Maui.Configuration;
using MFractor.Maui.XamlPlatforms;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui
{
    public interface IValueConverterTypeInferenceService
    {
        ValueConverterTypeInferance InferTypes(string valueConverterName, IXamlPlatform platform, IValueConverterTypeInfermentConfiguration typeInfermentConfiguration, Compilation compilation = null);

        ValueConverterTypeInferance InferTypes(string valueConverterName, string defaultColorType, string defaultImageType, string defaultTupe, Compilation compilation = null);
    }
}
