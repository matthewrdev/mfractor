using System;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.XamlPlatforms.Maui
{
    public class MauiXamlPlatform : IXamlPlatform
    {
        public bool IsSupported(Project project, Compilation compilation, IXmlSyntaxTree xmlSyntaxTree)
        {
            return compilation.HasAssembly((name) => name.StartsWith("Microsoft.Maui"));
        }

        public bool IsSupported(Project project, Compilation compilation)
        {
            return compilation.HasAssembly((name) => name.StartsWith("Microsoft.Maui"));
        }

        public bool IsSupported(IXmlSyntaxTree xmlSyntaxTree)
        {
            return xmlSyntaxTree.Root.GetAttributeByName("xmlns")?.Value?.Value == SchemaUrl;
        }

        public XamlPlatform Platform => XamlPlatform.Maui;

        public string SchemaUrl => "http://schemas.microsoft.com/dotnet/2021/maui";

        public const string MauiAssemblyName = "Microsoft.Maui.Controls";

        public string Assembly => MauiAssemblyName;

        public ITypeDefinition Element { get; } = new TypeDefinition("Microsoft.Maui.Controls.Element");

        public ITypeDefinition VisualElement { get; } = new TypeDefinition("Microsoft.Maui.Controls.VisualElement");

        public ITypeDefinition BindableObject { get; } = new TypeDefinition("Microsoft.Maui.Controls.BindableObject");

        public ITypeDefinition BindableProperty { get; } = new TypeDefinition("Microsoft.Maui.Controls.BindableProperty");

        public ITypeDefinition BindingBase { get; } = new TypeDefinition("Microsoft.Maui.Controls.BindingBase");

        public ITypeDefinition XmlnsDefinitionAttribute { get; } = new TypeDefinition("Microsoft.Maui.Controls.XmlnsDefinitionAttribute");

        public ITypeDefinition Command { get; } = new TypeDefinition("Microsoft.Maui.Controls.Command");

        public ITypeDefinition Application { get; } = new TypeDefinition("Microsoft.Maui.Controls.Application");

        public ITypeDefinition ResourceDictionary { get; } = new TypeDefinition("Microsoft.Maui.Controls.ResourceDictionary");

        public string BindingContextProperty { get; } = "BindingContext";

        public ITypeDefinition ContentPropertyAttribute { get; } = new TypeDefinition("Microsoft.Maui.Controls.ContentPropertyAttribute");

        public ITypeDefinition View { get; } = new TypeDefinition("Microsoft.Maui.Controls.View");

        public ITypeDefinition Layout { get; } = new TypeDefinition("Microsoft.Maui.Controls.Layout");

        public ITypeDefinition Frame { get; } = new TypeDefinition("Microsoft.Maui.Controls.Frame");

        public ITypeDefinition ContentView { get; } = new TypeDefinition("Microsoft.Maui.Controls.ContentView");

        public ITypeDefinition Grid { get; } = new TypeDefinition("Microsoft.Maui.Controls.Grid");

        public ITypeDefinition GridLength { get; } = new TypeDefinition("Microsoft.Maui.GridLength");

        public ITypeDefinition RowDefinitionCollection { get; } = new TypeDefinition("Microsoft.Maui.Controls.RowDefinitionCollection");

        public ITypeDefinition ColumnDefinitionCollection { get; } = new TypeDefinition("Microsoft.Maui.Controls.ColumnDefinitionCollection");

        public ITypeDefinition RowDefinition { get; } = new TypeDefinition("Microsoft.Maui.Controls.RowDefinition");

        public ITypeDefinition ColumnDefinition { get; } = new TypeDefinition("Microsoft.Maui.Controls.ColumnDefinition");

        public string RowDefinitionsProperty { get; } = "RowDefinitions";

        public string RowProperty { get; } = "Row";

        public string RowHeightProperty { get; } = "Height";

        public string ColumnDefinitionsProperty { get; } = "ColumnDefinitions";

        public string ColumnProperty { get; } = "Column";

        public string ColumnWidthProperty { get; } = "Width";

        public string GridNamedSize_Auto { get; } = "Auto";

        public string GridNamedSize_Star { get; } = "Star";

        public ITypeDefinition StackLayout { get; } = new TypeDefinition("Microsoft.Maui.Controls.StackLayout");

        public bool SupportsTypedOrientationStackLayouts => true;

        public ITypeDefinition VerticalStackLayout { get; } = new TypeDefinition("Microsoft.Maui.Controls.VerticalStackLayout");

        public ITypeDefinition HorizontalStackLayout { get; } = new TypeDefinition("Microsoft.Maui.Controls.HorizontalStackLayout");

        public string OrientationProperty { get; } = "Orientation";

        public string StackLayoutOrientation_Horizontal { get; } = "Horizontal";

        public string StackLayoutOrientation_Vertical { get; } = "Vertical";

        public ITypeDefinition FlexLayout { get; } = new TypeDefinition("Microsoft.Maui.Controls.FlexLayout");

        public ITypeDefinition AbsoluteLayout { get; } = new TypeDefinition("Microsoft.Maui.Controls.AbsoluteLayout");

        public ITypeDefinition RelativeLayout { get; } = new TypeDefinition("Microsoft.Maui.Controls.RelativeLayout");

        public ITypeDefinition ScrollView { get; } = new TypeDefinition("Microsoft.Maui.Controls.ScrollView");

        public ITypeDefinition Button { get; } = new TypeDefinition("Microsoft.Maui.Controls.Button");

        public ITypeDefinition Label { get; } = new TypeDefinition("Microsoft.Maui.Controls.Label");

        public ITypeDefinition FormattedString { get; } = new TypeDefinition("Microsoft.Maui.Controls.FormattedString");

        public ITypeDefinition Entry { get; } = new TypeDefinition("Microsoft.Maui.Controls.Entry");

        public ITypeDefinition Editor { get; } = new TypeDefinition("Microsoft.Maui.Controls.Editor");

        public ITypeDefinition Picker { get; } = new TypeDefinition("Microsoft.Maui.Controls.Picker");

        public ITypeDefinition Slider { get; } = new TypeDefinition("Microsoft.Maui.Controls.Slider");

        public bool SupportsRefreshView => true;

        public ITypeDefinition RefreshView { get; } = new TypeDefinition("Microsoft.Maui.Controls.RefreshView");

        public bool SupportsIndicatorView => true;

        public ITypeDefinition IndicatorView { get; } = new TypeDefinition("Microsoft.Maui.Controls.IndicatorView");

        public bool SupportsStyleSheets => true;

        public ITypeDefinition StyleSheet { get; } = new TypeDefinition("Microsoft.Maui.Controls.StyleSheets.StyleSheet");

        public bool SupportsGeometry => true;

        public ITypeDefinition PathFigureCollection { get; } = new TypeDefinition("Microsoft.Maui.Controls.Shapes.PathFigureCollection");

        public ITypeDefinition Path { get; } = new TypeDefinition("Microsoft.Maui.Controls.Shapes.Path");

        public ITypeDefinition Geometry { get; } = new TypeDefinition("Microsoft.Maui.Controls.Shapes.Geometry");

        public string PickerItemDisplayBindingProperty { get; } = "ItemDisplayBinding";

        public string GroupDisplayBindingProperty { get; } = "GroupDisplayBinding";

        public string IsGroupingEnabledProperty { get; } = "IsGroupingEnabled";

        public string ItemsViewItemSourceProperty { get; } = "ItemsSource";

        public ITypeDefinition ListView { get; } = new TypeDefinition("Microsoft.Maui.Controls.ListView");

        public ITypeDefinition CollectionView { get; } = new TypeDefinition("Microsoft.Maui.Controls.CollectionView");

        public ITypeDefinition ValueConverter { get; } = new TypeDefinition("Microsoft.Maui.Controls.IValueConverter");

        public ITypeDefinition Color { get; } = new TypeDefinition("Microsoft.Maui.Graphics.Color");

        public ITypeDefinition Style { get; } = new TypeDefinition("Microsoft.Maui.Controls.Style");

        public ITypeDefinition Setter { get; } = new TypeDefinition("Microsoft.Maui.Controls.Setter");

        public ITypeDefinition Thickness { get; } = new TypeDefinition("Microsoft.Maui.Thickness");

        public ITypeDefinition MarkupExtension { get; } = new TypeDefinition("Microsoft.Maui.Controls.Xaml.IMarkupExtension");

        public ITypeDefinition BindingExtension { get; } = new TypeDefinition("Microsoft.Maui.Controls.Xaml.BindingExtension");

        public ITypeDefinition StaticResourceExtension { get; } = new TypeDefinition("Microsoft.Maui.Controls.Xaml.StaticResourceExtension");

        public ITypeDefinition DynamicResourceExtension { get; } = new TypeDefinition("Microsoft.Maui.Controls.Xaml.DynamicResourceExtension");

        public bool SupportsBindableLayout => true;

        public ITypeDefinition BindableLayout { get; } = new TypeDefinition("Microsoft.Maui.Controls.BindableLayout");

        /// <summary>
        /// Fonts are included via the maui app builder.
        /// </summary>
        public bool SupportsExportFontAttribute => false;

        public ITypeDefinition ExportFontAttribute { get; } = new TypeDefinition("Microsoft.Maui.Controls.ExportFontAttribute");

        public ITypeDefinition TypeConverterAttribute { get; } = new TypeDefinition("Microsoft.Maui.Controls.TypeConverterAttribute");

        public string ItemSourceProperty { get; } = "ItemsSource";

        public string ItemTemplateProperty { get; } = "ItemTemplate";

        public ITypeDefinition Cell { get; } = new TypeDefinition("Microsoft.Maui.Controls.Cell");

        public ITypeDefinition ViewCell { get; } = new TypeDefinition("Microsoft.Maui.Controls.ViewCell");

        public ITypeDefinition DataTemplate { get; } = new TypeDefinition("Microsoft.Maui.Controls.DataTemplate");

        public bool SupportsTriggers => true;

        public ITypeDefinition TriggerBase { get; } = new TypeDefinition("Microsoft.Maui.Controls.TriggerBase");

        public ITypeDefinition DataTrigger { get; } = new TypeDefinition("Microsoft.Maui.Controls.DataTrigger");

        public ITypeDefinition MultiTrigger { get; } = new TypeDefinition("Microsoft.Maui.Controls.MultiTrigger");

        public ITypeDefinition Behaviour { get; } = new TypeDefinition("Microsoft.Maui.Controls.Behavior<T>");

        public bool SupportsOnPlatform => true;

        public ITypeDefinition OnPlatform { get; } = new TypeDefinition("Microsoft.Maui.Controls.OnPlatform<T>");

        public bool SupportsOnIdiom => true;

        public ITypeDefinition OnIdiom { get; } = new TypeDefinition("Microsoft.Maui.Controls.OnIdiom<T>");

        public ITypeDefinition On { get; } = new TypeDefinition("Microsoft.Maui.Controls.On");

        public bool SupportsDevice => true;

        public ITypeDefinition Device { get; } = new TypeDefinition("Microsoft.Maui.Controls.Device");

        public ITypeDefinition Page { get; } = new TypeDefinition("Microsoft.Maui.Controls.Page");

        public ITypeDefinition ContentPage { get; } = new TypeDefinition("Microsoft.Maui.Controls.ContentPage");

        public ITypeDefinition NavigationPage { get; } = new TypeDefinition("Microsoft.Maui.Controls.NavigationPage");

        public ITypeDefinition FlyoutPage { get; } = new TypeDefinition("Microsoft.Maui.Controls.FlyoutPage");

        public ITypeDefinition TabbedPage { get; } = new TypeDefinition("Microsoft.Maui.Controls.TabbedPage");

        public ITypeDefinition CarouselPage { get; } = new TypeDefinition("Microsoft.Maui.Controls.CarouselPage");

        public ITypeDefinition TemplatedPage { get; } = new TypeDefinition("Microsoft.Maui.Controls.TemplatedPage");

        public bool SupportsShell => true;

        public ITypeDefinition Shell { get; } = new TypeDefinition("Microsoft.Maui.Controls.Shell");

        public ITypeDefinition ShellItem { get; } = new TypeDefinition("Microsoft.Maui.Controls.ShellItem");

        public ITypeDefinition ShellContent { get; } = new TypeDefinition("Microsoft.Maui.Controls.ShellContent");

        public ITypeDefinition VisualState { get; } = new TypeDefinition("Microsoft.Maui.Controls.VisualState");

        public ITypeDefinition ImageView { get; } = new TypeDefinition("Microsoft.Maui.Controls.Image");

        public ITypeDefinition ImageSource { get; } = new TypeDefinition("Microsoft.Maui.Controls.ImageSource");

        public bool SupportsCustomRenderers => true;

        public ITypeDefinition ExportRendererAttribute { get; } = new TypeDefinition("Microsoft.Maui.Controls.ExportRendererAttribute");
    }
}
