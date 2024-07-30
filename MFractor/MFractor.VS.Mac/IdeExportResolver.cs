using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MFractor.IOC;
using MFractor.VS.Mac;
using MonoDevelop.IPhone;

[assembly: DeclareExportResolver(typeof(IdeExportResolver))]

namespace MFractor.VS.Mac
{
    class IdeExportResolver : BaseExportResolver
    {
        readonly Lazy<MethodInfo> getExportedValueMethodInfo = new Lazy<MethodInfo>(() =>
        {
            var provider = MonoDevelop.Ide.Composition.CompositionManager.Instance.ExportProvider;

            return provider.GetType()
                           .GetMethods()
                           .First(d => d.Name == "GetExportedValue" && d.GetParameters().Length == 0);
        });

        MethodInfo GetExportedValueMethodInfo => getExportedValueMethodInfo.Value;

        readonly Lazy<MethodInfo> getExportedValuesMethodInfo = new Lazy<MethodInfo>(() =>
        {
            var provider = MonoDevelop.Ide.Composition.CompositionManager.Instance.ExportProvider;

            return provider.GetType()
                           .GetMethods()
                           .First(d => d.Name == "GetExportedValues" && d.GetParameters().Length == 0);
        });

        MethodInfo GetExportedValuesMethodInfo => getExportedValuesMethodInfo.Value;

        public override Lazy<T> GetExport<T>()
        {
            return MonoDevelop.Ide.Composition.CompositionManager.GetExport<T>();
        }

        public override Lazy<IEnumerable<T>> GetExports<T>()
        {
            return MonoDevelop.Ide.Composition.CompositionManager.GetExports<T>();
        }

        public override T GetExportedValue<T>()
        {
            return MonoDevelop.Ide.Composition.CompositionManager.Instance.GetExportedValue<T>();
        }

        public override IEnumerable<T> GetExportedValues<T>()
        {
            var values =  MonoDevelop.Ide.Composition.CompositionManager.Instance.GetExportedValues<T>().ToList();

            return values;
        }

        public override object GetExportedValue(Type type)
        {
            // create an array of the generic types that the GetExportedValue<T> method expects
            var genericTypeArray = new Type[] { type };

            // add the generic types to the method
            var methodInfo = GetExportedValueMethodInfo.MakeGenericMethod(genericTypeArray);

            // invoke GetExportedValue<type>()
            return methodInfo.Invoke(MonoDevelop.Ide.Composition.CompositionManager.Instance.ExportProvider, null);
        }

        public override IEnumerable<object> GetExportedValues(Type type)
        {
            // create an array of the generic types that the GetExportedValues<T> method expects
            var genericTypeArray = new Type[] { type };

            var methodInfo = GetExportedValuesMethodInfo.MakeGenericMethod(genericTypeArray);

            // invoke GetExportedValue<type>()
            return (IEnumerable<object>)methodInfo.Invoke(MonoDevelop.Ide.Composition.CompositionManager.Instance.ExportProvider, null);
        }
    }
}