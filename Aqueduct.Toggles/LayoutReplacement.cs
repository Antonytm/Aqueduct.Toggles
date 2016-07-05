using System;
using System.Collections.Generic;

namespace Aqueduct.Toggles
{
    public class LayoutReplacement
    {
        public Guid? NewLayoutId { get; set; }
        public Guid LayoutId { get; set; }

        public IList<SublayoutReplacement> Sublayouts { get; set; }

        public class SublayoutReplacement
        {
            public Guid SublayoutId { get; set; }
            public Guid NewSublayoutId { get; set; }
            public string Placeholder { get; set; }
            public string NewPlaceholder { get; set; }
        }
    }
}