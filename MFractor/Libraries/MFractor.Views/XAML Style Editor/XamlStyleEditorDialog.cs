using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Configuration;
using MFractor.IOC;
using MFractor.Maui.CodeGeneration.Styles;
using MFractor.Maui.XamlPlatforms;
using MFractor.Utilities;
using MFractor.Utilities.StringMatchers;
using MFractor.Views.Branding;
using MFractor.Workspace;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.XamlStyleEditor
{

    public class XamlStyleEditorDialog : Xwt.Dialog
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [DebuggerDisplay("{Name} - {Value} - {Selected}")]
        class StyleProperty
        {
            public StyleProperty(bool isEnabled, string name, string value)
            {
                Selected = isEnabled;
                Name = name;
                Value = value;
            }

            public string Name { get; }

            public bool Selected { get; set; }

            public string Value { get; set; }
        }

        readonly LaneStringMatcher filter = new LaneStringMatcher();

        [Import]
        IConfigurationEngine ConfigurationEngine { get; set; }

        [Import]
        ITextEditorFactory TextEditorFactory { get; set; }

        [Import]
        IXmlFormattingPolicyService XmlFormattingPolicyService { get; set; }

        [Import]
        IXmlSyntaxWriter XmlSyntaxWriter { get; set; }

        [Import]
        IDispatcher Dispatcher { get; set; }

        readonly IReadOnlyList<StyleProperty> styleProperties;

        VBox root;

        HBox mainContainer;

        VBox leftContainer;

        VBox rightContainer;

        Button confirmButton;
        TextEntry nameEntry;
        TextEntry targetTypeEntry;
        TextEntry targetTypeXmlnsEntry;
        TextEntry parentStyleEntry;
        ComboBox parentStyleCombo;

        ComboBox fileCombo;
        ITextEditor editor;

        TextEntry propertySearchBar;

        ListView propertiesListView;

        CheckBox showAllPropertiesCheckbox;

        public bool ShowAllProperties
        {
            get => showAllPropertiesCheckbox.Active;
            set
            {
                UnbindEvents();
                try
                {
                    showAllPropertiesCheckbox.Active = value;

                    ApplyPropertiesToList();
                }
                finally
                {
                    BindEvents();
                }
            }
        }

        public string StyleName
        {
            get => nameEntry.Text;
            set
            {
                UnbindEvents();
                try
                {
                    nameEntry.Text = value;

                    ApplyStyleCode();
                }
                finally
                {
                    BindEvents();
                }
            }
        }

        public string ButtonLabel
        {
            get => confirmButton.Label;
            set => confirmButton.Label = value;
        }

        public string HelpUrl
        {
            get => brandedFooter.HelpUrl;
            set => brandedFooter.HelpUrl = value;
        }

        ListStore propertiesDataStore;

        readonly DataField<bool> includePropertyField = new DataField<bool>();
        readonly DataField<string> propertyNameField = new DataField<string>();
        readonly DataField<string> propertyValueField = new DataField<string>();

        CheckBoxCellView includePropertyCell;
        TextCellView propertyNameCell;
        TextCellView propertyValueCell;
        private BrandedFooter brandedFooter;

        public ProjectIdentifier ProjectIdentifier { get; }
        public ConfigurationId ConfigId { get; }
        public string CurrentFile { get; }
        public INamedTypeSymbol TargetType { get; }
        public IXamlPlatform Platform { get; }
        public IReadOnlyList<IProjectFile> TargetFiles { get; }
        public Dictionary<string, string> ExistingProperties { get; }

        readonly IStyleGenerator styleGenerator;

        public event EventHandler<XamlStyleEditedEventArgs> XamlStyleEdited;

        public XamlStyleEditorDialog(ProjectIdentifier projectIdentifier,
                                     string currentFile,
                                     INamedTypeSymbol targetType,
                                     string targetTypePrefix,
                                     IReadOnlyDictionary<string, string> existingProperties,
                                     string parentStyle,
                                     ParentStyleType parentStyleType,
                                     IXamlPlatform platform,
                                     IReadOnlyList<IProjectFile> targetFiles)
        {
            Resolver.ComposeParts(this);

            Title = "XAML Style Editor";
            Icon = Image.FromResource("mfractor_logo.png");

            Build();

            Height = 640;
            Width = 960;

            ProjectIdentifier = projectIdentifier;
            ConfigId = ConfigurationId.Create(projectIdentifier);
            CurrentFile = currentFile;
            TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            Platform = platform ?? throw new ArgumentNullException(nameof(platform));
            TargetFiles = targetFiles ?? new List<IProjectFile>();

            styleGenerator = ConfigurationEngine.Resolve<IStyleGenerator>(ConfigId);

            var properties = new Dictionary<string, StyleProperty>();

            var availableProperties = SymbolHelper.GetAllMemberSymbols<IPropertySymbol>(targetType)
                                                  .Where(p => p.IsStatic == false)
                                                  .Where(p => p.SetMethod != null)
                                                  .Where(p => !p.ExplicitInterfaceImplementations.Any());

            foreach (var prop in availableProperties)
            {
                properties[prop.Name] = new StyleProperty(false, prop.Name, "");
            }

            if (existingProperties != null)
            {
                foreach (var prop in existingProperties)
                {
                    if (properties.ContainsKey(prop.Key))
                    {
                        properties[prop.Key].Selected = true;
                        properties[prop.Key].Value = prop.Value;
                    }
                    else
                    {
                        properties[prop.Key] = new StyleProperty(true, prop.Key, prop.Value);
                    }
                }
            }

            this.styleProperties = properties.Values.ToList();

            ApplyPropertiesToList();

            parentStyleEntry.Text = parentStyle;
            parentStyleCombo.SelectedItem = parentStyleType.ToString();
            targetTypeEntry.Text = targetType.ToString();
            targetTypeXmlnsEntry.Text = targetTypePrefix;

            foreach (var targetFile in targetFiles.Where(tf => tf != null))
            {
                fileCombo.Items.Add(targetFile.VirtualPath);
            }

            fileCombo.SelectedIndex = 0;

            ApplyStyleCode();

            BindEvents();
        }

        IReadOnlyDictionary<string, string> GetSelectedProperties()
        {
            var properties = new Dictionary<string, string>();

            foreach (var p in this.styleProperties.Where(p => p.Selected))
            {
                properties[p.Name] = p.Value;
            }

            return properties;
        }

        void ApplyPropertiesToList()
        {
            var sorted = styleProperties.OrderBy(p => p.Name).ToList();

            if (!showAllPropertiesCheckbox.Active)
            {
                sorted = sorted.Where(s => s.Selected).ToList();
            }

            if (!string.IsNullOrEmpty(propertySearchBar.Text))
            {
                filter.SetFilter(propertySearchBar.Text);
                sorted = filter.Match(sorted, item => item.Name).ToList();
            }

            propertiesDataStore.Clear();

            foreach (var prop in sorted)
            {
                var row = propertiesDataStore.AddRow();
                propertiesDataStore.SetValue(row, includePropertyField, prop.Selected);
                propertiesDataStore.SetValue(row, propertyNameField, prop.Name);
                propertiesDataStore.SetValue(row, propertyValueField, prop.Value);
            }
        }

        void Build()
        {
            root = new VBox();

            mainContainer = new HBox();

            BuildLeftPanel();

            mainContainer.PackStart(new VSeparator());

            BuildRightPanel();

            root.PackStart(mainContainer, true, true);

            confirmButton = new Button();
            confirmButton.Label = "Extract Style";

            root.PackStart(confirmButton);

            brandedFooter = new BrandedFooter(string.Empty, "Xaml Style Editor");

            root.PackStart(new HSeparator());
            root.PackStart(brandedFooter);

            Content = root;
        }

        void BindEvents()
        {
            UnbindEvents();

            propertySearchBar.Changed += PropertySearchBar_Changed;
            nameEntry.Changed += Entry_Changed;
            targetTypeEntry.Changed += Entry_Changed;
            targetTypeXmlnsEntry.Changed += Entry_Changed;
            parentStyleEntry.Changed += Entry_Changed;
            includePropertyCell.Toggled += IncludePropertyCell_Toggled;
            propertyValueCell.TextChanged += PropertyValueCell_TextChanged;
            confirmButton.Clicked += ConfirmButton_Clicked;
            parentStyleCombo.SelectionChanged += ParentStyleCombo_SelectionChanged;
            showAllPropertiesCheckbox.Toggled += ShowAllPropertiesCheckbox_Toggled;
        }

        void UnbindEvents()
        {
            propertySearchBar.Changed -= PropertySearchBar_Changed;
            nameEntry.Changed -= Entry_Changed;
            targetTypeEntry.Changed -= Entry_Changed;
            targetTypeXmlnsEntry.Changed -= Entry_Changed;
            parentStyleEntry.Changed -= Entry_Changed;
            includePropertyCell.Toggled -= IncludePropertyCell_Toggled;
            propertyValueCell.TextChanged -= PropertyValueCell_TextChanged;
            confirmButton.Clicked -= ConfirmButton_Clicked;
            parentStyleCombo.SelectionChanged -= ParentStyleCombo_SelectionChanged;
            showAllPropertiesCheckbox.Toggled -= ShowAllPropertiesCheckbox_Toggled;
        }

        void ShowAllPropertiesCheckbox_Toggled(object sender, EventArgs e)
        {
            UnbindEvents();

            try
            {
                ApplyPropertiesToList();
            }
            finally
            {
                BindEvents();
            }
        }

        void ParentStyleCombo_SelectionChanged(object sender, EventArgs e)
        {
            UnbindEvents();

            try
            {
                ApplyStyleCode();
            }
            finally
            {
                BindEvents();
            }
        }

        void ConfirmButton_Clicked(object sender, EventArgs e)
        {
            var props = GetSelectedProperties();

            var selectedFile = TargetFiles.FirstOrDefault(tt => tt.VirtualPath == (fileCombo.SelectedItem as string));

            var args = new XamlStyleEditedEventArgs(Platform, nameEntry.Text, targetTypeEntry.Text, targetTypeXmlnsEntry.Text, parentStyleEntry.Text, (ParentStyleType)parentStyleCombo.SelectedIndex, selectedFile?.FilePath, props);

            try
            {
                XamlStyleEdited?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            this.Close();
            this.Dispose();
        }

        void Entry_Changed(object sender, EventArgs e)
        {
            UnbindEvents();

            try
            {
                ApplyStyleCode();
            }
            finally
            {
                BindEvents();
            }
        }

        void ApplyStyleCode()
        {
            var props = GetSelectedProperties();

            var syntax = styleGenerator.GenerateSyntax(Platform, nameEntry.Text, targetTypeEntry.Text, targetTypeXmlnsEntry.Text, parentStyleEntry.Text, (ParentStyleType)parentStyleCombo.SelectedIndex, props);

            var policy = XmlFormattingPolicyService.GetXmlFormattingPolicy(ProjectIdentifier);
            var code = XmlSyntaxWriter.WriteNode(syntax, string.Empty, policy, true, true, true);

            editor.Text = code;
        }

        void PropertySearchBar_Changed(object sender, EventArgs e)
        {
            UnbindEvents();

            try
            {
                ApplyPropertiesToList();
            }
            finally
            {
                BindEvents();
            }
        }

        StyleProperty SelectedProperty
        {
            get
            {
                var row = propertiesListView.SelectedRow;

                if (row > 0)
                {
                    var key = propertiesDataStore.GetValue(row, propertyNameField);
                    return styleProperties.FirstOrDefault(p => p.Name == key);
                }

                return null;
            }
        }

        void PropertyValueCell_TextChanged(object sender, Xwt.TextChangedEventArgs e)
        {
            UnbindEvents();

            try
            {
                var row = propertiesListView.CurrentEventRow;

                if (row > 0)
                {
                    var key = propertiesDataStore.GetValue(row, propertyNameField);
                    var prop = styleProperties.FirstOrDefault(p => p.Name == key);

                    if (prop != null)
                    {
                        prop.Value = e.NewText;
                        ApplyStyleCode();
                    }
                }
            }
            finally
            {
                BindEvents();
            }
        }

        void IncludePropertyCell_Toggled(object sender, WidgetEventArgs e)
        {
            UnbindEvents();

            Task.Run(async () =>
            {
                await Task.Delay(2);

                Dispatcher.InvokeOnMainThread(() =>
               {

                   try
                   {
                       for (var row = 0; row < propertiesDataStore.RowCount; ++row)
                       {
                           var key = propertiesDataStore.GetValue(row, propertyNameField);
                           var selected = propertiesDataStore.GetValue(row, includePropertyField);
                           var prop = styleProperties.FirstOrDefault(p => p.Name == key);

                           if (prop != null)
                           {
                               prop.Selected = selected;
                           }
                       }

                       ApplyStyleCode();

                   }
                   finally
                   {
                       ApplyPropertiesToList();

                       BindEvents();
                   }
               });
            });
        }

        void BuildLeftPanel()
        {
            leftContainer = new VBox();

            leftContainer.PackStart(new Label()
            {
                Text = "Name:"
            });

            nameEntry = new TextEntry();
            leftContainer.PackStart(nameEntry);


            leftContainer.PackStart(new Label()
            {
                Text = "TargetType:"
            });

            targetTypeEntry = new TextEntry();
            leftContainer.PackStart(targetTypeEntry);

            leftContainer.PackStart(new Label()
            {
                Text = "TargetType Xmlns:"
            });

            targetTypeXmlnsEntry = new TextEntry();
            leftContainer.PackStart(targetTypeXmlnsEntry);

            leftContainer.PackStart(new Label()
            {
                Text = "File:"
            });

            fileCombo = new ComboBox();
            leftContainer.PackStart(fileCombo);

            leftContainer.PackStart(new Label()
            {
                Text = "Parent:"
            });

            parentStyleCombo = new ComboBox()
            {
                TooltipText = "When setting the parent for the style, should the parent style be set using the BasedOn or BaseResourceKey attribute?"
            };
            var items = EnumHelper.GetEnumValueDescriptions<ParentStyleType>();
            foreach (var i in items)
            {
                parentStyleCombo.Items.Add(i);
            }
            leftContainer.PackStart(parentStyleCombo);

            parentStyleEntry = new TextEntry();
            leftContainer.PackStart(parentStyleEntry);

            leftContainer.PackStart(new HSeparator());

            leftContainer.PackStart(new Label()
            {
                Text = "Properties:"
            });

            propertySearchBar = new TextEntry()
            {
                PlaceholderText = "Search by property name...",
            };
            leftContainer.PackStart(propertySearchBar);

            BuildListView();


            mainContainer.PackStart(leftContainer);
        }

        void BuildListView()
        {
            propertiesListView = new ListView();
            propertiesListView.WidthRequest = 350;

            propertiesDataStore = new ListStore(includePropertyField, propertyNameField, propertyValueField);

            includePropertyCell = new CheckBoxCellView(includePropertyField)
            {
                Editable = true,
            };

            propertyNameCell = new TextCellView(propertyNameField)
            {
                Editable = false,
            };

            propertyValueCell = new TextCellView(propertyValueField)
            {
                Editable = true,
            };

            propertiesListView.Columns.Add("", includePropertyCell);
            propertiesListView.Columns.Add("Property", propertyNameCell);
            propertiesListView.Columns.Add("Value", propertyValueCell);

            propertiesListView.DataSource = propertiesDataStore;

            leftContainer.PackStart(propertiesListView, true, true);

            showAllPropertiesCheckbox = new CheckBox("Show All Properties")
            {
                TooltipText = "Should all properties be displayed or only the properties that have been selected for extraction?",
                Active = false,
            };

            leftContainer.PackStart(showAllPropertiesCheckbox);
        }

        void BuildRightPanel()
        {
            rightContainer = new VBox();

            rightContainer.PackStart(new Label()
            {
                Text = "XAML Style"
            });

            editor = TextEditorFactory.Create();
            editor.MimeType = "application/xaml";

            rightContainer.PackStart(editor.Widget, true, true);

            mainContainer.PackStart(rightContainer, true, true);
        }
    }
}
