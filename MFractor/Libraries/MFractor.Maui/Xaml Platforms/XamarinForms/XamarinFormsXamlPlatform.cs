using System.Linq;
using MFractor.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.XamlPlatforms.XamarinForms
{
    public class XamarinFormsXamlPlatform : IXamlPlatform
    {
        public bool IsSupported(Project project, Compilation compilation, IXmlSyntaxTree xmlSyntaxTree)
        {
            return compilation.HasAssembly(Assembly);
        }

        public bool IsSupported(Project project, Compilation compilation)
        {
            return compilation.HasAssembly(Assembly);
        }

        public bool IsSupported(IXmlSyntaxTree xmlSyntaxTree)
        {
            return xmlSyntaxTree.Root.GetAttributeByName("xmlns")?.Value?.Value == SchemaUrl;
        }

        public XamlPlatform Platform => XamlPlatform.XamarinForms;

        public string Assembly => "Xamarin.Forms.Core";

        public string SchemaUrl => "http://xamarin.com/schemas/2014/forms";

        public ITypeDefinition Element { get; } = new TypeDefinition("Xamarin.Forms.Element");

        public ITypeDefinition VisualElement { get; } = new TypeDefinition("Xamarin.Forms.VisualElement");

        public ITypeDefinition BindableObject { get; } = new TypeDefinition("Xamarin.Forms.BindableObject");

        public ITypeDefinition BindableProperty { get; } = new TypeDefinition("Xamarin.Forms.BindableProperty");

        public ITypeDefinition BindingBase { get; } = new TypeDefinition("Xamarin.Forms.BindingBase");

        public ITypeDefinition XmlnsDefinitionAttribute { get; } = new TypeDefinition("Xamarin.Forms.XmlnsDefinitionAttribute");

        public ITypeDefinition Command { get; } = new TypeDefinition("Xamarin.Forms.Command");

        public ITypeDefinition Application { get; } = new TypeDefinition("Xamarin.Forms.Application");

        public ITypeDefinition ResourceDictionary { get; } = new TypeDefinition("Xamarin.Forms.ResourceDictionary");

        public string BindingContextProperty { get; } = "BindingContext";

        public ITypeDefinition ContentPropertyAttribute { get; } = new TypeDefinition("Xamarin.Forms.ContentPropertyAttribute");

        public ITypeDefinition View { get; } = new TypeDefinition("Xamarin.Forms.View");

        public ITypeDefinition Layout { get; } = new TypeDefinition("Xamarin.Forms.Layout");

        public ITypeDefinition Frame { get; } = new TypeDefinition("Xamarin.Forms.Frame");

        public ITypeDefinition ContentView { get; } = new TypeDefinition("Xamarin.Forms.ContentView");

        public ITypeDefinition Grid { get; } = new TypeDefinition("Xamarin.Forms.Grid");

        public ITypeDefinition GridLength { get; } = new TypeDefinition("Xamarin.Forms.GridLength");

        public ITypeDefinition RowDefinitionCollection { get; } = new TypeDefinition("Xamarin.Forms.RowDefinitionCollection");

        public ITypeDefinition ColumnDefinitionCollection { get; } = new TypeDefinition("Xamarin.Forms.ColumnDefinitionCollection");

        public ITypeDefinition RowDefinition { get; } = new TypeDefinition("Xamarin.Forms.RowDefinition");

        public ITypeDefinition ColumnDefinition { get; } = new TypeDefinition("Xamarin.Forms.ColumnDefinition");

        public string RowDefinitionsProperty { get; } = "RowDefinitions";

        public string RowProperty { get; } = "Row";

        public string RowHeightProperty { get; } = "Height";

        public string ColumnDefinitionsProperty { get; } = "ColumnDefinitions";

        public string ColumnProperty { get; } = "Column";

        public string ColumnWidthProperty { get; } = "Width";

        public string GridNamedSize_Auto { get; } = "Auto";

        public string GridNamedSize_Star { get; } = "Star";

        public ITypeDefinition StackLayout { get; } = new TypeDefinition("Xamarin.Forms.StackLayout");

        public bool SupportsTypedOrientationStackLayouts => false;

        public ITypeDefinition VerticalStackLayout { get; } = TypeDefinition.Undefined;

        public ITypeDefinition HorizontalStackLayout { get; } = TypeDefinition.Undefined;

        public string OrientationProperty { get; } = "Orientation";

        public string StackLayoutOrientation_Horizontal { get; } = "Horizontal";

        public string StackLayoutOrientation_Vertical { get; } = "Vertical";

        public ITypeDefinition FlexLayout { get; } = new TypeDefinition("Xamarin.Forms.FlexLayout");

        public ITypeDefinition AbsoluteLayout { get; } = new TypeDefinition("Xamarin.Forms.AbsoluteLayout");

        public ITypeDefinition RelativeLayout { get; } = new TypeDefinition("Xamarin.Forms.RelativeLayout");

        public ITypeDefinition ScrollView { get; } = new TypeDefinition("Xamarin.Forms.ScrollView");

        public ITypeDefinition Button { get; } = new TypeDefinition("Xamarin.Forms.Button");

        public ITypeDefinition Label { get; } = new TypeDefinition("Xamarin.Forms.Label");

        public ITypeDefinition FormattedString { get; } = new TypeDefinition("Xamarin.Forms.FormattedString");

        public ITypeDefinition Entry { get; } = new TypeDefinition("Xamarin.Forms.Entry");

        public ITypeDefinition Editor { get; } = new TypeDefinition("Xamarin.Forms.Editor");

        public ITypeDefinition Picker { get; } = new TypeDefinition("Xamarin.Forms.Picker");

        public ITypeDefinition Slider { get; } = new TypeDefinition("Xamarin.Forms.Slider");

        public bool SupportsRefreshView => true;

        public ITypeDefinition RefreshView { get; } = new TypeDefinition("Xamarin.Forms.RefreshView");

        public bool SupportsIndicatorView => true;

        public ITypeDefinition IndicatorView { get; } = new TypeDefinition("Xamarin.Forms.IndicatorView");

        public bool SupportsStyleSheets => true;

        public ITypeDefinition StyleSheet { get; } = new TypeDefinition("Xamarin.Forms.StyleSheets.StyleSheet");

        public bool SupportsGeometry => true;

        public ITypeDefinition PathFigureCollection { get; } = new TypeDefinition("Xamarin.Forms.Shapes.PathFigureCollection");

        public ITypeDefinition Path { get; } = new TypeDefinition("Xamarin.Forms.Shapes.Path");

        public ITypeDefinition Geometry { get; } = new TypeDefinition("Xamarin.Forms.Shapes.Geometry");

        public string PickerItemDisplayBindingProperty { get; } = "ItemDisplayBinding";

        public string GroupDisplayBindingProperty { get; } = "GroupDisplayBinding";

        public string IsGroupingEnabledProperty { get; } = "IsGroupingEnabled";

        public string ItemsViewItemSourceProperty { get; } = "ItemsSource";

        public ITypeDefinition ListView { get; } = new TypeDefinition("Xamarin.Forms.ListView");

        public ITypeDefinition CollectionView { get; } = new TypeDefinition("Xamarin.Forms.CollectionView");

        public ITypeDefinition ValueConverter { get; } = new TypeDefinition("Xamarin.Forms.IValueConverter");

        public ITypeDefinition Color { get; } = new TypeDefinition("Xamarin.Forms.Color");

        public ITypeDefinition Style { get; } = new TypeDefinition("Xamarin.Forms.Style");

        public ITypeDefinition Setter { get; } = new TypeDefinition("Xamarin.Forms.Setter");

        public ITypeDefinition Thickness { get; } = new TypeDefinition("Xamarin.Forms.Thickness");

        public ITypeDefinition MarkupExtension { get; } = new TypeDefinition("Xamarin.Forms.Xaml.IMarkupExtension");

        public ITypeDefinition BindingExtension { get; } = new TypeDefinition("Xamarin.Forms.Xaml.BindingExtension");

        public ITypeDefinition StaticResourceExtension { get; } = new TypeDefinition("Xamarin.Forms.Xaml.StaticResourceExtension");

        public ITypeDefinition DynamicResourceExtension { get; } = new TypeDefinition("Xamarin.Forms.Xaml.DynamicResourceExtension");

        public bool SupportsBindableLayout => true;

        public ITypeDefinition BindableLayout { get; } = new TypeDefinition("Xamarin.Forms.BindableLayout");

        public bool SupportsExportFontAttribute => true;

        public ITypeDefinition ExportFontAttribute { get; } = new TypeDefinition("Xamarin.Forms.ExportFontAttribute");

        public ITypeDefinition TypeConverterAttribute { get; } = new TypeDefinition("Xamarin.Forms.TypeConverterAttribute");

        public string ItemSourceProperty { get; } = "ItemsSource";

        public string ItemTemplateProperty { get; } = "ItemTemplate";

        public ITypeDefinition Cell { get; } = new TypeDefinition("Xamarin.Forms.Cell");

        public ITypeDefinition ViewCell { get; } = new TypeDefinition("Xamarin.Forms.ViewCell");

        public ITypeDefinition DataTemplate { get; } = new TypeDefinition("Xamarin.Forms.DataTemplate");

        public bool SupportsTriggers => true;

        public ITypeDefinition TriggerBase { get; } = new TypeDefinition("Xamarin.Forms.TriggerBase");

        public ITypeDefinition DataTrigger { get; } = new TypeDefinition("Xamarin.Forms.DataTrigger");

        public ITypeDefinition MultiTrigger { get; } = new TypeDefinition("Xamarin.Forms.MultiTrigger");

        public ITypeDefinition Behaviour { get; } = new TypeDefinition("Xamarin.Forms.Behavior<T>");

        public bool SupportsOnPlatform => true;

        public ITypeDefinition OnPlatform { get; } = new TypeDefinition("Xamarin.Forms.OnPlatform<T>");

        public bool SupportsOnIdiom => true;

        public ITypeDefinition OnIdiom { get; } = new TypeDefinition("Xamarin.Forms.OnIdiom<T>");

        public ITypeDefinition On { get; } = new TypeDefinition("Xamarin.Forms.On");

        public bool SupportsDevice => true;

        public ITypeDefinition Device { get; } = new TypeDefinition("Xamarin.Forms.Device");

        public ITypeDefinition Page { get; } = new TypeDefinition("Xamarin.Forms.Page");

        public ITypeDefinition ContentPage { get; } = new TypeDefinition("Xamarin.Forms.ContentPage");

        public ITypeDefinition NavigationPage { get; } = new TypeDefinition("Xamarin.Forms.NavigationPage");

        public ITypeDefinition FlyoutPage { get; } = new TypeDefinition("Xamarin.Forms.FlyoutPage");

        public ITypeDefinition TabbedPage { get; } = new TypeDefinition("Xamarin.Forms.TabbedPage");

        public ITypeDefinition CarouselPage { get; } = new TypeDefinition("Xamarin.Forms.CarouselPage");

        public ITypeDefinition TemplatedPage { get; } = new TypeDefinition("Xamarin.Forms.TemplatedPage");

        public bool SupportsShell => true;

        public ITypeDefinition Shell { get; } = new TypeDefinition("Xamarin.Forms.Shell");

        public ITypeDefinition ShellItem { get; } = new TypeDefinition("Xamarin.Forms.ShellItem");

        public ITypeDefinition ShellContent { get; } = new TypeDefinition("Xamarin.Forms.ShellContent");

        public ITypeDefinition VisualState { get; } = new TypeDefinition("Xamarin.Forms.VisualState");

        public ITypeDefinition ImageView { get; } = new TypeDefinition("Xamarin.Forms.Image");

        public ITypeDefinition ImageSource { get; } = new TypeDefinition("Xamarin.Forms.ImageSource");

        public bool SupportsCustomRenderers => true;

        public ITypeDefinition ExportRendererAttribute { get; } = new TypeDefinition("Xamarin.Forms.ExportRendererAttribute");
    }
}
