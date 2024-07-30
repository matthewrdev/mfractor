using System;
using System.Collections.Generic;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using MFractor.Workspace;

namespace MFractor.Maui.WorkUnits
{
    /// <summary>
    /// Opens the Value Converter Wizard.
    /// </summary>
    public class ValueConverterWizardWorkUnit : WorkUnit
    {
        /// <summary>
        /// The project to generate this converter within.
        /// </summary>
        public ProjectIdentifier TargetProject { get; set; }

        public IXamlPlatform Platform { get; set; }

        /// <summary>
        /// What is the name of the new value converter?
        /// <para/>
        /// When <see cref="AutomaticTypeInference"/> is true, causes <see cref="InputType"/> and <see cref="OutputType"/> to be automatically inferred from this name using the <see cref="ITypeInfermentService"/>.
        /// </summary>
        public string ValueConverterName { get; set; }

        /// <summary>
        /// What is the input type of this value coverter?
        /// <para/>
        /// If null or empty, defaults to <see cref="object"/>.
        /// </summary>
        public string InputType { get; set; }

        /// <summary>
        /// What is the output type of this value coverter?
        /// <para/>
        /// If null or empty, defaults to <see cref="object"/>.
        /// </summary>
        public string OutputType { get; set; }

        /// <summary>
        /// What is the parameter type of this value coverter?
        /// <para/>
        /// If null or empty, defaults to <see cref="object"/>.
        /// </summary>
        public string ParameterType { get; set; }

        /// <summary>
        /// As the user types in the a <see cref="ValueConverterName"/>, should the wizard slice apart the input and infer what the likely <see cref="InputType"/> and <see cref="OutputType"/> is?
        /// </summary>
        public bool AutomaticTypeInference { get; set; } = true;

        /// <summary>
        /// The default namespace the 
        /// </summary>
        /// <value>The namespace.</value>
        public string Namespace { get; set; }

        /// <summary>
        /// When creating the new value converter, should a XAML declaration be added to a <see cref="TargetFiles"/> resource dictionary?
        /// </summary>
        public bool CreateXamlDeclaration { get; set; }

        /// <summary>
        /// A collection of files that this new value converter could be added into.
        /// </summary>
        public IReadOnlyList<IProjectFile> TargetFiles { get; set; }

        /// <summary>
        /// Invoked 
        /// </summary>
        /// <value>The on converter generated.</value>
        public Func<string, IReadOnlyList<IWorkUnit>> OnConverterGenerated { get; set; }
    }
}
