using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MFractor.VS.Windows.Utilities
{
    public static class ResourceAccessor
    {
        public static Uri Get(string resourcePath)
        {
            var uri = string.Format(
                "pack://application:,,,/{0};component/{1}"
                , Assembly.GetExecutingAssembly().GetName().Name
                , resourcePath
            );

            return new Uri(uri);
        }
    }
}
