using System;
using MFractor.Data.Models;

namespace MFractor.Images.Data.Models
{
    public class ImageAssetDefinition : GCEntity
    {
        /// <summary>
        /// The name of the image asset, including the file extension.
        /// </summary>
        /// <value>The name of the image.</value>
        public string Name { get; set; }

        /// <summary>
        /// The search name of the image asset, that is, the <see cref="Name"/> with diacritics removed.
        /// </summary>
        /// <value>The name of the search.</value>
        public string SearchName { get; }
    }
}
