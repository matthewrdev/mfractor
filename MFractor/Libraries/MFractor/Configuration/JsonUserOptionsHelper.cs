using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MFractor.Configuration
{
    /// <summary>
    /// A helper class that provides raw access to the <see cref="JsonUserOptions"/> data.
    /// <para/>
    /// This class is provided as a last resort if reading from the <see cref="IUserOptions"/> is not possible (EG: At immediate app startup before any IOC initialisation).
    /// </summary>
    internal static class JsonUserOptionsHelper
    {
        public const string PreferencesFileName = ".preferences";

        public static string ReadRawValue(IApplicationPaths applicationPaths, string key, string defaultValue = "")
        {
            if (applicationPaths is null)
            {
                throw new ArgumentNullException(nameof(applicationPaths));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));
            }

            var preferencesFilePath = Path.Combine(applicationPaths.ApplicationDataPath, PreferencesFileName);

            return ReadRawValue(preferencesFilePath, key, defaultValue);
        }

        public static string ReadRawValue(string preferencesFilePath, string key, string defaultValue = "")
        {
            if (string.IsNullOrWhiteSpace(preferencesFilePath))
            {
                throw new ArgumentException($"'{nameof(preferencesFilePath)}' cannot be null or whitespace.", nameof(preferencesFilePath));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));
            }

            if (!File.Exists(preferencesFilePath))
            {
                return defaultValue;
            }

            try
            {
                var fileContent = File.ReadAllText(preferencesFilePath);

                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    return defaultValue;
                }

                var options = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContent);

                if (!options.TryGetValue(key, out var value))
                {
                    return defaultValue;
                }

                return value;

            }
            catch
            {
                return defaultValue;
            }

        }
    }
}
