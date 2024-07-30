using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Utilities
{
    public static class DesignTimeBindingContextHelper
    {
        public static string DesignTimeBindingContextAttributeName => "DesignTimeBindingContextAttribute";

        public static INamedTypeSymbol GetTargettedDesignTimeBindingContext(Compilation compilation, INamedTypeSymbol namedType)
        {
            if (compilation == null || namedType == null)
            {
                return default;
            }

            var attributes = namedType.GetAttributes();

            var bindingContextAttribute = attributes.FirstOrDefault(a => a.AttributeClass.Name == DesignTimeBindingContextAttributeName);

            if (bindingContextAttribute == null)
            {
                return null;
            }

            // Resolve the input.
            if (bindingContextAttribute.ConstructorArguments.Length != 1)
            {
                return null;
            }

            var argument = bindingContextAttribute.ConstructorArguments[0];

            if (argument.Type.SpecialType == SpecialType.System_String)
            {
                return compilation.GetTypeByMetadataName(argument.Value as string);
            }

            return argument.Value as INamedTypeSymbol;
        }
    }
}
