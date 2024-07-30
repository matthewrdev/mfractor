using System;
using Newtonsoft.Json;

namespace MFractor.Data
{
    /// <summary>
    /// The base class for all entities in a <see cref="IDatabase"/>.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// The entities primary key.
        /// </summary>
        /// <value>The primary key.</value>
        public int PrimaryKey { get; set; }
    }
}
