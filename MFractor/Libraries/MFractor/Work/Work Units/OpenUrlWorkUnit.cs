using System;

namespace MFractor.Work.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> that launches the provided <see cref="Uri"/>
    /// </summary>
    public class OpenUrlWorkUnit : WorkUnit
    {
        public OpenUrlWorkUnit(Uri uri, bool addUtmSource = true)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            AddUtmSource = addUtmSource;
        }

        public OpenUrlWorkUnit(string url, bool addUtmSource = true)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("message", nameof(url));
            }

            Uri = new Uri(url);
            AddUtmSource = addUtmSource;
        }

        public Uri Uri { get; set; }

        public bool AddUtmSource { get; set; }
    }
}