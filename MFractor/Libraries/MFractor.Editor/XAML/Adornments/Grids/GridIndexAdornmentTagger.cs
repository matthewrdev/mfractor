using System;
using System.Collections.Generic;
using MFractor.Editor.Adornments;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Editor;

#if VS_MAC
using MFractor.VS.Mac.Adornments;
using UIElement = AppKit.NSView;
using IWpfTextView = Microsoft.VisualStudio.Text.Editor.ICocoaTextView;
using IntraTextAdornmentTag = Microsoft.VisualStudio.Text.Editor.XPlatIntraTextAdornmentTag;
#else
using System.Windows;
using MFractor.VS.Windows.Adornments;
#endif

namespace MFractor.Editor.XAML.Adornments.Grids
{
    class GridIndexAdornmentTagger : IntraTextAdornmentTagger<GridIndexTag, UIElement>, IDisposable
    {
        readonly ITagAggregator<GridIndexTag> tagger;

        internal static ITagger<IntraTextAdornmentTag> GetTagger(IWpfTextView view, Lazy<ITagAggregator<GridIndexTag>> tagger)
        {
            return view.Properties.GetOrCreateSingletonProperty(
                () => new GridIndexAdornmentTagger(view, tagger.Value));
        }

        GridIndexAdornmentTagger(IWpfTextView view, ITagAggregator<GridIndexTag> tagger)
            : base(view)
        {
            this.tagger = tagger;
        }


        // To produce adornments that don't obscure the text, the adornment tags
        // should have zero length spans. Overriding this method allows control
        // over the tag spans.
        protected override IEnumerable<Tuple<SnapshotSpan, PositionAffinity?, GridIndexTag>> GetAdornmentData(NormalizedSnapshotSpanCollection spans)
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

                var adornmentSpan = new SnapshotSpan(tagSpans[0].Start, 0);

                yield return Tuple.Create(adornmentSpan, (PositionAffinity?)PositionAffinity.Successor, dataTagSpan.Tag);
            }
        }

        protected override bool UpdateAdornment(UIElement adornment, GridIndexTag data)
        {
            if (adornment is GridIndexAdornment gridIndexAdornment)
            {
                gridIndexAdornment.SetData(data.Index, data.SampleCode);
                return true;
            }

            return true;
        }

        protected override UIElement CreateAdornment(GridIndexTag dataTag, SnapshotSpan span)
        {
            return new GridIndexAdornment(dataTag.Index, dataTag.SampleCode);
        }

        public void Dispose()
        {
            base.view.Properties.RemoveProperty(typeof(GridIndexAdornmentTagger));
        }
    }
}
