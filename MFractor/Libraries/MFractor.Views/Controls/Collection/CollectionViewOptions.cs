using System;

namespace MFractor.Views.Controls.Collection
{
    public class CollectionViewOptions
    {
        public CollectionViewOptions()
        {
        }


        public CollectionViewOptions(bool showSelectionCheckbox,
                                     string selectionCheckboxColumnName,
                                     bool showIcon,
                                     string iconColumnName,
                                     bool showPrimaryLabel,
                                     string primaryLabelColumnName,
                                     bool showSecondaryLabel,
                                     string secondaryLabelColumnName)
        {
            ShowSelectionCheckbox = showSelectionCheckbox;
            SelectionCheckboxColumnName = selectionCheckboxColumnName;
            ShowIcon = showIcon;
            IconColumnName = iconColumnName;
            ShowPrimaryLabel = showPrimaryLabel;
            PrimaryLabelColumnName = primaryLabelColumnName;
            ShowSecondaryLabel = showSecondaryLabel;
            SecondaryLabelColumnName = secondaryLabelColumnName;
        }

        public bool ShowSelectionCheckbox { get; private set; }
        public string SelectionCheckboxColumnName { get; private set; }

        public bool ShowIcon { get; private set; }
        public string IconColumnName { get; private set; }

        public bool ShowPrimaryLabel { get; private set; }
        public string PrimaryLabelColumnName { get; private set; }

        public bool ShowSecondaryLabel { get; private set; }
        public string SecondaryLabelColumnName { get; private set; }

        public CollectionViewOptions WithSelectionCheckboxColumn(string name)
        {
            SelectionCheckboxColumnName = name;
            ShowSelectionCheckbox = true;
            return this;
        }

        public CollectionViewOptions WithIconColumn(string name)
        {
            IconColumnName = name;
            ShowIcon = true;
            return this;
        }

        public CollectionViewOptions WithPrimaryLabelColumn(string name)
        {
            PrimaryLabelColumnName = name;
            ShowPrimaryLabel = true;
            return this;
        }

        public CollectionViewOptions WithSecondaryLabelColumn(string name)
        {
            SecondaryLabelColumnName = name;
            ShowSecondaryLabel = true;
            return this;
        }
    }
}
