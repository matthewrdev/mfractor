using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using EnvDTE80;
using MFractor.Android.Commands;
using MFractor.Code.Scaffolding;
using MFractor.Commands.MainMenu;
using MFractor.Commands.MainMenu.About;
using MFractor.Commands.MainMenu.Support;
using MFractor.CSharp.Commands;
using MFractor.Fonts.Commands;
using MFractor.Ide.Commands;
using MFractor.Ide.Commands.Navigation;
using MFractor.Ide.Commands.SolutionPad;
using MFractor.Images.Commands;
using MFractor.IOC;
using MFractor.iOS.Commands;
using MFractor.Localisation.Commands;
using MFractor.Maui.Commands.Fonts;
using MFractor.Maui.Commands.Wizards;
using MFractor.VS.Windows.Commands;
using MFractor.VS.Windows.UI.Options;
using MFractor.VS.Windows.UI.OptionsPage;
using MFractor.VS.Windows.Utilities;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace MFractor.VS.Windows
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideToolWindow(typeof(ToolWindows.ImageAssetsToolWindow))]
    [ProvideOptionPage(typeof(SettingsPage), "MFractor", "Settings", 0, 0, true, Sort = 0)]
    [ProvideOptionPage(typeof(CodeAnalysisPage), "MFractor", "Code Analysis", 0, 0, true, Sort = 1)]
    [ProvideOptionPage(typeof(FormattingPage), "MFractor", "Formatting", 0, 0, true, Sort = 2)]
    [ProvideOptionPage(typeof(DeleteOutputOptions), "MFractor", "Delete Output Folders", 0, 0, true, Sort = 3)]
    public sealed class MFractorPackage : AsyncPackage
    {
        Logging.ILogger log = Logging.Logger.Create();

        DTE2 DTE { get; } = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

        // Maintain a strong reference to prevent GCing.
        public DTEEvents DTEEvents { get; private set; }

        /// <summary>
        /// MFractor.VSWindowsPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "9772085d-2cd1-46f3-aa46-c62ee90fefec";

        public MFractorPackage()
        {
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            try
            {
                // Setup any precached environment information.
                IdeEnvironmentDetails.Prepare();

                // Start the XWT framework to support xplat UIs.
                Xwt.Application.Initialize();

                DTEEvents = DTE.Events.DTEEvents;
                DTEEvents.OnBeginShutdown += DTEEvents_OnBeginShutdown;

                // Start MFractor.
                Resolver.Resolve<IBootstrapper>().Start();

                // Bind all MFractor commands into the IDE shell.
                await InitializeCommandsAsync();
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void DTEEvents_OnBeginShutdown()
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                DTEEvents.OnBeginShutdown -= DTEEvents_OnBeginShutdown;
                DTEEvents = null;
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            try
            {
                log?.Info("MFractor is shutting down");

                Resolver.Resolve<IBootstrapper>().Shutdown();
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        async Task InitializeCommandsAsync()
        {
            log?.Info("Initialising and binding MFractors commands into Visual Studio Windows.");

            var commandService = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

            InitaliseMenuCommands(commandService);

            InitialiseLicensingCommands(commandService);

            InitialiseImportCommands(commandService);

            InitialiseSupportCommands(commandService);

            InitialiseAboutCommands(commandService);

            InitialiseLegalCommands(commandService);

            InitialiseWizardCommands(commandService);

            InitialiseSolutionPadCommands(commandService);

            InitialiseNavigationCommands(commandService);
        }

        void InitaliseMenuCommands(OleMenuCommandService commandService)
        {
            IdeCommandAdapter.BindMenuCommand<ILaunchImageManagerCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_ManageImageAssets);
            IdeCommandAdapter.BindMenuCommand<PreferencesCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_Preferences);
            IdeCommandAdapter.BindMenuCommand<ResyncSolutionResources>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_ResyncMFractor);
        }

        void InitialiseSolutionPadCommands(OleMenuCommandService commandService)
        {
            IdeCommandAdapter.BindSolutionPadCommand<DeleteImageAssetCommand>(commandService, PackageGuids.guidMFractorSolutionCmdSet, PackageIds.MFractor_Commands_Solution_DeleteImageAsset);
            IdeCommandAdapter.BindSolutionPadCommand<DeleteOutputFoldersCommand>(commandService, PackageGuids.guidMFractorSolutionCmdSet, PackageIds.MFractor_Commands_Solution_DeleteOutputFolders);
            IdeCommandAdapter.BindSolutionPadCommand<GenerateFontGlyphClassCommand>(commandService, PackageGuids.guidMFractorSolutionCmdSet, PackageIds.MFractor_Commands_Solution_GenerateFontGlyphCodeClass);
            IdeCommandAdapter.BindSolutionPadCommand<CleanAndCompressCommand>(commandService, PackageGuids.guidMFractorSolutionCmdSet, PackageIds.MFractor_Commands_Solution_CleanAndCompress);
            IdeCommandAdapter.BindSolutionPadCommand<ILaunchImageManagerCommand>(commandService, PackageGuids.guidMFractorSolutionCmdSet, PackageIds.MFractor_Commands_Solution_ManageImageAssets);
            IdeCommandAdapter.BindSolutionPadCommand<CreateClassFromClipboardCommand>(commandService, PackageGuids.guidMFractorSolutionCmdSet, PackageIds.MFractor_Commands_Solution_CreateClassFromClipboard);
            IdeCommandAdapter.BindSolutionPadCommand<ScaffolderCommand>(commandService, PackageGuids.guidMFractorSolutionCmdSet, PackageIds.MFractor_Commands_Solution_Scaffold);
            IdeCommandAdapter.BindSolutionPadCommand<CopyPackageNameToClipboardCommand>(commandService, PackageGuids.guidMFractorSolutionCmdSet, PackageIds.MFractor_Commands_Solution_CopyAndroidPackageId);
            IdeCommandAdapter.BindSolutionPadCommand<CopyBundleIdToClipboardCommand>(commandService, PackageGuids.guidMFractorSolutionCmdSet, PackageIds.MFractor_Commands_Solution_CopyiOSBundleId);
            IdeCommandAdapter.BindSolutionPadCommand<AddExportFontDeclarationCommand>(commandService, PackageGuids.guidMFractorSolutionCmdSet, PackageIds.MFractor_Commands_Solution_AddExportFontDeclaration);
            IdeCommandAdapter.BindSolutionPadCommand<OpenAndroidManifestCommand>(commandService, PackageGuids.guidMFractorSolutionCmdSet, PackageIds.MFractor_Commands_Solution_OpenAndroidManifest);
            IdeCommandAdapter.BindSolutionPadCommand<ConvertProjectToCSharp9Command>(commandService, PackageGuids.guidMFractorSolutionCmdSet, PackageIds.MFractor_Commands_Solution_ConvertToCSharp9);
        }

        void InitialiseNavigationCommands(OleMenuCommandService commandService)
        {
            IdeCommandAdapter.BindActiveDocumentCommand<GoToRelationalImplementationCommand>(commandService, PackageGuids.guidMFractorNavigationCmdSet, PackageIds.MFractor_Commands_Navigation_GoToRelationalImplementation);
            IdeCommandAdapter.BindActiveDocumentCommand<GoToRelationalDefinitionCodeBehindCommand>(commandService, PackageGuids.guidMFractorNavigationCmdSet, PackageIds.MFractor_Commands_Navigation_GoToRelationalDefinitionCodeBehind);
            IdeCommandAdapter.BindActiveDocumentCommand<GoToRelationalDefinitionCommand>(commandService, PackageGuids.guidMFractorNavigationCmdSet, PackageIds.MFractor_Commands_Navigation_GoToRelationalDefinition);

            IdeCommandAdapter.BindActiveDocumentCommand<MFractor.Maui.Commands.Navigation.GoToXamlSymbolCommand>(commandService, PackageGuids.guidMFractorNavigationCmdSet, PackageIds.MFractor_Commands_Navigation_GoToXamlSymbol);
        }

        void InitialiseWizardCommands(OleMenuCommandService commandService)
        {
            IdeCommandAdapter.BindMenuCommand<LocalisationWizardCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_Wizards_LocalizeDocument);
            IdeCommandAdapter.BindMenuCommand<MVVMWizardCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_Wizards_Mvvm);
            IdeCommandAdapter.BindMenuCommand<ValueConverterWizardCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_Wizards_ValueConverter);
        }

        void InitialiseLicensingCommands(OleMenuCommandService commandService)
        {
            IdeCommandAdapter.BindMenuCommand<LicenseSummaryCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_LicenseSummary);
            IdeCommandAdapter.BindMenuCommand<ViewLicenseCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_LicenseInfo);
            IdeCommandAdapter.BindMenuCommand<PurchaseCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_Buy);
            IdeCommandAdapter.BindMenuCommand<RecoverLicenseCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_RecoverLicense);
        }

        void InitialiseImportCommands(OleMenuCommandService commandService)
        {
            // Main menu
            IdeCommandAdapter.BindMenuCommand<ImportFontCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_ImportFont);
            IdeCommandAdapter.BindMenuCommand<ImportImageAssetCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_ImportImageAsset);
            IdeCommandAdapter.BindMenuCommand<ImportIconWizardCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_ImportIconWizard);

            // Solution Explorer
            IdeCommandAdapter.BindSolutionPadCommand<ImportFontCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_ImportFont_SolutionExplorer);
            IdeCommandAdapter.BindSolutionPadCommand<ImportImageAssetCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_ImportImageAsset_SolutionExplorer);
            IdeCommandAdapter.BindSolutionPadCommand<ImportIconWizardCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_ImportIconWizard_SolutionExplorer);
        }

        void InitialiseAboutCommands(OleMenuCommandService commandService)
        {
            IdeCommandAdapter.BindMenuCommand<AboutProductCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_About);
            IdeCommandAdapter.BindMenuCommand<OnboardingCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_Onboarding);
        }

        void InitialiseLegalCommands(OleMenuCommandService commandService)
        {
            IdeCommandAdapter.BindMenuCommand<PrivacyPolicyCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_PrivacyPolicy);
            IdeCommandAdapter.BindMenuCommand<EndUserLicenseCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_EndUserLicense);
            IdeCommandAdapter.BindMenuCommand<ThirdPartyAttributionCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_ThirdParty);
        }

        void InitialiseSupportCommands(OleMenuCommandService commandService)
        {
            IdeCommandAdapter.BindMenuCommand<SlackSupportCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_Support_Slack);
            IdeCommandAdapter.BindMenuCommand<GitterSupportCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_Support_Gitter);
            IdeCommandAdapter.BindMenuCommand<TwitterSupportCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_Support_Twitter);
            IdeCommandAdapter.BindMenuCommand<EmailSupportCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_Support_Email);
            IdeCommandAdapter.BindMenuCommand<SubmitFeedbackCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_Support_SubmitFeedback);
            IdeCommandAdapter.BindMenuCommand<DocumentationSupportCommand>(commandService, PackageGuids.guidMFractorTopMenuCmdSet, PackageIds.MFractor_Commands_Support_Documentation);
        }

        #endregion
    }
}
