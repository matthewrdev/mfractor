using System;
using System.Collections.Generic;
using MFractor.Maui.CodeGeneration.Styles;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Views.XamlStyleEditor
{
    public class XamlStyleEditedEventArgs : EventArgs
    {
        public IXamlPlatform Platform { get; }
        public string StyleName { get; }

        public string TargetType { get; }

        public string TargetTypePrefix { get; }

        public string ParentStyleName { get; }

        public ParentStyleType ParentStyleType { get; set; }

        public IReadOnlyDictionary<string, string> Properties { get; }

        public string TargetFile { get; set; }

        public XamlStyleEditedEventArgs(IXamlPlatform platform,
                                        string styleName, 
                                        string targetType, 
                                        string targetTypePrefix, 
                                        string parentStyleName,
                                        ParentStyleType parentStyleType,
                                        string targetFile,
                                        IReadOnlyDictionary<string, string> properties)
        {
            Platform = platform;
            StyleName = styleName;
            TargetType = targetType;
            TargetTypePrefix = targetTypePrefix;
            ParentStyleName = parentStyleName;
            ParentStyleType = parentStyleType;
            TargetFile = targetFile;
            Properties = properties;
        }
    }
}
