//using System;
//using MFractor.Views.Controls.Collection;
//using Xwt;
//using Xwt.Drawing;

//namespace MFractor.Views.Controls.CollapsibleCollection
//{
//    public class CollapsibleContentView : HBox
//    {


//        public CollectionView CollectionView { get; private set; }
//        Button toggleButton;

//        const double imageSize = 48.0;

//        readonly Image collapseImage = Image.FromResource("chevron-left.png").WithSize(imageSize, imageSize);

//        readonly Image expandImage = Image.FromResource("chevron-right.png").WithSize(imageSize, imageSize);


//        public CollapsibleContentView(CollectionViewOptions options,
//                                         ICollectionViewDragOperationHandler dragOperationHandler = null)
//        {
//            Build(options, dragOperationHandler);
//        }

//        public bool IsExpanded
//        {
//            get => CollectionView.Visible;
//            set
//            {
//                CollectionView.Visible = value;

//                ApplyButtonState(value);
//            }
//        }

//        void ApplyButtonState(bool isExpanded)
//        {
//            if (isExpanded)
//            {
//                toggleButton.Image = collapseImage;
//                toggleButton.TooltipText = "Hide this list";
//            }
//            else
//            {
//                toggleButton.Image = expandImage;
//                toggleButton.TooltipText = "Show this list";
//            }
//        }

//        void Build(CollectionViewOptions options, ICollectionViewDragOperationHandler dragOperationHandler)
//        {
//            CollectionView = new CollectionView(options, dragOperationHandler)
//            {
//                ExpandVertical = true,
//                WidthRequest = 300,
//            };

//            this.PackStart(CollectionView);

//            this.PackStart(new VSeparator());

//            toggleButton = new Button()
//            {
//                VerticalPlacement = WidgetPlacement.Center,
//            };
//            ApplyButtonState(true);
//            toggleButton.Clicked += ToggleButton_Clicked;

//            this.PackStart(toggleButton);
//        }

//        void ToggleButton_Clicked(object sender, EventArgs e)
//        {
//            IsExpanded = !IsExpanded;
//        }
//    }
//}
