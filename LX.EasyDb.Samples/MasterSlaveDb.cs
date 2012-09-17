using System;
using System.Data;

namespace LX.EasyDb.Samples
{
    class MasterSlaveDb : ISample
    {
        public void Go()
        {
            MasterSlaveDbProvider masterSlaveProvider = DbProviderBuilder
                .NewBuilder(System.Data.OleDb.OleDbFactory.Instance, "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Sample.mdb")
                .AddSlave(System.Data.OleDb.OleDbFactory.Instance, "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=SampleSlave.mdb", "Slave1")
                //.AddSlave(System.Data.SqlClient.SqlClientFactory.Instance, "Data Source=localhost;Initial Catalog=Sample;Integrated Security=True", "Slave2")
                .BuildMasterSlave();

            /* 
             * Slaves are running in background, so exceptions on them cannot be catched directly,
             * but by the SlaveException event.
             */
            masterSlaveProvider.SlaveException += new EventHandler<DbExceptionEventArgs>(OnSlaveException);

            IDbOperation operation = masterSlaveProvider;

            ITransaction transaction = masterSlaveProvider.BeginTransaction();
            operation = transaction;

            /* This statement will be performed both on master and slaves. */
            operation.CreateCommand("insert into [user] ([username], [password]) values ('master', 'slave')").ExecuteNonQuery();

            Console.WriteLine("*** Table user ***");
            using (IDataReader reader = operation.CreateCommand("select * from [user]").ExecuteReader())
            {
                PrintAndClose(reader);
            }

            /* This statement will cause a exception on Slave1 since the table doesn't exist there. */
            operation.CreateCommand("insert into [user1] ([username], [password]) values ('master', 'slave_got_exception')").ExecuteNonQuery();

            Console.WriteLine("*** Table user1 ***");
            using (IDataReader reader = operation.CreateCommand("select * from [user1]").ExecuteReader())
            {
                PrintAndClose(reader);
            }

            transaction.Commit();
        }

        void OnSlaveException(Object sender, DbExceptionEventArgs e)
        {
            IDbProvider provider = (IDbProvider)sender;
            Console.WriteLine("Exception occured on {0}.", provider.Name);
            Console.WriteLine("Statement: {0}", e.DataOperationBlock.CommandText);
            Console.WriteLine("Exception: {0}", e.Exception.Message);
        }

        void PrintAndClose(IDataReader reader)
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
