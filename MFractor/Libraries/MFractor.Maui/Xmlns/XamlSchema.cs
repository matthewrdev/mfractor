namespace MFractor.Maui.Xmlns
{
    /// <summary>
    /// Xaml schema.
    /// </summary>
    public class XamlSchema : IXamlSchema
    {
        /// <summary>
        /// The URI 
        /// </summary>
        /// <value>The URI.</value>
		public string Uri { get; }

        public XamlSchema(string uri)
        {
            Uri = uri;
        }
    }
}

