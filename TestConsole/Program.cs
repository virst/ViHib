using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using ViHib;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");

            ViHibConfiguration cn = new ViHibConfiguration();
            cn.TabelPrfix = "h_demo_";
            cn.providerName = "System.Data.SqlClient";
            cn.connectionString = "Data Source=localhost;Initial Catalog=db1;Persist Security Info=True;User ID=sa;Password=root";



            var con = new ViHibConnector(cn);
            var ses = con.GetSession();

            ses.AddClass(typeof(City));
            ses.AddClass(typeof(Person));

            Person p = new Person();
         /*   p.Name = DateTime.Now.ToLongDateString();
            p.dd = new DopData()
            {
                dt1 = DateTime.Now,
                name = "Viza",
                val = 12.46f
            };
            City c = new City();
            c.cod = 9;
            c.name = "LA";
            c.nd = new DateTime(1950, 5, 6);

            p.city = c;

            ses.Save(c);
            ses.Save(p);

            Console.WriteLine("SAVED");*/

            p = ses.Load<Person>(1);
            Console.WriteLine(p.city.nd.ToLongDateString());
            Console.WriteLine(p.dd.val);
            p.Name = "12312";
            ses.Update(p);
            p = ses.Load<Person>(1);
            Console.WriteLine(p.Name);

            ses.Commit();
            Console.WriteLine("End");
            Console.ReadKey();
        }
    }
}
