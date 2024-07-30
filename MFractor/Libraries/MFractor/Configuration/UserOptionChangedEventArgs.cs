using System;

namespace MFractor.Configuration
{
    /// <summary>
    /// The <see cref="EventArgs"/> that fire when a <see cref="IUserOptions"/> value changes.
    /// </summary>
    public class UserOptionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The key that changed.
        /// </summary>
        /// <value>The key.</value>
        public string Key { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFractor.Configuration.UserOptionChangedEventArgs"/> class.
        /// </summary>
        /// <param name="key">Key.</param>
        public UserOptionChangedEventArgs(string key)
        {
            Key = key;
        }
    }
}
