using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ViHib.Exceptions
{
    class NonLOadadTypeError : Exception
    {
        public override string Message
        {
            get { return "Попытка работы с незагруженным типом"; }
        }
    }
}
