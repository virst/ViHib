using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ViHib.Exceptions
{
   public class TypeNameError : Exception
    {
        public override string Message
        {
            get { return "Наименование  ViHid запрещено."; }
        }
    }
}
