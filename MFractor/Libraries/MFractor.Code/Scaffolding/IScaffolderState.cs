using System;
using System.Collections.Generic;

namespace MFractor.Code.Scaffolding
{
    public interface IScaffolderState
    {
        IReadOnlyDictionary<string, object> Properties { get; }
    }
}