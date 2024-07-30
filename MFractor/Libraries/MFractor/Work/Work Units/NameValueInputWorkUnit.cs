using System;
using System.Collections.Generic;

namespace MFractor.Work.WorkUnits
{
    public delegate IReadOnlyList<IWorkUnit> NameValueInputDelegate(string name, string value);

    /// <summary>
    /// An <see cref="IWorkUnit"/> that launches a name-value input dialog.
    /// </summary>
    public class NameValueInputWorkUnit : WorkUnit
    {
        public string Title { get; set; }

        public string Message { get; set; }

        public string NameLabel { get; set; }

        public string Name { get; set; }

        public string NamePlaceholder { get; set; }

        public string ValueLabel { get; set; }

        public string Value { get; set; }

        public string ValuePlaceholder { get; set; }

        public string ConfirmLabel { get; set; }

        public string CancelLabel { get; set; }

        public string HelpUrl { get; set; }

        public NameValueInputDelegate NameValueInputDelegate { get; set; }
    }
}