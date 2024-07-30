using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using MFractor.Attributes;

namespace MFractor.Configuration
{
    [ApplicationLifecyclePriority(uint.MaxValue)]
    public abstract class JsonUserOptions : IUserOptions, IApplicationLifecycleHandler
    {
        class JsonUserOptionsTransaction : IUserOptionsTransaction
        {
            readonly JsonUserOptions jsonUserOptions;

            public JsonUserOptionsTransaction(JsonUserOptions jsonUserOptions)
            {
                this.jsonUserOptions = jsonUserOptions;
            }

            public void Cancel()
            {
                jsonUserOptions.CancelTransaction();
            }

            public void Commit()
            {
                jsonUserOptions.CommitTransaction();
            }

            public void Dispose()
            {
                jsonUserOptions.CommitTransaction();
            }
        }

        protected JsonUserOptions(Lazy<IApplicationPaths> applicationPaths)
        {
            this.applicationPaths = applicationPaths;
        }

        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IApplicationPaths> applicationPaths;
        public IApplicationPaths ApplicationPaths => applicationPaths.Value;

        public event EventHandler<UserOptionChangedEventArgs> OnUserOptionChanged;

        void CommitTransaction()
        {
            currentTransaction = null;
            PersistPreferences();
        }

        void CancelTransaction()
        {
            currentTransaction = null;
        }

        void NotifyOptionChanged(string key)
        {
            try
            {
                OnUserOptionChanged?.Invoke(this, new UserOptionChangedEventArgs(key));
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
        const string preferencesFileName = ".preferences";

        string PreferencesFilePath => Path.Combine(ApplicationPaths.ApplicationDataPath, preferencesFileName);

        Dictionary<string, string> preferences = new Dictionary<string, string>();

        IUserOptionsTransaction currentTransaction;

        void PersistPreferences()
        {
            if (currentTransaction != null)
            {
                return;
            }

            if (File.Exists(PreferencesFilePath))
            {
                File.Delete(PreferencesFilePath);
            }

            try
            {
                var content = JsonConvert.SerializeObject(preferences, Newtonsoft.Json.Formatting.Indented);

                File.WriteAllText(PreferencesFilePath, content);
            }
            catch (Exception ex) // Shouldn't happen, indicates access permissions.
            {
                log?.Exception(ex);
            }
        }

        void ReloadPreferences()
        {
            log?.Info("Attempting to reload preferences");
            if (File.Exists(PreferencesFilePath))
            {
                try
                {
                    var content = File.ReadAllText(PreferencesFilePath);

                    preferences = JsonConvert.DeserializeObject<Dictionary<string, string>>(content) ?? new Dictionary<string, string>();
                    log?.Info("Reloaded preference store from " + PreferencesFilePath);
                }
                catch (Exception ex) // Shouldn't happen, indicates data-corruption.
                {
                    log?.Exception(ex);
                    Clear();
                }
            }
            else
            {
                log?.Info("Creating new preferences store.");
                preferences = new Dictionary<string, string>();
            }
        }

        void Clear()
        {
            try
            {
                if (File.Exists(PreferencesFilePath))
                {
                    File.Delete(PreferencesFilePath);
                }
            }
            finally
            {
                preferences = new Dictionary<string, string>();

            }
        }

        public bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            return preferences.ContainsKey(key);
        }

        void SetValue(string key, string value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                preferences[key] = value;

                PersistPreferences();
            }
        }

        string GetValue(string key)
        {
            if (preferences.TryGetValue(key, out var value))
            {
                return value;
            }

            return string.Empty;
        }

        public bool Get(string key, bool defaultValue)
        {
            if (!HasKey(key))
            {
                return defaultValue;
            }

            if (bool.TryParse(GetValue(key), out var value))
            {
                return value;
            }

            return defaultValue;
        }

        public void Set(string key, bool value)
        {
            SetValue(key, value.ToString());
            
            NotifyOptionChanged(key);
        }

        public string Get(string key, string defaultValue)
        {
            if (!HasKey(key))
            {
                return defaultValue;
            }

            var value = GetValue(key);

            return value;
        }

        public void Set(string key, string value)
        {
            SetValue(key, value);
            NotifyOptionChanged(key);
        }

        public int Get(string key, int defaultValue)
        {
            if (!HasKey(key))
            {
                return defaultValue;
            }


            if (int.TryParse(GetValue(key), out var value))
            {
                return value;
            }

            return defaultValue;
        }

        public void Set(string key, int value)
        {
            SetValue(key, value.ToString());

            NotifyOptionChanged(key);
        }


        public float Get(string key, float defaultValue)
        {
            if (!HasKey(key))
            {
                return defaultValue;
            }

            if (float.TryParse(GetValue(key), out var value))
            {
                return value;
            }

            return defaultValue;
        }

        public void Set(string key, float value)
        {
            SetValue(key, value.ToString());

            NotifyOptionChanged(key);
        }

        public double Get(string key, double defaultValue)
        {
            if (!HasKey(key))
            {
                return defaultValue;
            }


            if (double.TryParse(GetValue(key), out var value))
            {
                return value;
            }

            return defaultValue;
        }

        public void Set(string key, double value)
        {
            SetValue(key, value.ToString());
            NotifyOptionChanged(key);
        }

        public TEnum Get<TEnum>(string key, TEnum defaultValue) where TEnum : System.Enum
        {
            var value = Get(key, Convert.ToInt32(defaultValue));

            return (TEnum)Enum.ToObject(typeof(TEnum), value);
        }

        public void Set<TEnum>(string key, TEnum value) where TEnum : System.Enum
        {
            Set(key, Convert.ToInt32(value));
        }

        public void Startup()
        {
            ReloadPreferences();
        }

        public void Shutdown()
        {
        }

        public IUserOptionsTransaction StartTransaction()
        {
            if (currentTransaction != null)
            {
                currentTransaction.Commit();
            }

            currentTransaction = new JsonUserOptionsTransaction(this);

            return currentTransaction;
        }
    }
}
