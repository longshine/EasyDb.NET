using System;
using System.Data;

namespace LX.EasyDb.Samples
{
    class HelloEasyDb : ISample
    {
        public void Go()
        {
            /*
             * EasyDbHelper will be initialized with App.config by default.
             * Then, it is as easy as below to use.
             */
            Console.WriteLine("*** Running SQL statement with EasyDbHelper ***");

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

            /*
             * Or, using EasyDbHelper.CreateConnection() to get an original connection.
             * The rest of the code seems to be familiar.
             */
            Console.WriteLine("*** Running SQL statement with original connection ***");

            IDbConnection conn = EasyDbHelper.GetConnection();
            conn.Open();
            IDbCommand comm = conn.CreateCommand();
            comm.CommandText = "select * from `user`";
            using (IDataReader reader = comm.ExecuteReader())
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
            conn.Close();
        }
    }
}
