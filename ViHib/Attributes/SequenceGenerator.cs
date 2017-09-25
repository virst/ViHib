using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ViHib.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class SequenceGenerator : Attribute
    {
        public string Name { get; set; } = null;

        public long MinValue { get; set; } = 1;

        public long MaxValue { get; set; } = long.MaxValue;

        public long Incriment { get; set; } = 1;


    }
}
