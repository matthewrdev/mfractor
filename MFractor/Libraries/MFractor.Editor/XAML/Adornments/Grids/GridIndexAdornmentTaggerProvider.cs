﻿using System;
using System.ComponentModel.Composition;
using MFractor.Licensing;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using MFractor.Editor.Adornments;
using MFractor.Ide;

#if VS_MAC
using IWpfTextView = Microsoft.VisualStudio.Text.Editor.ICocoaTextView;
using IntraTextAdornmentTag = Microsoft.VisualStudio.Text.Editor.XPlatIntraTextAdornmentTag;
#else
#endif

namespace MFractor.Editor.XAML.Adornments.Grids
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IntraTextAdornmentTag))]
    [ContentType(ContentTypes.Xaml)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TextViewRole(PredefinedTextViewRoles.Analyzable)]
    class GridIndexAdornmentTaggerProvider : IViewTaggerProvider
    {
        readonly Lazy<ILicensingService> licensingService;
        public ILicensingService LicensingService => licensingService.Value;

        readonly Lazy<IIdeFeatureSettings> featureSettings;
        public IIdeFeatureSettings FeatureSettings => featureSettings.Value;

        readonly Lazy<IBufferTagAggregatorFactoryService> bufferTagAggregatorFactoryService;
        public IBufferTagAggregatorFactoryService BufferTagAggregatorFactoryService => bufferTagAggregatorFactoryService.Value;

        [ImportingConstructor]
        internal GridIndexAdornmentTaggerProvider(Lazy<ILicensingService> licensingService,
                                                  Lazy<IIdeFeatureSettings> featureSettings,
                                                  Lazy<IBufferTagAggregatorFactoryService> bufferTagAggregatorFactoryService)
        {
            this.licensingService = licensingService;
            this.featureSettings = featureSettings;
            this.bufferTagAggregatorFactoryService = bufferTagAggregatorFactoryService;
        }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (!FeatureSettings.AllowGridAdornments)
            {
                return null;
            }

            if (!LicensingService.IsPaid)
            {
                return null;
            }

            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            return GridIndexAdornmentTagger.GetTagger(
                (IWpfTextView)textView,
                new Lazy<ITagAggregator<GridIndexTag>>(
                    () => BufferTagAggregatorFactoryService.CreateTagAggregator<GridIndexTag>(textView.TextBuffer)))
                as ITagger<T>;
        }
    }
}
