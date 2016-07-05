using System;
using Aqueduct.Toggles.Configuration.Layouts;

namespace Aqueduct.Toggles
{
    public class LayoutToggleRendering
    {
        public string Name { get; set; }
        public string PlaceHolder { get; set; }
        public string NewPlaceHolder { get; set; }
        public Guid SublayoutId { get; set; }
        public Guid NewSublayoutId { get; set; }

        internal static LayoutToggleRendering FromConfig(FeatureToggleLayoutRenderingConfigurationElement element)
        {
            return new LayoutToggleRendering
            {
                Name = element.Name,
                PlaceHolder = element.Placeholder,
                SublayoutId = element.SublayoutId,
                NewPlaceHolder = element.NewPlaceholder,
                NewSublayoutId = element.NewSublayoutId
            };
        }
    }
}