using System;
using System.ComponentModel.Composition;
using System.IO;
using MFractor.Text;

namespace MFractor.Text
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILineCollectionFactory))]
    class LineCollectionFactory : ILineCollectionFactory
    {
        readonly Lazy<ITextProviderService> textProviderService;
        public ITextProviderService TextProviderService => textProviderService.Value;

        [ImportingConstructor]
        public LineCollectionFactory(Lazy<ITextProviderService> textProviderService)
        {
            this.textProviderService = textProviderService;
        }

        public ILineCollection Create(string content)
        {
            return new LineCollection(content);
        }

        public ILineCollection Create(Stream content)
        {
            if (content is null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            using (var reader = new StreamReader(content))
            {
                return Create(reader.ReadToEnd());
            }
        }

        public ILineCollection Create(ITextProvider textProvider)
        {
            if (textProvider is null)
            {
                return null;
            }

            return Create(textProvider.GetText());
        }

        public ILineCollection Create(FileInfo fileInfo)
        {
            if (fileInfo is null)
            {
                return null;
            }

            var provider = TextProviderService.GetTextProvider(fileInfo.FullName);

            return Create(provider);
        }
    }
}