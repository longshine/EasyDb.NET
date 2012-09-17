using System;
using System.Data;

namespace LX.EasyDb.Samples
{
    class UsingDbProvider : ISample
    {
        public void Go()
        {
            IDbProvider provider = null;

            /* Initializes an appropriate provider. */

            //provider = new DbProvider(System.Data.SqlClient.SqlClientFactory.Instance);
            //provider.ConnectionString = "Data Source=localhost;Initial Catalog=Test;Integrated Security=True";

            //provider = DbProviderBuilder.NewBuilder("System.Data.Odbc, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").Build();

            provider = DbProviderBuilder.NewBuilder(System.Data.OleDb.OleDbFactory.Instance).Build();
            provider.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Sample.mdb";
            provider.Name = "Db Provider";

            /* 
             * Prepare statement and parameters.
             * NOTE: To be compatible with different databases, the '@' is   in parameterized queries,
             */
            String sql = String.Format("select * from `user` where id=@id");
            IDbDataParameter[] ps = new IDbDataParameter[]{
                provider.CreateParameter("id", "1")
            };

            try
            {
                using (IDataReader reader = provider.CreateCommand(sql, ps).ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write(reader[i]);
                            Console.Write('\t');
                        }
                        Console.WriteLine();
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
