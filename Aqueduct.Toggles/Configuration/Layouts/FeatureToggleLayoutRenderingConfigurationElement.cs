﻿using System;
using System.Configuration;

namespace Aqueduct.Toggles.Configuration.Layouts
{
    internal class FeatureToggleLayoutRenderingConfigurationElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        internal string Name => (string)this["name"];

        [ConfigurationProperty("placeholder")]
        internal string Placeholder => (string)this["placeholder"];

        [ConfigurationProperty("id", IsRequired = true)]
        internal Guid SublayoutId => (Guid)this["id"];
    }
}
