using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.DataBaseEF
{
    using System;
    using System.Data.Common;

    namespace OdpTest
    {
        class Program
        {
            void Main(string connection)
            {
                string connectionString =
                    "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCgOL=TCP)" +
                    "(HOST=MYHOST)(PORT=1527))(CONNECT_DATA=(SID=MYSERVICE)));" +
                    "User Id=MYUSER;Password=MYPASS;";
                string provider =
                    "Oracle.DataAccess.Client.OracleConnection, Oracle.DataAccess";

                using (DbConnection conn = (DbConnection)Activator.
                    CreateInstance(Type.GetType(provider), connectionString))
                {
                    conn.Open();
                    string sql =
                        "select distinct owner from sys.all_objects order by owner";
                    using (DbCommand comm = conn.CreateCommand())
                    {
                        comm.CommandText = sql;
                        using (DbDataReader rdr = comm.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                string owner = rdr.GetString(0);
                                Console.WriteLine("{0}", owner);
                            }
                        }
                    }
                }
            }
        }
    }
}
