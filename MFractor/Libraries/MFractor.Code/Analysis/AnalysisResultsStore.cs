using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code.Documents;

namespace MFractor.Code.Analysis
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IAnalysisResultStore))]
    class AnalysisResultsStore : IAnalysisResultStore
    {
        readonly object storeLock = new object();
        readonly Dictionary<string, List<ICodeIssue>> store = new Dictionary<string, List<ICodeIssue>>();

        public void ClearAll()
        {
            lock (storeLock)
            {
                store.Clear();
            }
        }

        public IReadOnlyList<ICodeIssue> Retrieve(IParsedXmlDocument document)
        {
            if (document is null)
            {
                return Array.Empty<ICodeIssue>();
            }

            return Retrieve(document.FilePath);
        }

        public IReadOnlyList<ICodeIssue> Retrieve(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return Array.Empty<ICodeIssue>();
            }

            lock (storeLock)
            {
                if (store.ContainsKey(filePath) == false)
                {
                    return Array.Empty<ICodeIssue>();
                }

                return store[filePath];
            }
        }

        public void Store(IParsedXmlDocument document, IReadOnlyList<ICodeIssue> results)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (results is null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            Store(document.FilePath, results);
        }

        public void Store(string filePath, IReadOnlyList<ICodeIssue> results)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("message", nameof(filePath));
            }

            if (results is null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            lock (storeLock)
            {
                store[filePath] = results.ToList();
            }
        }

        public void Clear(IParsedXmlDocument document)
        {
            if (document is null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            Clear(document.FilePath);
        }

        public void Clear(string filePath)
        {
            lock (storeLock)
            {
                store.Remove(filePath);
            }
        }

        public int Count
        {
            get
            {
                lock (storeLock)
                {
                    return store.Count;
                }
            }
        }
    }
}

