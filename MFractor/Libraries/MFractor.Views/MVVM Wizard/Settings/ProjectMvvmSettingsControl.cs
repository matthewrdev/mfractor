using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Configuration;

using MFractor.Maui.Mvvm;
using MFractor.Maui.Mvvm.BindingContextConnectors;
using MFractor.IOC;
using MFractor.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;
using Xwt;
using Xwt.Drawing;
using MFractor.Maui.XamlPlatforms;

namespace MFractor.Views.MVVMWizard.Settings
{
    public class ProjectMvvmSettingsControl : VBox
    {
        [Import]
        IBaseViewModelInferenceService BaseViewModelInferenceService { get; set; }

        [Import]
        IProjectMvvmSettingsService ProjectMvvmOptionsService { get; set; }

        [Import]
        IBindingContextConnectorService BindingContextConnectorService { get; set; }

        [Import]
        IProjectService ProjectService { get; set; }

        [Import]
        IDialogsService DialogsService { get; set; }

        [Import]
        IDispatcher Dispatcher { get; set; }

        IReadOnlyList<IBindingContextConnector> BindingContextConnectors { get; }

        public event EventHandler<ProjectMvvmSettingsSavedEventArgs> MvvmOptionsSaved;

        public ProjectIdentifier ProjectIdentifier { get; set; }
        public ConfigurationId ConfigurationId { get; private set; }

        TextEntry viewSuffixEntry;

        TextEntry viewFolderEntry;

        TextEntry viewBaseClassEntry;

        ComboBox viewProjectCombo;

        Label viewBaseClassXmlnsPrefixLabel;
        TextEntry viewBaseClassPrefixXmlnsEntry;

        TextEntry viewModelSuffixEntry;

        TextEntry viewModelFolderEntry;

        ComboBox viewModelProjectCombo;

        HBox viewModelBaseClassContainer;
        TextEntry viewModelBaseClassEntry;
        Button detectBaseViewModelButton;

        ComboBox bindingContextConnectionCombo;

        Button resetSettingsButton;

        string defaultViewModel = "";

        public ProjectMvvmSettingsControl()
        {
            Resolver.ComposeParts(this);

            BindingContextConnectors = BindingContextConnectorService.BindingContextConnectors;

            Build();

            BindEvents();
        }

        void BindEvents()
        {
            UnbindEvents();

            bindingContextConnectionCombo.SelectionChanged += BindingContextConnectionCombo_SelectionChanged;
        }

        void BindingContextConnectionCombo_SelectionChanged(object sender, EventArgs e)
        {
            bindingContextConnectionCombo.TooltipText = BindingContextConnectorService.BindingContextConnectors[bindingContextConnectionCombo.SelectedIndex].Documentation;
        }

        void UnbindEvents()
        {
            bindingContextConnectionCombo.SelectionChanged -= BindingContextConnectionCombo_SelectionChanged;
        }

        public void Load(ProjectIdentifier projectIdentifier, IXamlPlatform platform)
        {
            ProjectIdentifier = projectIdentifier;
            ConfigurationId = ConfigurationId.Create(projectIdentifier);

            var options = ProjectMvvmOptionsService.Load(projectIdentifier, platform);

            Load(options, projectIdentifier);
        }

        public void Load(ProjectMvvmSettings options, ProjectIdentifier projectIdentifier)
        {
            var solution = ProjectService.GetProject(projectIdentifier).Solution;

            Load(options, solution);
        }

        public void Load(ProjectMvvmSettings options, Solution solution)
        {
            var availableProjects = solution.Projects.Where(p => p.SupportsCompilation).Select(p => p.GetIdentifier()).ToList();

            Load(options, availableProjects);
        }

