using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ViHib.Exceptions
{
    public class ErrorSeqColumnType : Exception
    {
        public override string Message
        {
            get { return "Поле SequenceGenerator должно иметь тип Int64"; }
        }
    }
}
