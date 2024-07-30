using System;
using Microsoft.CodeAnalysis.Text;
using MFractor.Localisation.Data.Models;
using MFractor.Workspace;

namespace MFractor.Localisation
{
    class LocalisationDeclaration : ILocalisationDeclaration
    {
        public LocalisationDeclaration(ResXLocalisationEntry entry, string key, IProjectFile projectFile)
        {
            if (entry is null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("message", nameof(key));
            }

            Key = key;
            ProjectFile = projectFile ?? throw new ArgumentNullException(nameof(projectFile));
            Value = entry.Value;
            CultureCode = entry.CultureCode;
            KeySpan = entry.KeySpan;
            ValueSpan = entry.ValueSpan;
        }

        public IProjectFile ProjectFile { get; }

        public string Key { get; }

        public string Value { get; }

        public string CultureCode { get; }

        public TextSpan KeySpan { get; }

        public TextSpan ValueSpan { get; }
    }
}