using System;
using System.Reflection;

namespace LX.EasyDb.Samples
{
    class Program
    {
        static void Main(String[] args)
        {
            RunSamples();
            Console.WriteLine("(end of samples; press any key)");
            Console.ReadKey();
        }

        static void RunSamples()
        {
            var samples = new Samples();
            int fail = 0;
            MethodInfo[] methods = typeof(Samples).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var activeTests = Array.FindAll(methods, delegate(MethodInfo m) { return Attribute.IsDefined(m, typeof(ActiveAttribute)); });
            if (activeTests.Length != 0) methods = activeTests;
            foreach (var method in methods)
            {
                Console.WriteLine("Running " + method.Name);
                try
                {
                    method.Invoke(samples, null);
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    fail++;
                    if (ex.InnerException == null)
                        Console.WriteLine(" - " + ex.Message);
                    else if (ex.InnerException.InnerException == null)
                        Console.WriteLine(" - " + ex.InnerException.Message);
                    else
                        Console.WriteLine(" - " + ex.InnerException.InnerException.Message);
                }
            }
            Console.WriteLine();
        }

        static IConnectionFactory factory;
        static Program()
        {
            factory = ConnectionFactoryBuilder.NewBuilder(MySql.Data.MySqlClient.MySqlClientFactory.Instance,
                "Server=127.0.0.1;Uid=root;Pwd=asdf;Database=sample;", null,
                new Dialect.MySQLDialect()).Build();
        }

        internal static IConnection GetOpenConnection()
        {
            return factory.OpenConnection();
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    sealed class ActiveAttribute : Attribute { }
}
