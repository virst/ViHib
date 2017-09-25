using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ViHib
{
    public class ViHibConfiguration
    {
        private Dictionary<string, string> typeSqlCon = new Dictionary<string, string>();
        private string clob;

        public string connectionString;
        string _providerName;

        public string providerName
        {
            get { return _providerName; }
            set { _providerName = value; }
        }

        public string TabelPrfix = "";
        public string _parPre = "@";

        public string parPre
        {
            get { return _parPre; }
        }

        public ViHibConfiguration()
        {
            typeSqlCon.Add(typeof(Int16).ToString(), "int");
            typeSqlCon.Add(typeof(Int32).ToString(), "int");
            typeSqlCon.Add(typeof(Int64).ToString(), "numeric(18, 0)");
            typeSqlCon.Add(typeof(Double).ToString(), "numeric(18, 10)");
            typeSqlCon.Add(typeof(float).ToString(), "numeric(8, 8)");
            typeSqlCon.Add(typeof(string).ToString(), "nvarchar(MAX)");
            typeSqlCon.Add(typeof(DateTime).ToString(), "nvarchar(MAX)");

            clob = "xml";
        }

        public string GetSqlCon(string t)
        {
            if (typeSqlCon.ContainsKey(t))
                return typeSqlCon[t];
            return clob;
        }

        public bool CompareType(string t)
        {
            return typeSqlCon.ContainsKey(t);
        }
    }
}
