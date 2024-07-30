using System.Collections.Generic;

namespace MFractor.Code.Scaffolding
{
    public class ScaffolderState : IScaffolderState
    {
        public IReadOnlyDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        public static readonly IScaffolderState Empty = new ScaffolderState();
    }
}