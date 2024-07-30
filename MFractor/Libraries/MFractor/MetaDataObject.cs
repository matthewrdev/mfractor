using System;
using System.Collections.Generic;

namespace MFractor
{
    /// <summary>
    /// An object that allows arbitary data to be attached in "string key" - "object value" format.
    /// </summary>
    public abstract class MetaDataObject : IMetaDataObject
    {
        /// <summary>
        /// The dictionary containing the meta-data attached to this object.
        /// </summary>
        /// <value>The meta data.</value>
        public Dictionary<string, object> MetaData { get; } = new Dictionary<string, object>();

        /// <summary>
        /// If this object has any meta-data attached to it.
        /// </summary>
        /// <value>If this element </value>
        public bool HasMetaData => MetaData != null && MetaData.Count > 0;

        /// <summary>
        /// Adds a meta-data value entry under the <paramref name="key"/>.
        /// <para/>
        /// If this element already has an entry under <paramref name="key"/> the old entry will be replaced.
        /// </summary>
        /// <param name="key">The key of the meta-data entry to add.</param>
        /// <param name="value">The value of the meta-data entry to add.</param>
        public void Add(string key, object value)
        {
            MetaData[key] = value;
        }

        /// <summary>
        /// Checks if this object has a meta-data entry under <paramref name="key"/>.
        /// </summary>
        /// <returns>True if this object has a meta-data entry under <paramref name="key"/>, false otherwise.</returns>
        /// <param name="key">The key of the meta-data entry to check.</param>
        public bool HasKey(string key)
        {
            return MetaData.ContainsKey(key);
        }

        /// <summary>
        /// Gets the type-cast value of the meta-data entry under <paramref name="key"/>.
        /// <para/>
        /// If the key does not exist, then the default value is returned.
        /// </summary>
        /// <returns>The meta data entry under <paramref name="key"/> cast as T if the key exists; <paramref name="defaultValue"/> otherwise.</returns>
        /// <param name="key">The key of the meta-data entry to check.</param>
        /// <param name="defaultValue">The default value to return</param>
        /// <typeparam name="T">The type of parameter.</typeparam>
        public T Get<T>(string key, T defaultValue)
        {
            if (!HasKey(key))
            {
                return defaultValue;
            }

            var result = defaultValue;
            try
            {
                result = (T)MetaData[key];
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body

            return result;
        }

        /// <summary>
        /// Clears all meta-data from this object.
        /// </summary>
        public void Clear()
        {
            MetaData.Clear();
        }

        /// <summary>
        /// Removes the meta-data entry under the provided <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the meta-data entry to delete.</param>
        public void RemoveKey(string key)
        {
            if (HasKey(key))
            {
                MetaData.Remove(key);
            }
        }

        /// <summary>
        /// Replaces the old <see cref="P:MFractor.IMetaDataObject.MetaData"/> with the <paramref name="newMetaData"/>.
        /// <para/>
        /// If <paramref name="newMetaData"/> is <see langword="null"/> or empty, the old metadata is cleared.
        /// </summary>
        /// <param name="newMetaData">New meta data.</param>
        public void ReplaceAll(Dictionary<string, object> newMetaData)
        {
            MetaData.Clear();

            if (newMetaData != null)
            {
                foreach (var kp in newMetaData)
                {
                    MetaData[kp.Key] = kp.Value;
                }
            }
        }
    }
}

