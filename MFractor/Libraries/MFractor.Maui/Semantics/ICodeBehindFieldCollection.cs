using System;
using System.Collections.Generic;

namespace MFractor.Maui.Semantics
{
    public interface ICodeBehindFieldCollection : IEnumerable<ICodeBehindField>
    {
        IReadOnlyDictionary<string, ICodeBehindField> CodeBehindFields { get; }

        ICodeBehindField GetCodeBehindField(string name);
    }
}