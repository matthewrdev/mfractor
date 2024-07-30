using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MFractor.Images
{
    class ImageAssetCollection : IReadOnlyDictionary<string, IImageAsset>, IImageAssetCollection
    {
        readonly IReadOnlyDictionary<string, IImageAsset> imageAssets;

        public ImageAssetCollection(IReadOnlyDictionary<string, ImageAsset> imageAssets)
        {
            if (imageAssets is null)
            {
                throw new ArgumentNullException(nameof(imageAssets));
            }

            this.imageAssets = imageAssets.OrderBy(ia => ia.Key).ToDictionary(ia => ia.Key, ia => ia.Value as IImageAsset);
        }

        public ImageAssetCollection(IReadOnlyDictionary<string, IImageAsset> imageAssets)
        {
            this.imageAssets = imageAssets ?? throw new ArgumentNullException(nameof(imageAssets));
        }

        public IEnumerable<string> Keys => imageAssets.Keys;

        public IEnumerable<IImageAsset> Values => imageAssets.Values;

        public int Count => imageAssets.Count;

        public IImageAsset this[string key] => imageAssets[key];

        public bool ContainsKey(string key)
        {
            return imageAssets.ContainsKey(key);
        }

        public bool TryGetValue(string key, out IImageAsset value)
        {
            return imageAssets.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, IImageAsset>> GetEnumerator()
        {
            return imageAssets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return imageAssets.GetEnumerator();
        }

        public static readonly IImageAssetCollection Empty = new ImageAssetCollection(new Dictionary<string, IImageAsset>());
    }
}