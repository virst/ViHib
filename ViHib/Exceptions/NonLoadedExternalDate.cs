using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ViHib.Exceptions
{
    class NonLoadedExternalDate: HidException
    {
        public override string Message
        {
            get { return "Выженный класс не сохранен"; }
        }
    }
}
