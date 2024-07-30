﻿using System;

namespace Microsoft.Language.Xml.InternalSyntax
{
    [Flags]
    public enum NodeFlags : byte
    {
        None = 0,
        ContainsDiagnostics = 1 << 0,
        ContainsStructuredTrivia = 1 << 1,
        ContainsDirectives = 1 << 2,
        ContainsSkippedText = 1 << 3,
        ContainsAnnotations = 1 << 4,
        IsMissing = 1 << 5,

        InheritMask = ContainsDiagnostics | ContainsStructuredTrivia | ContainsDirectives | ContainsSkippedText | ContainsAnnotations | IsMissing,
    }
}
