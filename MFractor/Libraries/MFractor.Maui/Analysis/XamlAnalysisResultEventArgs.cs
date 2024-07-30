﻿using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Code.Analysis;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis
{
    public class XamlAnalysisResultEventArgs : EventArgs
    {
        {
            FilePath = filePath;
            ProjectId = projectId;
            Issues = issues;
        }

        public string FilePath { get; }

        public ProjectId ProjectId { get; }

        public IReadOnlyList<ICodeIssue> Issues { get; }

        public bool HasIssues => Issues != null && Issues.Any();
    }
}