using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ViHib.Attributes;

namespace ConsoleApp2
{
    [Table]
    public class City
    {
        [Id]
        public Int16 cod;
        [Column]
        public string name;
        [Column]
        public DateTime nd;
    }
}
