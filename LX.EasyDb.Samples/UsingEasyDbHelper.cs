using System;
using System.Data;
using LX.EasyDb.Configuration;

namespace LX.EasyDb.Samples
{
    class UsingEasyDbHelper : ISample
    {
        public void Go()
        {
            InitDbHelper();
            RunSQL();
            RunParameterizedSQL();
            UsingTransaction();
            CreateDbDataAdapter();
        }

        private void UsingTransaction()
        {
            Console.WriteLine("*** Begining transaction ***");

            ITransaction transaction = EasyDbHelper.BeginTransaction();
            transaction.CreateCommand("update `user` set `password` = 'p1' where `id` = 1").ExecuteNonQuery();
            transaction.CreateCommand("update `user` set `password` = 'p2' where `id` = 2").ExecuteNonQuery();
            transaction.Commit();
        }

        private void InitDbHelper()
        {
            Console.WriteLine("*** Initializing EasyDbHelper ***");

            /*
             * EasyDbHelper will be initialized automatically with App.config if exists.
             * Otherwise, it must be initialized manually before being used.
             */

            /* The initialization may take a provider and connectionString as parameters: */
            EasyDbHelper.Initialize("System.Data.OleDb, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Sample.mdb");

            /* Or by EasyDbConfiguration. */
            EasyDbConfiguration config = new EasyDbConfiguration();
            config.Providers.Add(new DbProviderElement("", "System.Data.OleDb, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Sample.mdb"));
            EasyDbHelper.Initialize(config);
        }

        private void RunSQL()
        {
            Console.WriteLine("*** Running SQL statement ***");

            Console.WriteLine("Count: {0}", EasyDbHelper.ExecuteScalar("select count(*) from `user`"));

            using (IDataReader reader = EasyDbHelper.ExecuteReader("select * from `user`"))
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

        private void RunParameterizedSQL()
        {
            Console.WriteLine("*** Running parameterized SQL statement ***");

            String sql = "select * from `user` where id = @id";
            IDbDataParameter[] ps = new IDbDataParameter[] { 
                // FIXME: Parameter name seems to be useless. Only its index is concerned.
                EasyDbHelper.CreateParameter("id", 2),
            };

            using (IDataReader reader = EasyDbHelper.ExecuteReader(sql, ps))
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

        private void CreateDbDataAdapter()
        {
            Console.WriteLine("*** Creating IDbDataAdapter ***");

            IDbDataAdapter ada = EasyDbHelper.CreateDataAdapter("select * from `user`", null, CommandType.Text);
            DataSet ds = new DataSet();
            ada.Fill(ds);
            foreach (DataTable dt in ds.Tables)
            {
                foreach (DataRow row in dt.Rows)
                {
                    foreach (var item in row.ItemArray)
                    {
                        Console.Write(item);
                        Console.Write('\t');
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
