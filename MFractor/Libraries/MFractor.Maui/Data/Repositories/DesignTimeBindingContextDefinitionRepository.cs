using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Repositories;
using MFractor.Maui.Data.Models;

namespace MFractor.Maui.Data.Repositories
{
    /// <summary>
    /// Design time binding context definition repository.
    /// </summary>
    public class DesignTimeBindingContextDefinitionRepository : EntityRepository<DesignTimeBindingContextDefinition>
    {
        /// <summary>
        /// Gets the design time binding contexts for the provided <paramref name="codeBehindSymbol"/>.
        /// </summary>
        /// <returns>The binding contexts linked to the <paramref name="codeBehindSymbol"/>.</returns>
        /// <param name="codeBehindSymbol">The fully qualified meta data name of the code behind.</param>
        public IReadOnlyList<DesignTimeBindingContextDefinition> GetDesignTimeBindingContextsForSymbol(string codeBehindSymbol)
        {
            return Query(data => data.Values.Where(entity => entity.CodeBehindSymbol == codeBehindSymbol && !entity.GCMarked).ToList());
        }

        /// <summary>
        /// Gets the code behinds linked to the provided <paramref name="bindingContextSymbol"/>.
        /// </summary>
        /// <returns>The code behinds linked to the <paramref name="bindingContextSymbol"/></returns>
        /// <param name="bindingContextSymbol">The fully qualified meta data name of the binding context.</param>
        public IReadOnlyList<DesignTimeBindingContextDefinition> GetCodeBehindsForBindingContext(string bindingContextSymbol)
        {
            return Query(data => data.Values.Where(entity => entity.BindingContextSymbol == bindingContextSymbol && !entity.GCMarked).ToList());
        }
    }
}
