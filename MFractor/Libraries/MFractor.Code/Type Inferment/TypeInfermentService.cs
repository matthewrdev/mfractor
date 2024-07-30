using System;
using System.ComponentModel.Composition;
using MFractor.Utilities.SymbolVisitors;
using Microsoft.CodeAnalysis;

namespace MFractor.Code.TypeInferment
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ITypeInfermentService))]
    class TypeInfermentService : ITypeInfermentService
    {
        public string InferTypeFromNameAndValue(string name,
                                                string value,
                                                string colorType,
                                                string imageType,
                                                string defaultType,
                                                Compilation compilation = null)
        {
            value ??= string.Empty;
            name ??= string.Empty;

            colorType ??= "System.Drawing.Color";
            defaultType ??= "object";

            if (IsBoolean(name, value))
            {
                return "bool";
            }
            else if (IsStringName(name))
            {
                return "string";
            }
            else if (IsInteger(name, value))
            {
                return "int";
            }
            else if (double.TryParse(value, out _)
                    || name.EndsWith("decimal", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("double", StringComparison.OrdinalIgnoreCase))
            {
                return "double";
            }
            else if (IsFloat(name, value))
            {
                return "float";
            }
            else if (char.TryParse(value, out _)
                     || name.StartsWith("char", StringComparison.OrdinalIgnoreCase))
            {
                return "char";
            }
            else if (name.StartsWith("enum", StringComparison.OrdinalIgnoreCase))
            {
                return "System.Enum";
            }
            else if (IsImage(name, value))
            {
                return imageType;
            }
            else if (name.EndsWith("callback", StringComparison.OrdinalIgnoreCase)
                     || name.EndsWith("handler", StringComparison.OrdinalIgnoreCase)
                     || name.EndsWith("event", StringComparison.OrdinalIgnoreCase))
            {
                return "System.EventHandler";
            }
            else if (name.EndsWith("file", StringComparison.OrdinalIgnoreCase))
            {
                return "System.IO.FileInfo";
            }
            else if (name.EndsWith("command", StringComparison.OrdinalIgnoreCase))
            {
                return "System.Windows.Input.ICommand";
            }
            else if (IsDateTime(name, value))
            {
                return "System.DateTime";
            }
            else if (name.EndsWith("timespan", StringComparison.OrdinalIgnoreCase))
            {
                return "System.TimeSpan";
            }
            else if (name.EndsWith("type", StringComparison.OrdinalIgnoreCase))
            {
                return "System.Type";
            }

            if (IsStringValue(value))
            {
                return "string";
            }

            if (IsColor(name, value))
            {
                return colorType;
            }

            if (compilation != null
                && IsCollection(name))
            {
                var typeName = GetCollectionTypeName(name);

                var visitor = new FindNamedTypeVisitor(typeName);
                visitor.VisitAssembly(compilation.Assembly);

                if (visitor.MatchedSymbol != null)
                {
                    return $"System.Collections.Generic.List<{visitor.MatchedSymbol}>";
                }
            }

            return defaultType;
        }

        bool IsCollection(string name)
        {
            return !string.IsNullOrEmpty(GetCollectionTypeName(name));
        }

        string GetCollectionTypeName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            if (!name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return name.Substring(0, name.Length - 1);
        }

        bool IsStringName(string name)
        {
            return name.EndsWith("text", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("label", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("string", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("title", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("message", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("name", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("uri", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("url", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("html", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("glyph", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("character", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsDateTime(string name, string value)
        {
            if (name.EndsWith("date", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("time", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("date", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("time", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return DateTime.TryParse(value, out _);
        }

        public bool IsColor(string name, string value)
        {
            if (name.EndsWith("color", StringComparison.OrdinalIgnoreCase)
                       || name.EndsWith("colour", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            try
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var color = System.Drawing.ColorTranslator.FromHtml(value);
                    return true;
                }
            }
            catch { }

            return false;
        }

        public bool IsStringValue(string value)
        {
            return value.Contains(" ")
                || value.Contains("\n")
                || value.Contains("\r\n")
                || value.Contains("\t");
        }


        /// <summary>
        /// Inspects the given <paramref name="name"/> and <paramref name="value"/> and infers if it's an image.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsImage(string name, string value)
        {
            return value.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                || value.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                || value.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                || value.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("image", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("icon", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("picture", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith("photo", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Inspects the given <paramref name="name"/> and <paramref name="value"/> and infers if it's an integer.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsInteger(string name, string value)
        {
            if (int.TryParse(value, out _))
            {
                return true;
            }

            return name.EndsWith("number", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("id", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("int", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("integer", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("count", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("size", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("width", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("height", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("spacing", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("length", StringComparison.OrdinalIgnoreCase);
        }
        /// <summary>
        /// Inspects the given <paramref name="name"/> and <paramref name="value"/> and infers if it's a float.
        /// </summary>
        public bool IsFloat(string name, string value)
        {
            if (float.TryParse(value, out _))
            {
                return true;
            }

            return name.EndsWith("radius", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("diameter", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Inspects the given <paramref name="name"/> and <paramref name="value"/> and infers if it's a boolean.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsBoolean(string name, string value)
        {
            if (bool.TryParse(value, out _))
            {
                return true;
            }

            return name.EndsWith("boolean", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("bool", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("enabled", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith("flag", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("is", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("did", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("will", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("can", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("has", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("should", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("allow", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("show", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("include", StringComparison.OrdinalIgnoreCase)
                   || name.StartsWith("accept", StringComparison.OrdinalIgnoreCase);
        }
    }
}
