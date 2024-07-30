using System;
using System.Text.RegularExpressions;

namespace MFractor.Android.Helpers
{
    public static class AndroidResourceNameHelper
    {
        public const string ResourceNameRegexExpression = "^([a-z.A-Z_0-9])+$";
        public static readonly Regex ResourceNameRegex = new Regex(ResourceNameRegexExpression, RegexOptions.Compiled);

        public const string ResourceReferenceRegexExpression = "@(\\+)?(([a-zA-Z0-9_.])+:)?([a-zA-Z])+\\/([a-zA-Z_.0-9])+";
        public const string ThemeReferenceRegexExpression = "\\?[_a-zA-Z0-9.]+";
        public const string PackageScopeThemeReferenceRegexExpression = "\\?\\+?\\w+?\\:+[a-zA-Z]+\\/[_a-zA-Z0-9.]+";

        public static readonly Regex ResourceReferenceRegex = new Regex(ResourceReferenceRegexExpression, RegexOptions.Compiled);
        public static readonly Regex ThemeReferenceRegex = new Regex(ThemeReferenceRegexExpression, RegexOptions.Compiled);
        public static readonly Regex PackageScopeThemeReferenceRegex = new Regex(PackageScopeThemeReferenceRegexExpression, RegexOptions.Compiled);

        public const string DimensionFormatRegexExpresion = "(^\\d*\\.?\\d*)([a-zA-Z])+$";
        public const string NumericalRegexExpresion = "(^\\d*\\.?\\d*)";
        public const string FractionRegexExpression = "(^\\d*\\.?\\d*)(%(p)?)+$";
        public const string ColorFormatRegexPattern = @"\A\b[0-9a-fA-F]+\b\Z";
        public const string HexaDecimal8BitRegexPattern = "0x([a-fA-F0-9]{4})";
        public const string HexaDecimal16BitRegexPattern = "0x([a-fA-F0-9]{8})";

        public static readonly Regex DimensionFormatRegex = new Regex(DimensionFormatRegexExpresion, RegexOptions.Compiled);
        public static readonly Regex NumericalRegex = new Regex(NumericalRegexExpresion, RegexOptions.Compiled);
        public static readonly Regex FractionRegex = new Regex(FractionRegexExpression, RegexOptions.Compiled);
        public static readonly Regex ColorRegex = new Regex(ColorFormatRegexPattern, RegexOptions.Compiled);
        public static readonly Regex HexaDecimal8BitRegex = new Regex(HexaDecimal8BitRegexPattern, RegexOptions.Compiled);
        public static readonly Regex HexaDecimal16BitRegex = new Regex(HexaDecimal16BitRegexPattern, RegexOptions.Compiled);
    }
}
