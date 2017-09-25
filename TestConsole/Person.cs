using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ViHib.Attributes;

namespace ConsoleApp2
{
    [Table]
    public class Person
    { 
        [Id]
        [SequenceGenerator]
        public long Id;

        [Column]
        public string Name;

        [Column]
        public City city;

        [Column]
        public DopData dd;
    }
}
