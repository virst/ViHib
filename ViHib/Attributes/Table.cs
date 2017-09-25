using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ViHib.Attributes
{
    public class Table : Attribute
    {
        public string Name { get; set; } = null;
        public bool force { get; set; } = false;
    }
}
