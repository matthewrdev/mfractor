namespace MFractor.Maui.CodeGeneration.Mvvm
{
    public class ViewViewModelGenerationOptions
    {
        public string ViewName { get; }
        public string ViewFolderPath { get; }
        public string ViewBaseClass { get; }
        public string ViewNamespace { get; }
        public string ViewXmlnsPrefix { get; set; }
        public ProjectIdentifier ViewProjectIdentifier { get; }

        public string ViewMetadataName
        {
            get
            {
                if (string.IsNullOrEmpty(ViewNamespace))
                {
                    return ViewName;
                }

                return ViewNamespace + "." + ViewName;
            }
        }

        public string ViewModelName { get; }
        public string ViewModelFolderPath { get; }
        public string ViewModelBaseClass { get; }
        public string ViewModelNamespace { get; }
        public ProjectIdentifier ViewModelProjectIdentifier { get; }

        public string ViewModelMetadataName
        {
            get
            {
                if (string.IsNullOrEmpty(ViewModelNamespace))
                {
                    return ViewModelName;
                }

                return ViewModelNamespace + "." + ViewModelName;
            }
        }


        public string BindingContextConnectorId { get; }

        public ViewViewModelGenerationOptions(string viewName,
                                              string viewFolderPath,
                                              string viewBaseClass,
                                              string viewNamespace,
                                              string viewXmlnsPrefix,
                                              ProjectIdentifier viewProjectIdentifier,
                                              string viewModelName,
                                              string viewModelFolderPath,
                                              string viewModelBaseClass,
                                              string viewModelNamespace,
                                              ProjectIdentifier viewModelProjectIdentifier,
                                              string bindingContextConnectorId)
        {
            ViewName = viewName;
            ViewFolderPath = viewFolderPath;
            ViewBaseClass = viewBaseClass;
            ViewNamespace = viewNamespace;
            ViewXmlnsPrefix = viewXmlnsPrefix;
            ViewProjectIdentifier = viewProjectIdentifier;
            ViewModelName = viewModelName;
            ViewModelFolderPath = viewModelFolderPath;
            ViewModelBaseClass = viewModelBaseClass;
            ViewModelNamespace = viewModelNamespace;
            ViewModelProjectIdentifier = viewModelProjectIdentifier;
            BindingContextConnectorId = bindingContextConnectorId;
        }
    }
}
