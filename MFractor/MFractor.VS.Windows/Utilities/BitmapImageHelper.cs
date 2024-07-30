using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MFractor.Utilities;

namespace MFractor.VS.Windows.Utilities
{
    public static class BitmapImageHelper
    {
        public static BitmapSource GetSourceForOnRender(string sourceName)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceId = ResourcesHelper.LocateMatchingResourceId(assembly, sourceName);

            if (string.IsNullOrWhiteSpace(resourceId))
            {
                return null;
            }

            var bitmap = new BitmapImage();

            using (var stream = assembly.GetManifestResourceStream(resourceId))
            {
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }

            return bitmap;
        }
    }
}
