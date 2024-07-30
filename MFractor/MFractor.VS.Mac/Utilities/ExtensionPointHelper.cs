using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Addins;

namespace MFractor.VS.Mac.Utilities
{
    public class ExtensionPointHelper
    {
        public const string CommandsPath = "/MonoDevelop/Ide/Commands";

        public const string MenuToolsPath = "/MonoDevelop/Ide/MainMenu/Tools";

        public const string ProjectPadPath = "/MonoDevelop/Ide/ContextMenu/ProjectPad";

        public const string ProjectPadToolsPath = "/MonoDevelop/Ide/ContextMenu/ProjectPad/Tools";

        public static void RenderExtensionPointHierachy(string extensionPointPath)
        {
            var nodes = AddinManager.AddinEngine.GetExtensionNodes(extensionPointPath).GetEnumerator();

            var objs = new List<object>();
            while (nodes.MoveNext())
            {
                objs.Add(nodes.Current);
            }

            var names = objs.OfType<InstanceExtensionNode>().Where(t => t != null).ToList();

            foreach (var n in names)
            {
                Console.WriteLine(n);
            }

            Debugger.Break();

            Console.WriteLine();
        }
    }
}
