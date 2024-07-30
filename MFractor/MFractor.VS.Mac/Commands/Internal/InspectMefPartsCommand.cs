using System;
using System.Linq;
using MFractor.Commands;
using MFractor.IOC;
using Microsoft.CodeAnalysis.Host;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace MFractor.VS.Mac.Commands
{
    public class InspectMefPartsCommand : IInternalToolCommand
    {
        public void Execute(ICommandContext commandContext)
        {
            var taggers = Resolver.ResolveAll<IViewTaggerProvider>();
            var registery = Resolver.Resolve<IContentTypeRegistryService2>();

            var types = registery.ContentTypes;

            foreach (var type in types)
            {
                var mimeType = registery.GetMimeType(type);
                Console.WriteLine(type.DisplayName + " - " + type.TypeName + " - " + mimeType);
            }

            foreach (var tag in taggers)
            {
                Console.WriteLine(tag.GetType().ToString());
            }

            var languageServices = Resolver.ResolveAll<ILanguageService>();



        }

        public ICommandState GetExecutionState(ICommandContext commandContext)
        {
            return new CommandState(true, true, "Inspect MEF Parts", "");
        }
    }
}
