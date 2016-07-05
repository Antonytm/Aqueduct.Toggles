using System;
using System.Collections.Generic;
using System.Linq;

namespace Aqueduct.Toggles.Sitecore
{
    public class SitecoreFeatureToggles
    {
        internal static bool ShouldReplaceLayout(Guid itemId, Guid templateId, string currentLanguage)
        {
            var replacement = GetLayoutReplacementElement(itemId, templateId, currentLanguage);

            return replacement != null;
        }

        internal static LayoutReplacement GetLayoutReplacement(Guid itemId, Guid templateId, string currentLanguage)
        {
            var replacement = GetLayoutReplacementElement(itemId, templateId, currentLanguage);
            if (replacement == null) throw new ArgumentException($"Cannot find replacement layout for item ID: {itemId}, template ID: {templateId}");

            return new LayoutReplacement
                   {
                       NewLayoutId = replacement.New,
                       LayoutId = replacement.New ?? replacement.Id,
                       Sublayouts = replacement.Renderings
                           .Select(x => new LayoutReplacement.SublayoutReplacement
                                        {
                                            SublayoutId = x.SublayoutId,
                                            Placeholder = x.PlaceHolder,
                                            NewSublayoutId = x.NewSublayoutId,
                                            NewPlaceholder = x.NewPlaceHolder
                                        })
                           .ToList()
                   };
        }

        private static LayoutToggle GetLayoutReplacementElement(Guid itemId, Guid templateId, string currentLanguage)
        {
            return GetElement(itemId, currentLanguage, x => x.Items) ??
                   GetElement(templateId, currentLanguage, x => x.Templates);
        }

        private static LayoutToggle GetElement(Guid itemId, string currentLanguage, Func<Feature, IList<LayoutToggle>> expression)
        {
            return FeatureToggles.GetAllEnabledFeatures()
                .Where(x => x.EnabledForLanguage(currentLanguage))
                .SelectMany(expression.Invoke)
                .Where(x => x != null)
                .FirstOrDefault(x => x.Id == itemId);
        }

        internal static IEnumerable<RenderingReplacement> GetAllRenderingReplacements(string currentLanguage)
        {
            var allFeatures = FeatureToggles.GetAllEnabledFeatures().Where(x => x.EnabledForLanguage(currentLanguage));

            var allSublayouts = new List<RenderingReplacement>();
            foreach (var feature in allFeatures)
            {
                var currentFeature = feature;
                var sublayouts =
                    feature.Renderings.Select(x => new RenderingReplacement
                    {
                        Enabled = currentFeature.Enabled,
                        New = x.New,
                        Original = x.Original
                    });

                allSublayouts.AddRange(sublayouts);
            }

            return allSublayouts;
        }
    }
}