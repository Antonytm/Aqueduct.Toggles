using System;
using System.Collections.Generic;
using System.Linq;
using Aqueduct.Toggles.Configuration;
using Aqueduct.Toggles.Overrides;

namespace Aqueduct.Toggles
{
    internal class FeatureConfiguration
    {
        private IList<Feature> _features = new List<Feature>();
        internal IOverrideProvider Provider;

        internal void LoadFromConfiguration(FeatureToggleConfigurationSection config)
        {
            _features = config.Features.Cast<FeatureToggleConfigurationElement>().Select(Feature.FromConfig).ToList();
        }

        internal void SetOverrideProvider(IOverrideProvider provider)
        {
            Provider = provider;
        }
        
        public IEnumerable<Feature> AllFeatures => _features;

        public IEnumerable<Feature> EnabledFeatures => _features.Where(IsEnabled);

        public bool IsEnabled(string name)
        {
            var overrides = Provider.GetOverrides();
            if (overrides.ContainsKey(name)) return overrides[name];

            return IsEnabled(GetFeature(name));
        }

        public bool IsEnabled(Feature feature)
        {
            if (feature == null)
                return false;

            var overrides = Provider.GetOverrides();
            return overrides.ContainsKey(feature.Name) ? overrides[feature.Name] : feature.Enabled;
        }

        public Feature GetFeature(string name)
        {
            return _features.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}