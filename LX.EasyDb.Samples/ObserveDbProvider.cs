using System;

namespace LX.EasyDb.Samples
{
    class ObserveDbProvider : ISample
    {
        public void Go()
        {
            ObservableDbProvider observableDbProvider =
                DbProviderBuilder.NewBuilder(System.Data.OleDb.OleDbFactory.Instance, "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=Sample.mdb")
                .BuildObservable();
            
            /*
             * Executed operations can be observed by the Operated event.
             */
            observableDbProvider.Operating += new EventHandler<DbOperationEventArgs>(observableDbProvider_Operating);
            observableDbProvider.Transacting += new EventHandler<DbTransactionEventArgs>(observableDbProvider_Transacting);

            observableDbProvider.CreateCommand("select count(*) from `user`").ExecuteScalar();
            observableDbProvider.CreateCommand("select * from `user`").ExecuteReader(System.Data.CommandBehavior.CloseConnection).Close();
            observableDbProvider.CreateCommand("insert into `user` (`username`, `password`) values ('master', 'slave')").ExecuteNonQuery();

            ITransaction transaction = observableDbProvider.BeginTransaction();
            transaction.CreateCommand("insert into `user` (`username`, `password`) values ('transaction', 'transaction')").ExecuteNonQuery();
            transaction.Commit();
        }

        void observableDbProvider_Operating(Object sender, DbOperationEventArgs e)
        {
            Console.WriteLine("Operation: " + e.DataOperationBlock.Operation + " " + e.DataOperationBlock.CommandText);
        }

        void observableDbProvider_Transacting(object sender, DbTransactionEventArgs e)
        {
            Console.WriteLine("Transaction: " + e.Type);
            if (e.DataOperationBlock != null)
                Console.WriteLine("--Operation: " + e.DataOperationBlock.Operation + " " + e.DataOperationBlock.CommandText);
        }
    }
}
