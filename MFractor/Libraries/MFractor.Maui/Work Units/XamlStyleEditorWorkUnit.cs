using System.Collections.Generic;
using MFractor.Maui.CodeGeneration.Styles;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.WorkUnits
{
    /// <summary>
    /// A delegate to apply the new style.
    /// </summary>
    public delegate IReadOnlyList<IWorkUnit> ApplyStyleDelegate(IXamlPlatform platform,
                                                              string styleName,
                                                              string targetType,
                                                              string targetTypePrefix,
                                                              string parentStyleName,
                                                              ParentStyleType parentStyleType,
                                                              IReadOnlyDictionary<string, string> styleProperties,
                                                              string targetFilePath);

    /// <summary>
    /// Opens the style editor dialog.
    /// </summary>
    public class XamlStyleEditorWorkUnit : WorkUnit
    {
        /// <summary>
        /// The name of the style.
        /// </summary>
        public string StyleName { get; set; }

        /// <summary> 
        /// The symbol the 
        /// </summary>
        /// <value>The symbol.</value>
        public INamedTypeSymbol TargetType { get; set; }

        /// <summary>
        /// What is the xmlns prefix to use for the <see cref="TargetType"/>?
        /// </summary>
        /// <value>The target type prefix.</value>
        public string TargetTypePrefix { get; set; }

        /// <summary>
        /// The existing properties that should be included in the style.
        /// </summary>
        /// <value>The properties.</value>
        public IReadOnlyDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// If the style editor should show all properties available for editing or only the properites selected in <see cref="Properties"/>.
        /// <para/>
        /// Defaults to <see cref="false"/>.
        /// </summary>
        public bool ShowAllProperties { get; set; } = false;

        /// <summary>
        /// The existing style that the new style should be based on.
        /// </summary>
        /// <value>The parent style.</value>
        public string ParentStyleName { get; set; }

        /// <summary>
        /// Gets or sets the type of the parent style.
        /// </summary>
        /// <value>The type of the parent style.</value>
        public ParentStyleType ParentStyleType { get; set; }

        /// <summary>
        /// The XAML file that this style is being extracted from.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath { get; set; }

        /// <summary>
        /// The project that this style is being extracted form.
        /// </summary>
        /// <value>The project.</value>
        public Project Project { get; set; }

        /// <summary>
        /// The XAML platform that the stye editor is targetting.
        /// </summary>
        public IXamlPlatform Platform { get; set; }

        /// <summary>
        /// The files available to extract the style into.
        /// </summary>
        /// <value>The target files.</value>
        public IReadOnlyList<IProjectFile> TargetFiles { get; set; }

        /// <summary>
        /// The label to 
        /// </summary>
        public string ApplyButtonLabel { get; set; } = "Create Style";

        /// <summary>
        /// The help url 
        /// </summary>
        public string HelpUrl { get; set; }

        /// <summary>
        /// After the user has built their style, this callback informs of the new style name and which properties they choose to extract.
        /// </summary>
        /// <value>The callback.</value>
        public ApplyStyleDelegate ApplyStyleDelegate;
    }
}
