using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MFractor.Maui.Styles
{
    [DebuggerDisplay("{Name} - {TargetType}")]
    class Style : IStyle
    {
        public Style(string targetType,
                     string name,
                     bool isImplicit,
                     IReadOnlyList<string> inheritanceChain,
                     IStylePropertyCollection properties)
        {
            TargetType = targetType;
            Name = name;
            IsImplicit = isImplicit;
            InheritanceChain = inheritanceChain;
            Properties = properties;
        }

        public string TargetType
        {
            get;
        }

        public string Name
        {
            get;
        }

        public bool IsImplicit
        {
            get;
        }

        public IReadOnlyList<string> InheritanceChain { get; }

        public IStylePropertyCollection Properties
        {
            get;
        }
    }
}