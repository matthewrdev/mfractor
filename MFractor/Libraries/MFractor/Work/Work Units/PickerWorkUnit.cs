using System;
using System.Collections.Generic;

namespace MFractor.Work.WorkUnits
{
    public delegate void PickerResultDelegate(object selection);

    public class PickerWorkUnit : WorkUnit
    {
        public string Title { get; set; }

        public string Message { get; set; }

        public string Confirm { get; set; } = "Confirm";

        public string Cancel { get; set; } = "Cancel";

        public IReadOnlyDictionary<string, object> Choices { get; set; }

        public PickerResultDelegate Delegate { get; set; }

        public string HelpUrl { get; set; }
    }
}
