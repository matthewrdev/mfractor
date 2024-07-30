using System;
using System.Globalization;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Localisation
{
    class LocalisationValue : ILocalisationValue
    {
        public string Key { get; }
        public TextSpan? KeySpan { get; }
        public string Value { get; }
        public TextSpan? ValueSpan { get; }
        public string Comment { get; }
        public TextSpan? CommentSpan { get; }
        public CultureInfo Culture { get; }

        public LocalisationValue(string key,
                                 TextSpan? keySpan,
                                 string value,
                                 TextSpan? valueSpan,
                                 string comment,
                                 TextSpan? commentSpan,
                                 CultureInfo culture)
        {
            Key = key;
            Value = value;
            Culture = culture;
            Comment = comment;
            CommentSpan = commentSpan;
            KeySpan = keySpan;
            ValueSpan = valueSpan;
        }

        public bool HasCulture => Culture != null;
    }
}
