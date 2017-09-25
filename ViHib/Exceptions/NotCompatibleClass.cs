using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ViHib.Exceptions
{
    public class NotCompatibleClass : Exception
    {
        public override string Message
        {
            get { return "Не совместимый класс."; }
        }
    }
}
