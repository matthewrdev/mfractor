using System;

namespace MFractor.Configuration
{
    /// <summary>
    /// User options.
    /// </summary>
	public interface IUserOptions
	{
        /// <summary>
        /// Starts a transaction against the user options.
        /// </summary>
        /// <returns></returns>
        IUserOptionsTransaction StartTransaction();

        /// <summary>
        /// Does the provided key exist in the <see cref="IUserOptions"/>?
        /// </summary>
        /// <returns><c>true</c>, if key was hased, <c>false</c> otherwise.</returns>
        /// <param name="key">Key.</param>
		bool HasKey(string key);

        /// <summary>
        /// Get the specified key and defaultValue.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="key">Key.</param>
        /// <param name="defaultValue">If set to <c>true</c> default value.</param>
		bool Get (string key, bool defaultValue);

        /// <summary>
        /// Set the specified key and value.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">If set to <c>true</c> value.</param>
		void Set (string key, bool value);

        /// <summary>
        /// Get the specified key and defaultValue.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="key">Key.</param>
        /// <param name="defaultValue">Default value.</param>
		string Get (string key, string defaultValue);

        /// <summary>
        /// Set the specified key and value.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
		void Set (string key, string value);

        /// <summary>
        /// Get the specified key and defaultValue.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="key">Key.</param>
        /// <param name="defaultValue">Default value.</param>
		int Get (string key, int defaultValue);

        /// <summary>
        /// Set the specified key and value.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
		void Set (string key, int value);


        /// <summary>
        /// Get the specified key and defaultValue.
        /// </summary>
        /// <returns>The get.</returns>
        /// <param name="key">Key.</param>
        /// <param name="defaultValue">Default value.</param>
        TEnum Get<TEnum>(string key, TEnum defaultValue) where TEnum : System.Enum;

        /// <summary>
        /// Set the specified key and value.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        void Set<TEnum>(string key, TEnum value) where TEnum : System.Enum;

        float Get (string key, float defaultValue);
		void Set (string key, float value);

		double Get (string key, double defaultValue);
		void Set (string key, double value);

        event EventHandler<UserOptionChangedEventArgs> OnUserOptionChanged;
	}
}
