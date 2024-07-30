using System;
using System.IO;

namespace MFractor.Localisation.Utilities
{
    public static class ResxFileHelper
    {
        public static int GetClosingTagOffset(string filePath)
        {
            var fileContent = File.ReadAllText(filePath);

            var fileEndData = "";
            var offset = -1;
            for (var i = fileContent.Length - 1; i >= 0; --i)
            {
                var character = fileContent[i];
                if (fileEndData.StartsWith("</root>", StringComparison.Ordinal))
                {
                    offset = i;
                    break;
                } 
                else
                {
                    fileEndData = fileEndData.Insert(0, character.ToString());
                }
            }

            if (offset == -1)
            {
                throw new InvalidOperationException($"The ResX file {filePath} does not have a closing tag");
            }

            return offset;
        }
    }
}
