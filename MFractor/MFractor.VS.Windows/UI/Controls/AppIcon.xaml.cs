using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MFractor.Images.Models;

namespace MFractor.VS.Windows.UI.Controls
{
    /// <summary>
    /// Interaction logic for AppIcon.xaml
    /// </summary>
    public partial class AppIcon : StackPanel
    {
        public static readonly DependencyProperty IconProperty = 
            DependencyProperty.Register(
                nameof(Icon), 
                typeof(IconImage), 
                typeof(AppIcon),
                new PropertyMetadata(default(IconImage), IconPropertyChanged));

        public IconImage Icon
        {
            get { return (IconImage)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        
        public static void IconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AppIcon)d).IconPropertyChanged((IconImage)e.NewValue);
        }

        void IconPropertyChanged(IconImage icon)
        {
            //nameLabel.Content = icon.ImageScale.Name;
            //icon.PropertyChanged += OnIconPropertyChanged;
        }

        void OnIconPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == nameof(IconImage.ImageFileName))
            //{
            //    iconImage.Source = new BitmapImage(new Uri(Icon.ImageFileName));
            //}
        }

        public AppIcon()
        {
            InitializeComponent();
        }
    }
}
