using System;

namespace LX.EasyDb.Samples
{
    class Program
    {
        static void Main(String[] args)
        {
            new HelloEasyDb().Go();
            new UsingEasyDbHelper().Go();
            new ObserveDbProvider().Go();
            new MasterSlaveDb().Go();
            new UsingDbProvider().Go();
            new SupportMySQL().Go();
            Console.ReadLine();
        }
    }
}
