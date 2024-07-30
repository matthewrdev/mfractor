using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Configuration;
using MFractor.Licensing;
using MFractor.Maui.CodeGeneration.ValueConversion;
using MFractor.Maui.WorkUnits;
using MFractor.Progress;
using MFractor.Work;
using MFractor.Work.WorkUnits;

namespace MFractor.Views.ValueConverterWizard
{
    class ValueConverterWizardWorkUnitHandler : WorkUnitHandler<ValueConverterWizardWorkUnit>
    {
        [ImportingConstructor]
        public ValueConverterWizardWorkUnitHandler(Lazy<IWorkEngine> workEngine,
                                                   Lazy<IRootWindowService> rootWindowService,
                                                   Lazy<IConfigurationEngine> configurationEngine,
                                                   Lazy<ILicensingService> licensingService,
                                                   Lazy<IDispatcher> dispatcher)
        {
            this.workEngine = workEngine;
            this.rootWindowService = rootWindowService;
            this.configurationEngine = configurationEngine;
            this.licensingService = licensingService;
            this.dispatcher = dispatcher;
        }

        readonly Lazy<IWorkEngine> workEngine;
        IWorkEngine  WorkEngine => workEngine.Value;

        readonly Lazy<IRootWindowService> rootWindowService;
        IRootWindowService RootWindowService => rootWindowService.Value;

        readonly Lazy<IConfigurationEngine> configurationEngine;
        IConfigurationEngine ConfigurationEngine => configurationEngine.Value;

        readonly Lazy<ILicensingService> licensingService;
        ILicensingService LicensingService => licensingService.Value;

        readonly Lazy<IDispatcher> dispatcher;
        public IDispatcher Dispatcher => dispatcher.Value;

        public override Task<IWorkExecutionResult> OnExecute(ValueConverterWizardWorkUnit workUnit, IProgressMonitor progressMonitor)
        {
            Dispatcher.InvokeOnMainThread(() =>
           {
               var dialog = new ValueConverterWizard(workUnit.TargetProject, workUnit.Platform);
               dialog.Apply(workUnit.ValueConverterName, workUnit.InputType, workUnit.OutputType);
               dialog.SetXamlEntryTargetFiles(workUnit.CreateXamlDeclaration, workUnit.TargetFiles);

               dialog.OnGenerate += async (sender, e) =>
               {
                   if (!LicensingService.IsPaid)
                   {
                       await WorkEngine.ApplyAsync(new RequestTrialPromptWorkUnit($"The Value Converter Wizard is a Professional-only MFractor feature. Please upgrade or request a trial.", "Value Converter Wizard"));
                       return;
                   }

                   var options = e.Options;
                   var id = ConfigurationId.Create(options.Project);

                   var valueConverterGenerator = ConfigurationEngine.Resolve<IValueConverterGenerator>(id);

                   var workUnits = valueConverterGenerator.Generate(options).ToList();

                   dialog.Close();
                   dialog.Dispose();

                   var typeName = valueConverterGenerator.GetConverterTypeName(options);

                   if (workUnit.OnConverterGenerated != null)
                   {
                       workUnits.AddRange(workUnit.OnConverterGenerated.Invoke(typeName));
                   }

                   await WorkEngine.ApplyAsync(workUnits);
               };

               dialog.InitialLocation = Xwt.WindowLocation.CenterParent;
               dialog.Run(RootWindowService.RootWindowFrame);
           });

            return default;
        }
    }
}
