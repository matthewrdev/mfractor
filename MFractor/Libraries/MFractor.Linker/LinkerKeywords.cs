using System;

namespace MFractor.Linker
{
    public static class LinkerKeywords
    {
        public static class Elements
        {
            public const string Linker = "linker";
            public const string Assembly = "assembly";
            public const string Type = "type";
            public const string Namespace = "namespace";
            public const string Field = "field";
            public const string Method = "method";
        }

        public static class Attributes
        {
            public const string FullName = "fullname";
            public const string Name = "name";
            public const string Preserve = "preserve";
            public const string Signature = "signature";
        }
    }
}
