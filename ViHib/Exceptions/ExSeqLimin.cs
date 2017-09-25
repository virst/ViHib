using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ViHib.Exceptions
{
    class ExSeqLimit : Exception
    {
        public override string Message
        {
            get { return "Достигнут максимум последовательнсти"; }
        }
    }
}
