using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Configuration;
using MFractor.CSharp.CodeGeneration.ClassFromClipboard;
using MFractor.CSharp.WorkUnits;
using MFractor.IOC;
using MFractor.Licensing;
using MFractor.Utilities;
using MFractor.Views.Branding;
using MFractor.Views.Controls;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Xwt;

namespace MFractor.Views.ClassFromClipboard
{
    public class CreateClassFromContentDialog : Dialog
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        [Import]
        public IWorkEngine WorkEngine { get; set; }

        [Import]
        public ILicensingService LicensingService { get; set; }

        [Import]
        public IConfigurationEngine ConfigurationEngine { get; set; }

        [Import]
        public IClipboard Clipboard { get; set; }

        [Import]
        public IDispatcher Dispatcher { get; set; }

        [Import]
        public IClassFromStringContentGenerator ClassFromStringContentGenerator { get; set; }

        VBox root;

        HBox mainContainer;

        VBox leftPanel;

        Label description;
        Label nameLabel;
        TextEntry nameEntry;
        Label folderLabel;
        TextEntry folderEntry;

        Label namespaceLabel;
        ComboBox namespaceKindCombo; // Automatic, Custom, Preserve Original
        TextEntry namespaceEntry;

        public event EventHandler<WorkUnitEventArgs> OnApplyResult;

        Button generateButton;

        WorkUnitPreviewControl workUnitPreviewControl;

        DateTime displayedTime = DateTime.UtcNow;
        BrandedFooter footer;

        public CreateClassFromContentWorkUnit WorkUnit { get; private set; }

        public CreateClassFromContentDialog()
        {
            Resolver.ComposeParts(this);

            Width = 960;
            Height = 640;

            Build();

            BindEvents();
        }

        void Build()
        {
            root = new VBox();
            mainContainer = new HBox();

            BuildLeftPanel();

            BuildRightPanel();

            root.PackStart(mainContainer, true, true);

            generateButton = new Button("Generate");
            generateButton.Clicked += GenerateButton_Clicked;

            root.PackStart(generateButton);

            footer = new BrandedFooter();

            root.PackStart(new HSeparator());
            root.PackStart(new BrandedFooter());

            Content = root;
        }

        public void SetWorkUnit(CreateClassFromContentWorkUnit workUnit)
        {
            Title = "Create Class From Clipboard";

            WorkUnit = workUnit;
            description.Text = "Create a new class file using the contents of the clipbaord";
            nameEntry.Text = workUnit.DefaultClassName;
            folderEntry.Text = WorkUnit.FolderPath;

            footer.HelpUrl = "https://docs.mfractor.com/csharp/create-class-from-clipboard";

            CreateOutputPreviewAsync().ConfigureAwait(false);
        }

        void BindEvents()
        {
            UnbindEvents();

            nameEntry.KeyReleased += NameEntry_KeyReleased;
            folderEntry.KeyReleased += FolderEntry_KeyReleased;
            namespaceKindCombo.SelectionChanged += NamespaceKindCombo_SelectionChanged;
            namespaceEntry.KeyReleased += NamespaceEntry_KeyReleased;
        }

        void CodeFilesSelectionComboBox_SelectionChanged(object sender, EventArgs e)
        {
            CreateOutputPreviewAsync().ConfigureAwait(false);
        }

        void UnbindEvents()
        {
            nameEntry.KeyReleased -= NameEntry_KeyReleased;
            folderEntry.KeyReleased -= FolderEntry_KeyReleased;
            namespaceKindCombo.SelectionChanged -= NamespaceKindCombo_SelectionChanged;
            namespaceEntry.KeyReleased += NamespaceEntry_KeyReleased;
        }

        void NamespaceEntry_KeyReleased(object sender, KeyEventArgs e)
        {
            ThrottledCreateOutputPreviewAsync().ConfigureAwait(false);
        }

        void NamespaceKindCombo_SelectionChanged(object sender, EventArgs e)
        {
            ThrottledCreateOutputPreviewAsync().ConfigureAwait(false);
        }

        CancellationTokenSource cancellationTokenSource;

        Task ThrottledCreateOutputPreviewAsync()
        {
            return Task.Run(async () =>
           {

               if (cancellationTokenSource != null)
               {
                   cancellationTokenSource.Cancel();
               }

               cancellationTokenSource = new CancellationTokenSource();
               var cancellationToken = cancellationTokenSource.Token;

               await Task.Delay(100);

               if (!cancellationToken.IsCancellationRequested)
               {
                   await CreateOutputPreviewAsync();
               }
           });
        }

