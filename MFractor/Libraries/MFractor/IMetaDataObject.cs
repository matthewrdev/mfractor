using System.Collections.Generic;

namespace MFractor
{
    /// <summary>
    /// An object that allows arbitary data to be attached in "string key" - "object value" format.
    /// </summary>
    public interface IMetaDataObject
    {
        /// <summary>
        /// The dictionary containing the meta-data attached to this object.
        /// <para/>
        /// This dictionary is not guaranteed to be created: for safe access to the objects meta-data
        /// use <see cref="Add"/>, <see cref="HasMetaData"/> and <see cref="Get"/>.
        /// </summary>
        /// <value>The meta data.</value>
        Dictionary<string, object> MetaData { get; }

        /// <summary>
        /// If this object has any meta-data attached to it.
        /// </summary>
        /// <value>If this element has meta-data attached to it.</value>
        bool HasMetaData { get; }

        /// <summary>
        /// Adds a meta-data value entry under the <paramref name="key"/>.
        /// <para/>
        /// If this element already has an entry under <paramref name="key"/> the old entry will be replaced.
        /// </summary>
        /// <param name="key">The key of the meta-data entry to add.</param>
        /// <param name="value">The value of the meta-data entry to add.</param>
        void Add(string key, object value);

        /// <summary>
        /// Checks if this object has a meta-data entry under <paramref name="key"/>.
        /// </summary>
        /// <returns>True if this object has a meta-data entry under <paramref name="key"/>, false otherwise.</returns>
        /// <param name="key">The key of the meta-data entry to check.</param>
        bool HasKey(string key);

        /// <summary>
        /// Gets the type-cast value of the meta-data entry under <paramref name="key"/>.
        /// <para/>
        /// If the key does not exist, then the default value is returned.
        /// </summary>
        /// <returns>The meta data entry under <paramref name="key"/> cast as T if the key exists; <paramref name="defaultValue"/> otherwise.</returns>
        /// <param name="key">The key of the meta-data entry to check.</param>
        /// <param name="defaultValue">The default value to return</param>
        /// <typeparam name="T">The type of parameter.</typeparam>
        T Get<T>(string key, T defaultValue);

        /// <summary>
        /// Clears all meta-data from this object.
        /// </summary>
        void Clear();

        /// <summary>
        /// Replaces the old <see cref="MetaData"/> with the <paramref name="newMetaData"/>.
        /// <para/>
        /// If <paramref name="newMetaData"/> is <see langword="null"/> or empty, the old metadata is cleared.
        /// </summary>
        /// <param name="newMetaData">New meta data.</param>
        void ReplaceAll(Dictionary<string, object> newMetaData);

        /// <summary>
        /// Removes the meta-data entry under the provided <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the meta-data entry to delete.</param>
        void RemoveKey(string key);
    }
}
