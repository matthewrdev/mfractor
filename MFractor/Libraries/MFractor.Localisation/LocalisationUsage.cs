using System;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Localisation
{
    public class LocalisationUsage
    {
        public string Key { get; }
        public string ContextDescription { get; }
        public string File { get; }
        public TextSpan Span { get; }

        public LocalisationUsage(string key, 
                                 string contextDescription, 
                                 string file, 
                                 TextSpan span)
        {
            Key = key;
            ContextDescription = contextDescription;
            File = file;
            Span = span;
        }
    }
}