        public void Load(ProjectMvvmSettings options, IReadOnlyList<ProjectIdentifier> projects)
        {
            viewBaseClassEntry.Text = options.ViewBaseClass;
            viewBaseClassPrefixXmlnsEntry.Text = options.ViewBaseClassXmlnsPrefix;
            viewFolderEntry.Text = options.ViewFolder;
            viewModelFolderEntry.Text = options.ViewModelFolder;
            viewModelBaseClassEntry.Text = options.ViewModelBaseClass;
            viewModelSuffixEntry.Text = options.ViewModelSuffix;
            viewSuffixEntry.Text = options.ViewSuffix;

            var connector = BindingContextConnectorService.ResolveById(options.BindingContextConnectorId);

            bindingContextConnectionCombo.SelectedIndex = connector != null ? EnumerableHelper.IndexOf(BindingContextConnectors, connector) : 0;
            bindingContextConnectionCombo.TooltipText = BindingContextConnectorService.BindingContextConnectors[bindingContextConnectionCombo.SelectedIndex].Documentation;

            viewProjectCombo.Items.Clear();
            viewModelProjectCombo.Items.Clear();

            foreach (var p in projects)
            {
                viewProjectCombo.Items.Add(p, p.Name);
                viewModelProjectCombo.Items.Add(p, p.Name);
            }

            viewProjectCombo.SelectedIndex = EnumerableHelper.FindIndex(projects, (p => p.Guid == options.ViewProjectId));
            viewModelProjectCombo.SelectedIndex = EnumerableHelper.FindIndex(projects, (p => p.Guid == options.ViewModelProjectId));
        }

        public void Save()
        {
            var settings = GetSettings();
            ProjectMvvmOptionsService.Save(ProjectIdentifier, settings);

            MvvmOptionsSaved?.Invoke(this, new ProjectMvvmSettingsSavedEventArgs(settings, ProjectIdentifier));
        }

        public ProjectMvvmSettings GetSettings()
        {
            var bindingContextConnectorId = BindingContextConnectorService.ResolveByName(bindingContextConnectionCombo.SelectedText)?.Identifier ?? DefaultBindingContextConnector.Id;

            var viewProjectId = (viewProjectCombo.SelectedItem as ProjectIdentifier).Guid;
            var viewModelProjectId = (viewModelProjectCombo.SelectedItem as ProjectIdentifier).Guid;

            var options = new ProjectMvvmSettings
            {
                ViewBaseClass = viewBaseClassEntry.Text,
                ViewBaseClassXmlnsPrefix = viewBaseClassPrefixXmlnsEntry.Text,
                ViewFolder = viewFolderEntry.Text,
                ViewModelFolder = viewModelFolderEntry.Text,
                ViewModelBaseClass = viewModelBaseClassEntry.Text,
                ViewModelSuffix = viewModelSuffixEntry.Text,
                ViewSuffix = viewSuffixEntry.Text,
                ViewModelProjectId = viewModelProjectId,
                ViewProjectId = viewProjectId,
                BindingContextConnectorId = bindingContextConnectorId,
            };

            return options;
        }

        void Build()
        {
            BuildViewSettings();

            PackStart(new HSeparator());

            BuildViewModelSettings();

            PackStart(new HSeparator());

            resetSettingsButton = new Button("Reset Settings");

            PackStart(resetSettingsButton);
        }

        void BuildViewSettings()
        {
            PackStart(new Label("View Project:")
            {
                Font = Font.SystemFont.WithWeight(FontWeight.Bold),
            });

            viewProjectCombo = new ComboBox()
            {
                TooltipText = "What is the project that a new view should be placed within?",
            };
            PackStart(viewProjectCombo);

            PackStart(new Label("View Suffix:")
            {
                Font = Font.SystemFont.WithWeight(FontWeight.Bold),
            });

            viewSuffixEntry = new TextEntry();
            PackStart(viewSuffixEntry);

            viewFolderEntry = new TextEntry();
            viewFolderEntry.PlaceholderText = viewFolderEntry.TooltipText = "What folder will the new view be placed into?\n\nYou can include $name$ to indicate that the name of the View/ViewModel pair be inserted.";

            PackStart(new Label("View Folder:")
            {
                Font = Font.SystemFont.WithWeight(FontWeight.Bold),
            });
            PackStart(viewFolderEntry);

            viewBaseClassEntry = new TextEntry();
            viewBaseClassEntry.PlaceholderText = viewBaseClassEntry.TooltipText = "What is the base class to use for the new view?";

            PackStart(new Label("View Base Class:")
            {
                Font = Font.SystemFont.WithWeight(FontWeight.Bold),
            });
            PackStart(viewBaseClassEntry);

            viewBaseClassXmlnsPrefixLabel = new Label("View Base Class XML Namespace:")
            {
                Font = Font.SystemFont.WithWeight(FontWeight.Bold),
            };
            viewBaseClassXmlnsPrefixLabel.TooltipText = "When the base class for the new view is not a type from the framework assembly, what prefix should it use?";

            viewBaseClassPrefixXmlnsEntry = new TextEntry();
            viewBaseClassPrefixXmlnsEntry.PlaceholderText = viewBaseClassPrefixXmlnsEntry.TooltipText = "When the base class for the new view is not a type from the framework assembly, what prefix should it use?";

            PackStart(viewBaseClassXmlnsPrefixLabel);
            PackStart(viewBaseClassPrefixXmlnsEntry);

            PackStart(new Label("Binding Context Initialisation:")
            {
                Font = Font.SystemFont.WithWeight(FontWeight.Bold),
            });
            bindingContextConnectionCombo = new ComboBox();

            foreach (var c in BindingContextConnectors)
            {
                bindingContextConnectionCombo.Items.Add(c.Name);
            }

            bindingContextConnectionCombo.SelectedIndex = 0;
            PackStart(bindingContextConnectionCombo);
        }

