using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace ViHib
{
    public class ViHibConnector
    {
        private ViHibConfiguration conf;

               public ViHibConnector(ViHibConfiguration c)
        {
            conf = c;
        }

        public ViHibSession GetSession()
        {
            return new ViHibSession(conf);
        }
    }
}
