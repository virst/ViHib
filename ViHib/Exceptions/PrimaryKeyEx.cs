using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ViHib.Exceptions
{
    public class PrimaryKeyException : Exception
    {
        public override string Message
        {
            get { return "Требуется первичный ключ"; }
        }
    }
}
