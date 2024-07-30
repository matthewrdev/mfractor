using System;
using System.Collections.Generic;
using MFractor.Editor.Adornments;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.CodeAnalysis.Host;

#if VS_MAC
using MFractor.VS.Mac.Adornments;
using UIElement = AppKit.NSView;
using IWpfTextView = Microsoft.VisualStudio.Text.Editor.ICocoaTextView;
using IntraTextAdornmentTag = Microsoft.VisualStudio.Text.Editor.XPlatIntraTextAdornmentTag;
#else
using System.Windows;
using MFractor.VS.Windows.Adornments;
using Microsoft.VisualStudio.Text.Editor;
#endif

namespace MFractor.Editor.XAML.Adornments.Colors
{
    class ColorAdornmentTagger : IntraTextAdornmentTagger<ColorTag, UIElement>, IDisposable
    {
        readonly ITagAggregator<ColorTag> tagger;

        internal static ITagger<IntraTextAdornmentTag> GetTagger(IWpfTextView view, Lazy<ITagAggregator<ColorTag>> tagger)
        {
            return view.Properties.GetOrCreateSingletonProperty(
                () => new ColorAdornmentTagger(view, tagger.Value));
        }

        ColorAdornmentTagger(IWpfTextView view, ITagAggregator<ColorTag> tagger)
            : base(view)
        {
            this.tagger = tagger;
        }


        // To produce adornments that don't obscure the text, the adornment tags
        // should have zero length spans. Overriding this method allows control
        // over the tag spans.
        protected override IEnumerable<Tuple<SnapshotSpan, PositionAffinity?, ColorTag>> GetAdornmentData(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0)
            {
                yield break;
            }

            var snapshot = spans[0].Snapshot;

            var tags = tagger.GetTags(spans);

            foreach (var dataTagSpan in tags)
            {
                var tagSpans = dataTagSpan.Span.GetSpans(snapshot);

                // Ignore data tags that are split by projection.
                // This is theoretically possible but unlikely in current scenarios.
                if (tagSpans.Count != 1)
                {
                    continue;
                }

                var adornmentSpan = new SnapshotSpan(tagSpans[0].End, 0);

                yield return Tuple.Create(adornmentSpan, (PositionAffinity?)PositionAffinity.Successor, dataTagSpan.Tag);
            }
        }

        protected override bool UpdateAdornment(UIElement adornment, ColorTag data)
        {
            if (adornment is ColorAdornment colorAdornment)
            {
                colorAdornment.Update(data.Color, data.ColorEditedDelegate);
                return true;
            }

            return true;
        }

        protected override UIElement CreateAdornment(ColorTag dataTag, SnapshotSpan span)
        {
            return new ColorAdornment(dataTag.Color, dataTag.ColorEditedDelegate);
        }

        public void Dispose()
        {
            base.view.Properties.RemoveProperty(typeof(ColorAdornmentTagger));
        }
    }
}