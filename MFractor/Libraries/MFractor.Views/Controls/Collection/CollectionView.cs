using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using MFractor.IOC;
using MFractor.Utilities;
using MFractor.Utilities.StringMatchers;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.Controls.Collection
{
    public class CollectionView : Xwt.VBox
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly LaneStringMatcher filter = new LaneStringMatcher();

        Label titleLabel;

        TextEntry searchEntry;

        ListView listView;
        ListStore listDataStore;

        readonly Image dragDropImage = Image.FromResource("feather-image-16.png").WithSize(16, 16);

        readonly DataField<bool> selectedField = new DataField<bool>();
        readonly DataField<Image> iconField = new DataField<Image>();
        readonly DataField<string> labelField = new DataField<string>();
        readonly DataField<string> secondaryLabelField = new DataField<string>();

        readonly CollectionViewOptions options;
        readonly ICollectionViewDragOperationHandler dragOperationHandler;
        CheckBoxCellView selectionCellView;
        ImageCellView iconCellView;
        TextCellView labelCellView;
        TextCellView secondaryLabelCellView;

        public event EventHandler ItemsCheckSelectionChanged;
        public event EventHandler<CollectionViewItemSelectedEventArgs> ItemSelected;
        public event EventHandler<CollectionViewItemInteractionEventArgs> ItemRightClicked;
        public event EventHandler<CollectionViewItemInteractionEventArgs> ItemDoubleClicked;

        [Import]
        public IDispatcher Dispatcher { get; set; }

        public CollectionView(CollectionViewOptions options, ICollectionViewDragOperationHandler dragOperationHandler = null)
        {
            Resolver.ComposeParts(this);

            this.options = options;
            this.dragOperationHandler = dragOperationHandler;

            Build();

            if (dragOperationHandler != null)
            {
                listView.SetDragDropTarget(TransferDataType.Text);
                listView.SetDragSource(TransferDataType.Text);
                listView.DragStarted += ListView_DragStarted;
            }

            ApplyItems(Items);

            BindEvents();
        }

        void BindEvents()
        {
            UnbindEvents();

            searchEntry.Changed += SearchEntry_Changed;
            selectionCellView.Toggled += SelectionCellView_Toggled;

            listView.ButtonReleased += ListView_ButtonReleased;
            listView.ButtonPressed += ListView_ButtonPressed;
            listView.SelectionChanged += ListView_SelectionChanged;
        }

        void ListView_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                ItemSelected?.Invoke(this, new CollectionViewItemSelectedEventArgs(SelectedItem));
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void ListView_ButtonPressed(object sender, ButtonEventArgs e)
        {
            if (e.Button == PointerButton.Left && e.MultiplePress == 2)
            {
                try
                {
                    ItemDoubleClicked?.Invoke(this, new CollectionViewItemInteractionEventArgs(SelectedItem, listView, e));
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }
        }

        void ListView_ButtonReleased(object sender, ButtonEventArgs e)
        {
            if (e.Button == PointerButton.Right)
            {
                try
                {
                    ItemRightClicked?.Invoke(this, new CollectionViewItemInteractionEventArgs(SelectedItem, listView, e));
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
            }
        }

        void SearchEntry_Changed(object sender, EventArgs e)
        {
            UnbindEvents();

            try
            {
                ApplyItems(FilteredItems);
            }
            finally
            {
                BindEvents();
            }
        }

        public void Focus(Func<ICollectionItem, bool> predicate)
        {
            if (predicate == null)
            {
                return;
            }

            try
            {
                var items = FilteredItems;
                for (var i = 0; i < items.Count; ++i)
                {
                    var item = items[i];
                    if (predicate(item))
                    {
                        Focus(i);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public void Focus(ICollectionItem item)
        {
            if (item == null)
            {
                return;
            }

            try
            {
                var index = FilteredItems.IndexOf(item);

                Focus(index);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public void Focus(int row)
        {
            if (row < listDataStore.RowCount && row >= 0)
            {
                listView.SelectRow(row);
                listView.ScrollToRow(row);
            }
        }

        void SelectionCellView_Toggled(object sender, WidgetEventArgs e)
        {
            UnbindEvents();

            Task.Run(async () =>
            {
                await Task.Delay(2);

                Dispatcher.InvokeOnMainThread(() =>
                {

                    try
                    {
                        var filteredItems = FilteredItems;
                        for (var row = 0; row < listDataStore.RowCount; ++row)
                        {
                            var selected = listDataStore.GetValue(row, selectedField);

                            if (row < filteredItems.Count)
                            {
                                var item = filteredItems[row];

                                item.IsChecked = selected;
                            }
                        }

                    }
                    finally
                    {
                        ItemsCheckSelectionChanged?.Invoke(this, EventArgs.Empty);
                        ApplyItems(Items);

                        BindEvents();
                    }
                });
            });
        }

        void UnbindEvents()
        {
            searchEntry.Changed -= SearchEntry_Changed;
            selectionCellView.Toggled -= SelectionCellView_Toggled;
        }

        void Build()
        {
            BuildHeader();

            BuildListView();
        }

        void BuildListView()
        {
            listView = new ListView()
            {
                ExpandVertical = true
            };

            selectionCellView = new CheckBoxCellView(selectedField)
            {
                Editable = true,
            };
            iconCellView = new ImageCellView(iconField);
            labelCellView = new TextCellView(labelField)
            {
                Editable = false,
            };
            secondaryLabelCellView = new TextCellView(secondaryLabelField)
            {
                Editable = false,
            };

            ApplyDataFields(options);
            ApplyColumns(options);

            PackStart(listView, true, true);
        }

        void BuildHeader()
        {
            titleLabel = new Label();
            PackStart(titleLabel);

            searchEntry = new TextEntry();
            PackStart(searchEntry);
            PackStart(new HSeparator());
        }

        IReadOnlyList<ICollectionItem> items;
        public IReadOnlyList<ICollectionItem> Items
        {
            get => items;
            set
            {
                items = value;
                ApplyItems(items);
            }
        }

        public bool AllowSearch
        {
            get => searchEntry.Visible;
            set
            {
                searchEntry.Visible = value;
                if (!value)
                {
                    UnbindEvents();
                    try
                    {
                        searchEntry.Text = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                    finally
                    {
                        BindEvents();
                    }
                }
            }
        }

        void ListView_DragStarted(object sender, DragStartedEventArgs e)
        {
            try
            {
                e.DragOperation.SetDragImage(dragDropImage, 16, 16);
                dragOperationHandler?.OnDrag(SelectedItem, e.DragOperation);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        public IReadOnlyList<ICollectionItem> FilteredItems => GetSearchMatches(SearchText, Items).ToList();

        IEnumerable<ICollectionItem> GetSearchMatches(string searchText, IEnumerable<ICollectionItem> collectionItems)
        {
            if (collectionItems == null)
            {
                return Enumerable.Empty<ICollectionItem>().ToList();
            }

            if (string.IsNullOrEmpty(searchText))
            {
                return collectionItems.ToList();
            }

            filter.SetFilter(searchText.RemoveDiacritics());
            return filter.Match(collectionItems, (item) => item.SearchText);
        }

        public string Title
        {
            get => titleLabel.Text;
            set => titleLabel.Text = value;
        }

        public string SearchText
        {
            get => searchEntry.Text;
            set => searchEntry.Text = value;
        }

        public string SearchPlaceholderText
        {
            get => searchEntry.PlaceholderText;
            set => searchEntry.PlaceholderText = value;
        }

        public ICollectionItem SelectedItem
        {
            get
            {
                var filteredItems = FilteredItems;

                if (filteredItems == null)
                {
                    return null;
                }

                if (listView.SelectedRow < 0 || listView.SelectedRow >= filteredItems.Count)
                {
                    return null;
                }

                var selection = filteredItems[listView.SelectedRow];

                return selection;
            }
        }

        void ApplyItems(IReadOnlyList<ICollectionItem> items)
        {
            listDataStore.Clear();
            if (items != null)
            {
                items = GetSearchMatches(SearchText, items).ToList();

                foreach (var item in items)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    var row = listDataStore.AddRow();

                    if (options.ShowPrimaryLabel)
                    {
                        listDataStore.SetValue(row, labelField, item.DisplayText);
                    }

                    if (options.ShowSecondaryLabel)
                    {
                        listDataStore.SetValue(row, secondaryLabelField, item.SecondaryDisplayText);
                    }

                    if (options.ShowIcon && item.Icon != null)
                    {
                        listDataStore.SetValue(row, iconField, item.Icon);
                    }

                    if (options.ShowSelectionCheckbox)
                    {
                        listDataStore.SetValue(row, selectedField, item.IsChecked);
                    }
                }
            }
        }

        void ApplyColumns(CollectionViewOptions options)
        {
            if (options.ShowSelectionCheckbox)
            {
                listView.Columns.Add(options.SelectionCheckboxColumnName ?? string.Empty, selectionCellView);
            }

            if (options.ShowIcon)
            {
                listView.Columns.Add(options.IconColumnName ?? string.Empty, iconCellView);
            }

            if (options.ShowPrimaryLabel)
            {
                listView.Columns.Add(options.PrimaryLabelColumnName ?? string.Empty, labelCellView);
            }

            if (options.ShowSecondaryLabel)
            {
                listView.Columns.Add(options.SecondaryLabelColumnName ?? string.Empty, secondaryLabelCellView);
            }

            listView.DataSource = listDataStore;
        }

        void ApplyDataFields(CollectionViewOptions options)
        {
            var fields = new List<IDataField>();

            if (options.ShowSelectionCheckbox)
            {
                fields.Add(selectedField);
            }

            if (options.ShowIcon)
            {
                fields.Add(iconField);
            }

            if (options.ShowPrimaryLabel)
            {
                fields.Add(labelField);
            }

            if (options.ShowSecondaryLabel)
            {
                fields.Add(secondaryLabelField);
            }

            listDataStore = new ListStore(fields.ToArray());
        }
    }
}