        async Task CreateOutputPreviewAsync()
        {
            UnbindEvents();

            try
            {
                Dispatcher.InvokeOnMainThread(() =>
                {
                    workUnitPreviewControl.IsWorking = true;
                });

                var workUnits = Enumerable.Empty<IWorkUnit>();

                if (WorkUnit != null)
                {
                    var source = new TaskCompletionSource<CreateClassFromClipboardOptions>();

                    Dispatcher.InvokeOnMainThread(() =>
                    {
                        source.TrySetResult(this.GetOptions());
                    });

                    var options = await source.Task;

                    workUnits = this.ClassFromStringContentGenerator.Generate(options, WorkUnit.Content);
                }

                Dispatcher.InvokeOnMainThread(() =>
               {
                   workUnitPreviewControl.WorkUnits = workUnits.ToList();
               });
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
            finally
            {
                workUnitPreviewControl.IsWorking = false;
                BindEvents();
            }
        }

        CreateClassFromClipboardOptions GetOptions()
        {
            return new CreateClassFromClipboardOptions(nameEntry.Text, WorkUnit.Project, folderEntry.Text, (NamespaceMode)((int)namespaceKindCombo.SelectedItem), namespaceEntry.Text);
        }

        void FolderEntry_KeyReleased(object sender, KeyEventArgs e)
        {
            ThrottledCreateOutputPreviewAsync().ConfigureAwait(false);
        }

        void BuildRightPanel()
        {
            workUnitPreviewControl = new WorkUnitPreviewControl();
            workUnitPreviewControl.WidthRequest = 600;

            mainContainer.PackStart(new HSeparator());

            mainContainer.PackStart(workUnitPreviewControl, true, true);
        }

        void BuildLeftPanel()
        {
            leftPanel = new VBox();

            description = new Label();

            nameLabel = new Label()
            {
                Markup = "<b>Name:</b>"
            };

            nameEntry = new TextEntry()
            {
                PlaceholderText = "Enter a name for the new class",
            };

            folderLabel = new Label()
            {
                Markup = "<b>Folder:</b>"
            };

            folderEntry = new TextEntry()
            {
                PlaceholderText = "Enter the folder for the new file.",
                TooltipText = "The folder, relative to the projects root, that the new file will be placed into. If this folder path does not exist, it will be created.",
            };


            namespaceLabel = new Label()
            {
                Markup = "<b>Namespace Options:</b>"
            };

            namespaceKindCombo = new ComboBox();

            var values = EnumHelper.GetDisplayValues<NamespaceMode>();

            foreach (var value in values)
            {
                namespaceKindCombo.Items.Add(value.Item2, value.Item1);
            }

            namespaceKindCombo.SelectedIndex = 0; // Automatic

            namespaceEntry = new TextEntry()
            {
                PlaceholderText = "Enter a custom namespace value",
            };

            leftPanel.PackStart(description);

            leftPanel.PackStart(nameLabel);
            leftPanel.PackStart(nameEntry);

            leftPanel.PackStart(folderLabel);
            leftPanel.PackStart(folderEntry);

            leftPanel.PackStart(namespaceLabel);
            leftPanel.PackStart(namespaceKindCombo);
            leftPanel.PackStart(namespaceEntry);

            mainContainer.PackStart(leftPanel);
        }


        void NameEntry_KeyReleased(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return
                && DateTime.UtcNow - displayedTime > TimeSpan.FromSeconds(2)) // Don't allow instant commit.
            {
                Submit();
            }

            ThrottledCreateOutputPreviewAsync().ConfigureAwait(false);
        }

        void GenerateButton_Clicked(object sender, EventArgs e)
        {
            Submit();
        }

        void Submit()
        {
            if (!LicensingService.IsPaid)
            {
                WorkEngine.ApplyAsync(new RequestTrialPromptWorkUnit($"{Title} is a Professional-only MFractor feature. Please upgrade or request a trial.", "Font Importer")).ConfigureAwait(false);
                return;
            }

            var options = GetOptions();

            var units = ClassFromStringContentGenerator.Generate(options, WorkUnit.Content);

            OnApplyResult?.Invoke(this, new WorkUnitEventArgs(units));
            this.Close();
        }
    }
}
