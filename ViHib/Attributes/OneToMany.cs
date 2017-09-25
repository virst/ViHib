using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ViHib.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class OneToMany : Attribute
    {
                
    }
}
