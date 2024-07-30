using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Configuration;

namespace MFractor.Code.CodeActions
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICodeActionRepository))]
    class CodeActionRepository : ConfigurablePartRepository<ICodeAction>, ICodeActionRepository
    {
        [ImportingConstructor]
        public CodeActionRepository(Lazy<IConfigurationEngine> configurationEngine)
                                    : base(configurationEngine)
        {
        }

        public IReadOnlyList<ICodeAction> CodeActions => Parts;
    }
}