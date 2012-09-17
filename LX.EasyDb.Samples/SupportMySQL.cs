using System;
using System.Data;
using System.Collections.Generic;
using System.Collections;

namespace LX.EasyDb.Samples
{
    class SupportMySQL : ISample
    {
        public void Go()
        {
            IDbProvider provider = null;

            /*
             * Follow these steps to support a custom database:
             * 1. Get a client library of the database for .NET, i.e., mysql.data.dll.
             * 2. Build the provider with full name of the client library,
             * 3. Or with a insance of the factory class which extends System.Data.Common.DbProviderFactory.
             * 4. Do same things as usual!
             */
            provider = DbProviderBuilder.NewBuilder("MySql.Data.MySqlClient, mysql.data").Build();
            provider = DbProviderBuilder.NewBuilder(new MySql.Data.MySqlClient.MySqlClientFactory()).Build();
            provider.ConnectionString = "Server=127.0.0.1;Uid=root;Pwd=asdf;Database=sample;";

            String sql = "select * from user where id=@id";
            IDbDataParameter[] ps = new IDbDataParameter[]{
                provider.CreateParameter("id", "2")
            };

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
    }
}
