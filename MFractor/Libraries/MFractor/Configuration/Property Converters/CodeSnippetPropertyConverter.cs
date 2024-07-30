using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MFractor.CodeSnippets;
using MFractor.Configuration.Attributes;
using MFractor.Documentation;

namespace MFractor.Configuration.PropertyConverters
{
    class CodeSnippetPropertyConverter : TypedPropertyConverter<ICodeSnippet>
    {
        const string fileSource = "file";
        const string ideSource = "ide";

        public override string Name => "Code Snippet";

        public override string Identifier => "com.mfractor.configuration.property_converters.code_snippet";

        public override string Documentation => "For an ICodeSnippet, finds the users configured snippet.";

        readonly Lazy<ICodeSnippetService> codeSnippetService;
        protected ICodeSnippetService CodeSnippetService => codeSnippetService.Value;

        readonly Lazy<ICodeSnippetFactory> codeSnippetFactory;
        public ICodeSnippetFactory CodeSnippetFactory => codeSnippetFactory.Value;

        [ImportingConstructor]
        public CodeSnippetPropertyConverter(Lazy<ICodeSnippetService> codeSnippetService,
                                            Lazy<ICodeSnippetFactory> codeSnippetFactory)
        {
            this.codeSnippetService = codeSnippetService;
            this.codeSnippetFactory = codeSnippetFactory;
        }

        public override bool ApplyValue(ConfigurationId configId,
                                        IPropertySetting setting,
                                        IConfigurableProperty property,
                                        out string errorMessage)
        {
            setting.MetaData.TryGetValue("source", out var source);

            if (string.IsNullOrEmpty(source))
            {
                source = "file";
            }

            switch (source)
            {
                case ideSource:
                    return ApplyFromIdeSnippet(configId, setting, property, out errorMessage);
                case fileSource:
                default:
                    if (source != fileSource)
                    {
                        errorMessage = $"\"{source}\" is an unknown code snippet source type.";
                        errorMessage += "Valid values are \"ide\" for IDE defined code snippets and \"file\" for project/package included code snippets.";
                        errorMessage += "Defaulting to \"file\" source.";
                    }
                    return ApplyFromFile(configId, setting, property, out errorMessage);
            }
        }

        bool ApplyFromIdeSnippet(ConfigurationId configId,
                                 IPropertySetting configuration,
                                 IConfigurableProperty property,
                                 out string errorMessage)
        {
            errorMessage = "";

            var snippet = CodeSnippetService.GetCodeSnippetById(configuration.Value);

            if (snippet == null)
            {
                errorMessage = $"A code snippet named {configuration.Value} could not be found";
                return false;
            }

            property.Value = snippet;

            return true;
        }

        bool ApplyFromFile(ConfigurationId configId,
                           IPropertySetting configuration,
                           IConfigurableProperty property,
                           out string errorMessage)
        {
            errorMessage = "";
            var codeSnippetPath = configuration.Value;
            var code = "";
            var description = "";

            try
            {
                var parent = configuration.Parent;
                var parentPath = new FileInfo(parent.FilePath);

                if (!Path.IsPathRooted(codeSnippetPath))
                {
                    codeSnippetPath = Path.Combine(parentPath.Directory.FullName, codeSnippetPath);
                }

                if (!File.Exists(codeSnippetPath))
                {
                    errorMessage = $"The code snippet file at \"{codeSnippetPath}\" was not found";
                    return false;
                }

                code = File.ReadAllText(codeSnippetPath);
            }
            catch (Exception ex)
            {
                errorMessage = "An unexpected error occurred while loading the code snippet. Reason:\n" + ex.ToString();
                return false;
            }

            description = "Autoloaded code snippet from " + codeSnippetPath;
            var name = Path.GetFileNameWithoutExtension(codeSnippetPath);

            property.Value = CodeSnippetFactory.CreateSnippet(name, description, code, CodeSnippetFactory.ExtractArguments(code));

            return true;
        }
    }
}
