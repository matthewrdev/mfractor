using MFractor.Maui.Styles;

namespace MFractor.Maui.Analysis.Styles
{
    class PropertyValueIsAlreadyAppliedByStyleBundle
    {
        public PropertyValueIsAlreadyAppliedByStyleBundle(IStyle style, IStyleProperty property)
        {
            Style = style;
            Property = property;
        }

        public IStyle Style { get; }

        public IStyleProperty Property { get; }
    }
}
