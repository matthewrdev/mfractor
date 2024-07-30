using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MFractor.Linker.Helpers
{
    public static class LinkerSignatureHelper
    {
        public static string GetSignature(ISymbol symbol)
        {
            if (symbol is IFieldSymbol field)
            {
                return GetSignatureSymbolName(field.Type) + " " + field.Name;
            }

            if (symbol is IMethodSymbol method)
            {
                var parameters = string.Empty;
                if (method.Parameters.Any())
                {
                    try
                    {
                        parameters = "(" + string.Join(",", method.Parameters.Select(p => GetSignatureSymbolName(p.Type))) + ")";
                    }
                    catch (Exception)
                    { }
                }

                var returnType = GetSignatureSymbolName(method.ReturnType);

                return returnType + " " + method.Name + parameters;
            }

            return string.Empty;
        }

        public static string GetSignatureSymbolName(ITypeSymbol namedType)
        {
            if (namedType is IArrayTypeSymbol array)
            {
                return GetSignatureSymbolName(array.ElementType) + "[]";
            }

            return namedType.ContainingNamespace.ToString() + "." + namedType.Name;
        }
    }
}
