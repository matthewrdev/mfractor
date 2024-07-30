using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.XamlPlatforms
{
    [InheritedExport]
    public interface IXamlPlatform
    {
        XamlPlatform Platform { get; }

        string SchemaUrl { get; }

        /// <summary>
        /// Is this <see cref="IXamlPlatform"/> supported in the given <paramref name="project"/> and <paramref name="compilation"/> pair.
        /// <para/>
        /// </summary>
        /// <param name="project"></param>
        /// <param name="compilation"></param>
        /// <returns></returns>
        bool IsSupported(Project project, Compilation compilation);

        bool IsSupported(Project project, Compilation compilation, IXmlSyntaxTree xmlSyntaxTree);

        bool IsSupported(IXmlSyntaxTree xmlSyntaxTree);

        string Assembly { get; }

        ITypeDefinition BindableObject { get; }

        ITypeDefinition BindableProperty { get; }

        ITypeDefinition XmlnsDefinitionAttribute { get; }

        ITypeDefinition BindingBase { get; }

        ITypeDefinition Command { get; }

        ITypeDefinition VisualElement { get; }

        ITypeDefinition Element { get; }

        ITypeDefinition Application { get; }

        ITypeDefinition ResourceDictionary { get; }

        string BindingContextProperty { get; }

        ITypeDefinition ContentPropertyAttribute { get; }

        ITypeDefinition View { get; }

        ITypeDefinition Layout { get; }

        ITypeDefinition Frame { get; }

        ITypeDefinition ContentView { get; }

        ITypeDefinition Grid { get; }

        ITypeDefinition GridLength { get; }

        ITypeDefinition RowDefinitionCollection { get; }

        ITypeDefinition ColumnDefinitionCollection { get; }

        ITypeDefinition RowDefinition { get; }

        ITypeDefinition ColumnDefinition { get; }

        string RowDefinitionsProperty { get; }

        string RowHeightProperty { get; }

        string RowProperty { get; }

        string ColumnDefinitionsProperty { get; }

        string ColumnWidthProperty { get; }

        string ColumnProperty { get; }

        string GridNamedSize_Auto { get; }

        string GridNamedSize_Star { get; }

        ITypeDefinition AbsoluteLayout { get; }

        ITypeDefinition RelativeLayout { get; }

        ITypeDefinition ScrollView { get; }

        ITypeDefinition StackLayout { get; }

        bool SupportsTypedOrientationStackLayouts { get; }

        ITypeDefinition VerticalStackLayout { get; }

        ITypeDefinition HorizontalStackLayout { get; }

        string OrientationProperty { get; }

        string StackLayoutOrientation_Horizontal { get; }

        string StackLayoutOrientation_Vertical { get; }

        ITypeDefinition FlexLayout { get; }

        bool SupportsBindableLayout { get; }

        ITypeDefinition BindableLayout { get; }

        bool SupportsExportFontAttribute { get; }

        ITypeDefinition ExportFontAttribute { get; }

        ITypeDefinition TypeConverterAttribute { get; }

        ITypeDefinition Button { get; }

        ITypeDefinition Label { get; }

        ITypeDefinition FormattedString { get; }

        ITypeDefinition Entry { get; }

        ITypeDefinition Editor { get; }

        ITypeDefinition Picker { get; }

        ITypeDefinition Slider { get; }

        bool SupportsRefreshView { get; }

        ITypeDefinition RefreshView { get; }

        bool SupportsIndicatorView { get; }

        ITypeDefinition IndicatorView { get; }

        bool SupportsStyleSheets { get; }

        ITypeDefinition StyleSheet { get; }

        bool SupportsGeometry { get; }

        ITypeDefinition PathFigureCollection { get; }

        ITypeDefinition Path { get; }

        ITypeDefinition Geometry { get; }

        string PickerItemDisplayBindingProperty { get; }

        string GroupDisplayBindingProperty { get; }

        string IsGroupingEnabledProperty { get; }

        string ItemSourceProperty { get; }

        string ItemTemplateProperty { get; }

        ITypeDefinition ListView { get; }

        ITypeDefinition CollectionView { get; }

        ITypeDefinition ValueConverter { get; }

        ITypeDefinition Cell { get; }

        ITypeDefinition ViewCell { get; }

        ITypeDefinition DataTemplate { get; }

        ITypeDefinition Color { get; }

        ITypeDefinition Thickness { get; }

        ITypeDefinition Style { get; }

        ITypeDefinition Setter { get; }

        ITypeDefinition MarkupExtension { get; }

        ITypeDefinition BindingExtension { get; }

        ITypeDefinition StaticResourceExtension { get; }

        ITypeDefinition DynamicResourceExtension { get; }

        bool SupportsTriggers { get; }

        ITypeDefinition TriggerBase { get; }

        ITypeDefinition DataTrigger { get; }

        ITypeDefinition MultiTrigger { get; }

        ITypeDefinition Behaviour { get; }

        bool SupportsOnPlatform { get; }

        ITypeDefinition OnPlatform { get; }

        bool SupportsOnIdiom { get; }

        ITypeDefinition OnIdiom { get; }

        ITypeDefinition On { get; }

        bool SupportsDevice { get; }

        ITypeDefinition Device { get; }

        ITypeDefinition Page { get; }

        ITypeDefinition ContentPage { get; }

        ITypeDefinition NavigationPage { get; }

        ITypeDefinition FlyoutPage { get; }

        ITypeDefinition TabbedPage { get; }

        ITypeDefinition CarouselPage { get; }

        ITypeDefinition TemplatedPage { get; }

        bool SupportsShell { get; }

        ITypeDefinition Shell { get; }

        ITypeDefinition ShellItem { get; }

        ITypeDefinition ShellContent { get; }

        ITypeDefinition VisualState { get; }

        ITypeDefinition ImageView { get; }

        ITypeDefinition ImageSource { get; }

        bool SupportsCustomRenderers { get; }

        ITypeDefinition ExportRendererAttribute { get; }
    }

}