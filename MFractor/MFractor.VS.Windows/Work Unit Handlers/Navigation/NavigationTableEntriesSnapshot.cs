using System;
using MFractor.Work.WorkUnits;
using Microsoft.VisualStudio.Shell.TableControl;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.Shell.TableManager;
using MFractor.Ide.WorkUnits;
using MFractor.Text;

namespace MFractor.VS.Windows.WorkUnitHandlers.Navigation
{
    class NavigationTableEntriesSnapshot : WpfTableEntriesSnapshotBase
    {
        public IReadOnlyList<NavigateToFileSpanWorkUnit> WorkUnits { get; }

        readonly Dictionary<string, string> contentDictionary = new Dictionary<string, string>();
        readonly Dictionary<string, FileLocation> fileLocationDictionary = new Dictionary<string, FileLocation>();
        readonly Dictionary<string, ILineCollection> fileLinesDictionary = new Dictionary<string, ILineCollection>();
        readonly ILineCollectionFactory lineCollectionFactory;

        public NavigationTableEntriesSnapshot(IEnumerable<NavigateToFileSpanWorkUnit> workUnits,
                                              ILineCollectionFactory lineCollectionFactory)
        {
            WorkUnits = (workUnits ?? Enumerable.Empty<NavigateToFileSpanWorkUnit>()).ToList();
            this.lineCollectionFactory = lineCollectionFactory;
        }

        public override int Count => WorkUnits.Count;

        public override bool CanCreateDetailsContent(int index)
        {
            return false;
        }

        public override bool TryGetValue(int index, string keyName, out object content)
        {
            var unit = WorkUnits[index];

            var success = true;
            switch (keyName)
            {
                case StandardTableKeyNames.BuildTool:
                    content = "MFractor";
                    break;
                case StandardTableKeyNames.ProjectName:
                    content = unit.Project?.Name ?? string.Empty;
                    break;
                case StandardTableKeyNames.DocumentName:
                    content = unit.FilePath;
                    break;
                case StandardTableKeyNames.Text:
                    content = GetContent(unit);
                    break;
                case StandardTableKeyNames.Column:
                    content = GetColumn(unit);
                    break;
                case StandardTableKeyNames.Line:
                    content = GetLine(unit);
                    break;
                default:
                    success = false;
                    content = string.Empty;
                    break;
            }

            return success;
        }

        int GetLine(NavigateToFileSpanWorkUnit unit)
        {
            if (fileLocationDictionary.TryGetValue(unit.Identifier, out var location))
            {
                return location.Line;
            }

            var lineCollection = GetLineCollection(unit.FilePath);

            fileLocationDictionary[unit.Identifier] = lineCollection.GetLocation(unit.Span.Start) ?? new FileLocation(0, 0);
            return fileLocationDictionary[unit.Identifier].Line;
        }

        int GetColumn(NavigateToFileSpanWorkUnit unit)
        {
            if (fileLocationDictionary.TryGetValue(unit.Identifier, out var location))
            {
                return location.Column;
            }

            var lineCollection = GetLineCollection(unit.FilePath);

            fileLocationDictionary[unit.Identifier] = lineCollection.GetLocation(unit.Span.Start) ?? new FileLocation(0,0);
            return fileLocationDictionary[unit.Identifier].Column;
        }

        string GetContent(NavigateToFileSpanWorkUnit unit)
        {
            if (contentDictionary.TryGetValue(unit.Identifier, out var content))
            {
                return content;
            }

            var lineCollection = GetLineCollection(unit.FilePath);

            contentDictionary[unit.Identifier] = lineCollection.GetContent(unit.Span);
            return contentDictionary[unit.Identifier];
        }

        ILineCollection GetLineCollection(string filePath)
        {
            if (fileLinesDictionary.TryGetValue(filePath, out var lines))
            {
                return lines;
            }

            fileLinesDictionary[filePath] = this.lineCollectionFactory.Create(new FileInfo(filePath));

            return fileLinesDictionary[filePath];
        }
    }
}
