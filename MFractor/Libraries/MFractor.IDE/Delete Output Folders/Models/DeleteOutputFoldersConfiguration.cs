using Newtonsoft.Json;

namespace MFractor.Ide.DeleteOutputFolders.Models
{
    class DeleteOutputFoldersConfiguration : IDeleteOutputFoldersConfiguration
    {
        public DeleteOutputFoldersConfiguration()
        {
        }

        public DeleteOutputFoldersConfiguration(IDeleteOutputFoldersConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new System.ArgumentNullException(nameof(configuration));
            }

            Name = configuration.Name;
            Identifier = configuration.Identifier;
            OptionsImpl = new DeleteOutputFoldersOptions(configuration.Options);
        }

        public DeleteOutputFoldersConfiguration(string name, string identifier, IDeleteOutputFoldersOptions options)
        {
            if (options is null)
            {
                throw new System.ArgumentNullException(nameof(options));
            }

            Name = name ?? throw new System.ArgumentNullException(nameof(name));
            Identifier = identifier ?? throw new System.ArgumentNullException(nameof(identifier));
            OptionsImpl = new DeleteOutputFoldersOptions(options);
        }

        public string Name
        {
            get;
            set;
        }

        public string Identifier
        {
            get;
            set;
        }

        public DeleteOutputFoldersOptions OptionsImpl
        {
            get;
            set;
        }

        [JsonIgnore]
        public IDeleteOutputFoldersOptions Options => OptionsImpl;
    }
}