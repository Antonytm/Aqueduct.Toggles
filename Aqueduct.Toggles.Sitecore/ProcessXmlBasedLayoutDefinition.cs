using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Mvc.Configuration;
using Sitecore.Mvc.Extensions;
using Sitecore.Mvc.Pipelines;
using Sitecore.Mvc.Pipelines.Response.BuildPageDefinition;
using Sitecore.Mvc.Pipelines.Response.GetXmlBasedLayoutDefinition;
using Sitecore.Mvc.Presentation;

namespace Aqueduct.Toggles.Sitecore
{
    public class ProcessXmlBasedLayoutDefinition :
        global::Sitecore.Mvc.Pipelines.Response.BuildPageDefinition.ProcessXmlBasedLayoutDefinition
    {
        private Guid _layoutId { get; set; }
        private IList<LayoutReplacement.SublayoutReplacement> _sublayoutReplacements { get; set; }

        public override void Process(BuildPageDefinitionArgs args)
        {
            Assert.ArgumentNotNull((object) args, "args");
      
            var item = Context.Item;
            if (item == null) return;

            var itemId = item.ID.Guid;
            var templateId = item.Template.ID.Guid;

            var currentLanguage = Context.Language.Name;
            var renderingBuilder = new RenderingBuilder();
            PageDefinition pageDefinition = args.Result;

            if (pageDefinition == null)
                return;

            if (SitecoreFeatureToggles.ShouldReplaceLayout(itemId, templateId, currentLanguage))
            {
                //resolve all the renderings from the featuretoggle
                var layoutReplacement = SitecoreFeatureToggles.GetLayoutReplacement(itemId, templateId, currentLanguage);
                _layoutId = layoutReplacement.LayoutId;
                var layoutRendering = renderingBuilder.GetRenederingById(layoutReplacement.LayoutId);
                layoutRendering.RenderingType = "Layout";
                pageDefinition.Renderings.Add(layoutRendering);
                _sublayoutReplacements = layoutReplacement.Sublayouts;
                this.AddRenderings(pageDefinition, args);
            }
            else
            {
                base.Process(args);
                var renderingReplacements = SitecoreFeatureToggles.GetAllRenderingReplacements(currentLanguage);
                //loop through and replace the one I need to replace
                foreach (var replacement in renderingReplacements)
                {
                    ProcessRenderings(pageDefinition.Renderings, replacement, renderingBuilder);
                }
            }

        }

        protected override Rendering GetRendering(System.Xml.Linq.XElement renderingNode, System.Guid deviceId, System.Guid layoutId, string renderingType, XmlBasedRenderingParser parser)
        {
            var id = renderingNode.GetAttributeValueOrNull("id");
            var placecholder = renderingNode.GetAttributeValueOrNull("ph");
            if (_sublayoutReplacements != null)
            {
                var replacementRule =
                    _sublayoutReplacements.FirstOrDefault(x =>
                        String.Equals(x.Placeholder, placecholder, StringComparison.InvariantCultureIgnoreCase) && x.SublayoutId.Equals(new Guid(id)));
                if (replacementRule != null)
                {
                    renderingNode.SetAttributeValue("id", replacementRule.NewSublayoutId);
                    renderingNode.SetAttributeValue("ph", replacementRule.NewPlaceholder);
                }
            }

            Rendering rendering = parser.Parse(renderingNode, false);
            rendering.DeviceId = deviceId;
            rendering.LayoutId = _layoutId != Guid.Empty ? _layoutId: layoutId;
            if (renderingType != null)
            {
                rendering.RenderingType = renderingType;
            }
            
            return rendering;
        }

        private void ProcessRenderings(List<Rendering> renderings, RenderingReplacement replacement,
            RenderingBuilder renderingBuilder)
        {
            for (var i = 0; i < renderings.Count; i++)
            {
                var rendering = renderings[i];
                if (rendering.RenderingItem.ID.Guid == replacement.Original)
                {
                    var newRendering = renderingBuilder.GetRenederingById(replacement.New);
                    newRendering.Placeholder = rendering.Placeholder;
                    newRendering.DeviceId = rendering.DeviceId;
                    newRendering.LayoutId = rendering.LayoutId;
                    renderings[i] = newRendering;
                }

                if(rendering.ChildRenderings.Any())
                    ProcessRenderings(rendering.ChildRenderings, replacement, renderingBuilder);
            }

        }
        
        private class RenderingBuilder : XmlBasedRenderingParser
        {
            public Rendering GetRenederingById(Guid id)
            {
                var rendering = new Rendering() {RenderingItemPath = id.ToString()};
                rendering.UniqueId = Guid.NewGuid();
                base.AddRenderingItemProperties(rendering);
                return rendering;
            }
        }
    }
}