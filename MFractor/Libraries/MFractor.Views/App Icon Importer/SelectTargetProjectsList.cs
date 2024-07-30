using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Images.Importing;
using Xwt;

namespace MFractor.Views.AppIconImporter
{
    class SelectTargetProjectsList : ListView
    {
        List<TargetProject> targetProjects;
        ListStore listStore;
        DataField<bool> includeProjectField;
        DataField<bool> cleanupProjectField;

        public SelectTargetProjectsList(IEnumerable<TargetProject> targetProjects)
        {
            this.targetProjects = targetProjects.ToList();
            Build();
        }

        void Build()
        {
            includeProjectField = new DataField<bool>();
            var projectNameField = new DataField<string>();
            cleanupProjectField = new DataField<bool>();

            listStore = new ListStore(includeProjectField, projectNameField, cleanupProjectField);
            DataSource = listStore;
            GridLinesVisible = GridLines.Horizontal;

            var includeProjectCell = new CheckBoxCellView
            {
                Editable = true,
                ActiveField = includeProjectField,
            };
            includeProjectCell.Toggled += async (s, e) =>
            {
                await Task.Delay(5);
                UpdateSelection();
            };
            Columns.Add("Include", includeProjectCell);

            var projectNameCell = new TextCellView
            {
                Editable = false,
                TextField = projectNameField,
            };
            Columns.Add(new ListViewColumn("Project Name".PadRight(80, ' '), projectNameCell));

            var cleanupProjectCell = new CheckBoxCellView
            {
                Editable = true,
                ActiveField = cleanupProjectField,
            };
            // TODO: Temporarily removed the Cleanup field because lack of user information about its workings. Need to check back later
            //Columns.Add("Cleanup", cleanupProjectCell);

            for (var i = 0; i < targetProjects.Count; i++)
            {
                var project = targetProjects[i];
                var row = listStore.AddRow();
                listStore.SetValue(row, includeProjectField, project.IsSelected);
                listStore.SetValue(row, projectNameField, project.Project.Name);
                listStore.SetValue(row, cleanupProjectField, project.IsSelected);
            }
        }

        void UpdateSelection()
        {
            for (var i = 0; i < listStore.RowCount; i++)
            {
                targetProjects[i].IsSelected = listStore.GetValue(i, includeProjectField);
            }
        }
    }

    public static class ListUtils
    {
        public static void ForEach<T>(this List<T> list, Action<int, T> action)
        {
            for (var i = 0; i < list.Count; i++)
            {
                action(i, list[i]);
            }
        }
    }
}
