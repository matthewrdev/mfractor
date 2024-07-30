// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
// This file has been copied from the Roslyn repository at: https://github.com/dotnet/roslyn/blob/master/src/EditorFeatures/Core/Shared/Extensions/ITextViewExtensions.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Utilities;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Editor.Shared.Extensions
{
    internal static partial class ITextViewExtensions
    {
        public static SnapshotPoint? GetPositionInView(this ITextView textView, SnapshotPoint point)
        {
            return textView.BufferGraph.MapUpToSnapshot(point, PointTrackingMode.Positive, PositionAffinity.Successor, textView.TextSnapshot);
        }

        public static bool TryMoveCaretToAndEnsureVisible(this ITextView textView, SnapshotPoint point, IOutliningManagerService outliningManagerService = null, EnsureSpanVisibleOptions ensureSpanVisibleOptions = EnsureSpanVisibleOptions.None)
        {
            return textView.TryMoveCaretToAndEnsureVisible(new VirtualSnapshotPoint(point), outliningManagerService, ensureSpanVisibleOptions);
        }

        public static bool TryMoveCaretToAndEnsureVisible(this ITextView textView, VirtualSnapshotPoint point, IOutliningManagerService outliningManagerService = null, EnsureSpanVisibleOptions ensureSpanVisibleOptions = EnsureSpanVisibleOptions.None)
        {
            if (textView.IsClosed)
            {
                return false;
            }

            var pointInView = textView.GetPositionInView(point.Position);

            if (!pointInView.HasValue)
            {
                return false;
            }

            // If we were given an outlining service, we need to expand any outlines first, or else
            // the Caret.MoveTo won't land in the correct location if our target is inside a
            // collapsed outline.
            if (outliningManagerService != null)
            {
                var outliningManager = outliningManagerService.GetOutliningManager(textView);

                if (outliningManager != null)
                {
                    outliningManager.ExpandAll(new SnapshotSpan(pointInView.Value, length: 0), match: _ => true);
                }
            }

            var newPosition = textView.Caret.MoveTo(new VirtualSnapshotPoint(pointInView.Value, point.VirtualSpaces));

            // We use the caret's position in the view's current snapshot here in case something 
            // changed text in workUnit to a caret move (e.g. line commit)
            var spanInView = new SnapshotSpan(newPosition.BufferPosition, 0);
            textView.ViewScroller.EnsureSpanVisible(spanInView, ensureSpanVisibleOptions);

            return true;
        }
    }
}