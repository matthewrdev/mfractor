using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MFractor.IOC;
using MFractor.Utilities;
using MFractor.VS.Mac.Utilities;
using Microsoft.VisualStudio.Text.Adornments;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;

namespace MFractor.VS.Mac
{
    class IdeStartupCommand : CommandHandler
    {
        protected override void Run()
        {
            Task.Run(() =>
            {

                try
                {
                    //ExtensionPointHelper.RenderExtensionPointHierachy("/MonoDevelop/Ide");
                    //ExtensionPointHelper.RenderExtensionPointHierachy("/MonoDevelop/Ide");

                    using (Profiler.Profile())
                    {
                        try
                        {
                            Resolver.Resolve<IBootstrapper>().Start();

                            //var factories = Resolver.ResolveAll<IViewElementFactory>();

                            //foreach (var f in factories)
                            //{
                            //    var type = f.GetType();
                            //    var attrs2 = type.CustomAttributes;
                            //    Debugger.Break();
                            //}
                        }
                        catch (Exception)
                        {
                            // MFractors MEF parts aren't yet imported when it's first installed.
                            // A resolution exception will be thrown when Resolve is used; we use this exception to detect an installation.
                            Xwt.Application.InvokeAsync(() =>
                            {
                                MessageService.ShowMessage("Thanks for installing MFractor!\n\nPlease restart Visual Studio Mac to complete the installation.");
                            }).ConfigureAwait(false);
                        }
                        finally
                        {
                            IdeApp.Exiting += OnApplicationExiting;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            });
        }

        void OnApplicationExiting(object sender, ExitEventArgs args)
        {
            try
            {
                Resolver.Resolve<IBootstrapper>().Shutdown();
            }
            catch (Exception)
            {
                // Suppress: MFractors MEF parts aren't yet imported.
            }
            finally
            {
                IdeApp.Exiting -= OnApplicationExiting;
            }
        }
    }
}
