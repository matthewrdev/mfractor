using System;
using System.Collections.Generic;
using MFractor.Editor.Adornments;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text.Editor;

#if VS_MAC
using UIElement = AppKit.NSView;
using IWpfTextView = Microsoft.VisualStudio.Text.Editor.ICocoaTextView;
using IntraTextAdornmentTag = Microsoft.VisualStudio.Text.Editor.XPlatIntraTextAdornmentTag;
using MFractor.VS.Mac.Adornments;
#else
using System.Windows;
using MFractor.VS.Windows.Adornments;
#endif

namespace MFractor.Editor.XAML.Adornments.EscapedXamlCharacters
{
    class EscapedXamlCharacterAdornmentTagger : IntraTextAdornmentTagger<EscapedCharacterTag, UIElement>, IDisposable
    {
        readonly ITagAggregator<EscapedCharacterTag> tagger;

        internal static ITagger<IntraTextAdornmentTag> GetTagger(IWpfTextView view, Lazy<ITagAggregator<EscapedCharacterTag>> tagger)
        {
            return view.Properties.GetOrCreateSingletonProperty(
                () => new EscapedXamlCharacterAdornmentTagger(view, tagger.Value));
        }

        EscapedXamlCharacterAdornmentTagger(IWpfTextView view, ITagAggregator<EscapedCharacterTag> tagger)
            : base(view)
        {
            this.tagger = tagger;
        }

        // To produce adornments that don't obscure the text, the adornment tags
        // should have zero length spans. Overriding this method allows control
        // over the tag spans.
        protected override IEnumerable<Tuple<SnapshotSpan, PositionAffinity?, EscapedCharacterTag>> GetAdornmentData(NormalizedSnapshotSpanCollection spans)
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

        protected override bool UpdateAdornment(UIElement adornment, EscapedCharacterTag data)
        {
            if (adornment is LabelAdornment label)
            {
                label.SetData(data.Character);
                return true;
            }

            return true;
        }

        protected override UIElement CreateAdornment(EscapedCharacterTag dataTag, SnapshotSpan span)
        {
            return new LabelAdornment(dataTag.Character);
        }

        public void Dispose()
        {
            base.view.Properties.RemoveProperty(typeof(EscapedXamlCharacterAdornmentTagger));
        }
    }
}
