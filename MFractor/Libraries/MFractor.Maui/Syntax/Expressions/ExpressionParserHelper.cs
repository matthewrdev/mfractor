using System;
using System.Collections.Generic;
using MFractor.Utilities;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax.Expressions
{
    /// <summary>
    /// Helper class for parsing XAML expressions.
    /// <para/>
    /// Matt R: I wrote this but I really have no idea how this even works, use with caution.
    /// </summary>
    public static class ExpressionParserHelper
    {
        public static bool IsPropertyAssignmentExpression(string expression)
        {
            // Walk until we find a '=', '{', ''' or the expression end.

            for (var i = 0; i < expression.Length; ++i)
            {
                var character = expression[i];
                switch (character)
                {
                    case '=':
                        return true;
                    case '{':
                    case '\'':
                        return false;
                    default:
                        continue;
                }
            }

            return false;
        }

        public static bool IsExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                return false;
            }

            var isNestedExpression = false;
            var index = 0;
            var isInQuotes = false;
            while (index < expression.Length && !isNestedExpression)
            {
                for (; index < expression.Length; index++)
                {
                    var character = expression[index];

                    if (character == '\'')
                    {
                        if (!isInQuotes)
                        {
                            isInQuotes = true;
                        }
                        else
                        {
                            if (index - 1 < 0)
                            {
                                isInQuotes = false; // Not really possible but best to account for it.
                            }
                            else
                            {
                                // Check if previous char was an escape character.
                                var previous = expression[index - 1];

                                isInQuotes = previous == '\\';
                            }
                        }
                    }

                    if (character == '{')
                    {
                        if (!isInQuotes)
                        {
                            isNestedExpression = true;
                            break;
                        }
                    }
                }
            }

            return isNestedExpression;
        }

        public static bool BeginsWithKeyword(string expression, string keyword)
        {
            var foundKeyword = "";

            // First extract to first non whitespace character.
            var index = 0;
            for (; index < expression.Length; index++)
            {
                var character = expression[index];
                if (char.IsLetterOrDigit(character) || character == '_')
                {
                    break;
                }
            }

            for (; index < expression.Length; index++)
            {
                var character = expression[index];
                if (!(char.IsLetterOrDigit(character) || character == '_' || character == ':'))
                {
                    break;
                }
                else
                {
                    foundKeyword += character;
                }
            }

            var isMatch = foundKeyword == keyword;
            if (!isMatch)
            {

                // Attempt a fuzzy match.

                var distance = LevenshteinDistanceHelper.Compute(foundKeyword, keyword);
                if (distance < 3)
                {
                    isMatch = true;
                }
            }
            return isMatch;
        }

        public static bool ResolveMarkupExtensionExpressionNameAndRegions(string expression, int startOffset, out TextSpan fullNameSpan, out string fullName, out TextSpan xmlNamespaceSpan, out string xmlns, out TextSpan nameSpan, out string match)
        {
            int nameStart, nameEnd;
            nameSpan = default(TextSpan); ;
            xmlNamespaceSpan = default(TextSpan);

            fullName = "";
            fullNameSpan = default(TextSpan);
            xmlns = "";
            match = "";

            if (string.IsNullOrEmpty(expression))
            {
                return false;
            }

            var foundKeyword = "";

            // First extract to first non whitespace character.
            var index = 0;
            var offset = startOffset;

            for (; index < expression.Length; index++)
            {
                var character = expression[index];
                if (char.IsLetterOrDigit(character) || character == '_')
                {
                    break;
                }

                offset++;
            }

            nameStart = offset;
            for (; index < expression.Length; index++)
            {
                var character = expression[index];
                if (!(char.IsLetterOrDigit(character) || character == '_' || character == ':'))
                {
                    break;
                }
                else
                {
                    foundKeyword += character;
                }
            }

            nameEnd = nameStart + foundKeyword.Length;

            fullName = foundKeyword;
            fullNameSpan = TextSpan.FromBounds(nameStart, nameEnd);

            return ExplodeName(foundKeyword, fullNameSpan, out xmlns, out xmlNamespaceSpan, out match, out nameSpan);
        }

        public static bool ExtractAssignmentExpressionComponents(string expression, int startOffset, out ExpressionComponent nameComponent, out ExpressionComponent valueComponent)
        {
            nameComponent = null;
            valueComponent = null;

            var hasOpeningExpressionTag = false;

            // First extract to first non whitespace character.
            var index = 0;
            var offset = startOffset;
            for (; index < expression.Length; index++)
            {
                var character = expression[index];
                if (char.IsLetterOrDigit(character) || character == '_' || character == ':')
                {
                    break;
                }

                if (character == '{')
                {
                    hasOpeningExpressionTag = true;
                }

                offset++;
            }

            var keyword = "";
            var keywordStart = offset;
            for (; index < expression.Length; index++)
            {
                var character = expression[index];
                if (!(char.IsLetterOrDigit(character) || character == '_' || character == ':'))
                {
                    break;
                }

                keyword += character;
                offset++;
            }

            var keywordSpan = new TextSpan(keywordStart, keyword.Length);
            nameComponent = new ExpressionComponent(keyword, keywordSpan);

            var betweenKeyAndValue = 0;
            // Walk to the value component
            for (; index < expression.Length; index++)
            {
                var character = expression[index];
                if (!(char.IsWhiteSpace(character) || character == '='))
                {
                    break;
                }
                else
                {
                    betweenKeyAndValue++;
                }

                offset++;
            }

            var braceStackCount = 0;

            var valueContent = "";
            var isInQuotes = false;
            for (; index < expression.Length; index++)
            {
                var character = expression[index];

                if (character == '\'')
                {
                    if (!isInQuotes)
                    {
                        isInQuotes = true;
                    }
                    else
                    {
                        if (index - 1 < 0)
                        {
                            isInQuotes = false; // Not really possible but best to account for it.
                        }
                        else
                        {
                            // Check if previous char was an escape character.
                            var previous = expression[index - 1];

                            isInQuotes = previous == '\\';
                        }
                    }
                }

                if (character == '{')
                {
                    braceStackCount++;
                }
                else if (character == '}')
                {
                    if (!isInQuotes)
                    {
                        braceStackCount--;
                        if (braceStackCount < 0)
                        {
                            braceStackCount = 0;
                        }
                    }
                }

                if (character == '}')
                {
                    if (!isInQuotes && index - 1 > 0 && braceStackCount == 0 && hasOpeningExpressionTag)
                    {
                        if (index + 1 >= expression.Length)
                        {
                            break;
                        }
                    }
                }

                if (character == ' ' && !isInQuotes && braceStackCount == 0)
                {
                    break;
                }

                offset++;

                valueContent += character;
            }

            var span = new TextSpan(keywordSpan.End + betweenKeyAndValue, valueContent.Length);
            valueComponent = new ExpressionComponent(valueContent, span);

            return true;
        }

        public static List<ExpressionComponent> ParseExpressionComponents(string expression, int start)
        {
            var components = new List<ExpressionComponent>();
            var component = "";

            // First extract to first non whitespace character.
            var index = 0;
            var offset = start;
            var hasOpeningTag = false;
            for (; index < expression.Length; index++)
            {
                var character = expression[index];
                if (char.IsLetterOrDigit(character) || character == '_')
                {
                    break;
                }

                if (character == '{')
                {
                    hasOpeningTag = true;
                }

                offset++;
            }

            var componentStart = offset;

            // read the components
            var braceStackCount = 0;
            var isInQuotes = false;
            while (index < expression.Length)
            {

                for (; index < expression.Length; index++)
                {

                    var character = expression[index];
                    // Check if we have hit a , character.
                    if (character == ',')
                    {
                        if (!isInQuotes && braceStackCount == 0)
                        {
                            components.Add(new ExpressionComponent(component, new TextSpan(componentStart, component.Length)));
                            component = "";
                            offset++;
                            componentStart = offset;
                            continue;
                        }
                    }

                    if (character == '\'')
                    {
                        if (!isInQuotes)
                        {
                            isInQuotes = true;
                        }
                        else
                        {
                            if (index - 1 < 0)
                            {
                                isInQuotes = false; // Not really possible but best to account for it.
                            }
                            else
                            {
                                // Check if previous char was an escape character.
                                var previous = expression[index - 1];

                                isInQuotes = previous == '\\';
                            }
                        }
                    }

                    if (character == '{')
                    {
                        braceStackCount++;
                    }
                    else if (character == '}')
                    {
                        braceStackCount--;
                        if (braceStackCount < 0)
                        {
                            braceStackCount = 0;
                        }

                        if (!isInQuotes
                            && index - 1 > 0
                            && hasOpeningTag
                            && braceStackCount == 0)
                        {
                            if (index + 1 >= expression.Length)
                            {
                                index++;
                                break;
                            }
                        }
                    }

                    offset++;
                    component += expression[index];
                }
            }

            components.Add(new ExpressionComponent(component, new TextSpan(componentStart, component.Length)));

            return components;
        }

        public static string ExtractStringFormatValue(string formatContent, TextSpan formatSpan, out TextSpan formatValueSpan, out bool malformed)
        {
            var value = formatContent;
            var formatValueStart = formatSpan.Start;

            var hasStart = false;
            var hasEnd = false;

            if (value.StartsWith("'"))
            {
                hasStart = true;
                value = value.Remove(0, 1);
                formatValueStart += 1;
            }

            var offset = formatValueStart;
            var foundValue = "";
            for (var index = 0; index < value.Length; index++)
            {
                var character = value[index];

                offset++;
                if (character == '\'')
                {
                    var last = value.Length - 2;
                    if (last > 0 && value[last] != '\\')
                    {
                        hasEnd = true;
                        break;
                    }
                }

                foundValue += character;
            }

            value = foundValue;

            malformed = hasStart & hasEnd;
            formatValueSpan = new TextSpan(formatValueStart, value.Length);

            return value;
        }

        public static bool ExplodeName(string fullName, TextSpan fullNameSpan, out string xamlNamespace, out TextSpan namespaceSpan, out string name, out TextSpan nameSpan)
        {

            xamlNamespace = "";
            name = "";
            namespaceSpan = default(TextSpan);
            nameSpan = default(TextSpan);

            if (string.IsNullOrEmpty(fullName))
            {
                return false;
            }

            if (!fullName.Contains(":"))
            {
                name = fullName;
                nameSpan = fullNameSpan;
                return true;
            }

            var offset = fullNameSpan.Start;

            var foundValue = "";
            int index;
            for (index = 0; index < fullName.Length; index++)
            {
                var character = fullName[index];

                offset++;

                if (character == ':')
                {
                    xamlNamespace = foundValue;
                    namespaceSpan = TextSpan.FromBounds(fullNameSpan.Start, offset);

                    index++;

                    break;
                }

                foundValue += character;
            }

            var nameStart = fullNameSpan.Start;
            if (!string.IsNullOrEmpty(xamlNamespace))
            {
                nameStart = namespaceSpan.End + 1;
            }

            name = fullName.Substring(index);
            nameSpan = new TextSpan(nameStart, name.Length);// TextSpan.FromBounds(namespaceSpan.End.Line, namespaceSpan.End.Column + 1, namespaceSpan.End.Line, namespaceSpan.End.Column);

            return true;
        }

        public static string ExtractXmlNamespaceFromString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            var trimmed = input.Trim();

            var components = trimmed.Split(':');

            if (components == null || components.Length < 2)
            {
                return null;
            }

            return components[0];
        }
    }
}