        void BuildViewModelSettings()
        {
            viewModelProjectCombo = new ComboBox()
            {
                TooltipText = "What is the project that a new view model should be placed within?",
            };

            PackStart(new Label("ViewModel Project:")
            {
                Font = Font.SystemFont.WithWeight(FontWeight.Bold),
            });
            PackStart(viewModelProjectCombo);

            viewModelSuffixEntry = new TextEntry
            {
                TooltipText = "What is the suffix for the new ViewModel?",
                PlaceholderText = "What is the name of the new ViewModel?"
            };

            PackStart(new Label("ViewModel Suffix:")
            {
                Font = Font.SystemFont.WithWeight(FontWeight.Bold),
            });
            PackStart(viewModelSuffixEntry);

            viewModelFolderEntry = new TextEntry();
            viewModelFolderEntry.PlaceholderText = viewModelFolderEntry.TooltipText = "What folder will the new ViewModel be placed into?\n\nYou can include $name$ to indicate that the name of the View/ViewModel pair be inserted.";

            PackStart(new Label("ViewModel Folder:")
            {
                Font = Font.SystemFont.WithWeight(FontWeight.Bold),
            });
            PackStart(viewModelFolderEntry);

            viewModelBaseClassContainer = new HBox();

            viewModelBaseClassEntry = new TextEntry();
            viewModelBaseClassEntry.PlaceholderText = viewModelBaseClassEntry.TooltipText = "What is the base class to use for the new ViewModel?";

            detectBaseViewModelButton = new Button();
            detectBaseViewModelButton.Image = Image.FromResource("wand.png").WithSize(20, 20);
            detectBaseViewModelButton.VerticalPlacement = WidgetPlacement.Center;
            detectBaseViewModelButton.HorizontalPlacement = WidgetPlacement.Center;
            detectBaseViewModelButton.TooltipText = "MFractor can figure out what the most likely ViewModel base class is based on your projects current ViewModels.\n\nIf you would like MFractor to automatically decide this, click on this button.";
            detectBaseViewModelButton.Clicked += async (sender, e) =>
            {
                await RunBaseViewModelDetection();
            };

            PackStart(new Label("ViewModel Base Class:")
            {
                Font = Font.SystemFont.WithWeight(FontWeight.Bold),
            });
            viewModelBaseClassContainer.PackStart(viewModelBaseClassEntry, true, true);
            viewModelBaseClassContainer.PackStart(detectBaseViewModelButton);

            PackStart(viewModelBaseClassContainer);
        }

        public Task RunBaseViewModelDetection()
        {
            return Task.Run(async () =>
            {
                var baseType = await BaseViewModelInferenceService.InferBaseViewModelForProjectAsync(ProjectIdentifier);

                if (baseType != null)
                {
                    UnbindEvents();

                    defaultViewModel = baseType.ToString();

                    await Dispatcher.InvokeOnMainThreadAsync(() =>
                    {
                        viewModelBaseClassEntry.Text = defaultViewModel;
                    });
                }
                else
                {
                    DialogsService.ShowMessage("We couldn't locate a ViewModel base class; MFractor inspects the base classes of your ViewModels to find the most common one.\n\nDo your view models use a base class?", "Ok");
                }
            });
        }
    }
}
