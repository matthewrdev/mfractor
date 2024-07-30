using System;
using System.Diagnostics;
using MFractor.Maui.Styles;

namespace MFractor.Maui.Styles
{
    [DebuggerDisplay("{Name} - {Value}")]
    class StyleProperty : IStyleProperty
    {
        public StyleProperty(int parentStyleKey,
                             string name,
                             IStylePropertyValue value,
                             int priority)
        {
            ParentStyleKey = parentStyleKey;
            Name = name;
            Value = value;
            Priority = priority;
        }

        public int ParentStyleKey
        {
            get;
        }

        public string Name
        {
            get;
        }

        public IStylePropertyValue Value
        {
            get;
        }

        public int Priority
        {
            get;
        }
    }
}