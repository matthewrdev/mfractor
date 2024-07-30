using System;
using Microsoft.VisualStudio.Text.Tagging;

namespace MFractor.Editor.Adornments
{
    public class EscapedCharacterTag : ITag
    {
        public EscapedCharacterTag(string character)
        {
            if (string.IsNullOrEmpty(character))
            {
                throw new ArgumentException("message", nameof(character));
            }

            Character = character.Replace("&#10;", "\\n")
                                 .Replace("&gt;", ">")
                                 .Replace("&lt;", "<")
                                 .Replace("&amp;", "&")
                                 .Replace("&quot;", "\"");
        }

        public string Character { get; }
    }
}
