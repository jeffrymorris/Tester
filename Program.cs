using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Annotations;
using Couchbase.Configuration.Client;
using Couchbase.IO;
using Couchbase.N1QL;
using Couchbase.Views;

namespace Tester
{
    class Program
    {
        public static string BucketName = "default";
        public static string[] payload = new string[10];
        static void Main(string[] args)
        {
            var config = TestConfiguration.GetConfiguration("basic");
            config.UseConnectionPooling = false;
            config.BucketConfigs["default"].PoolConfiguration.MaxSize = 5;
            config.BufferSize = 1024 * 1024;
            ClusterHelper.Initialize(config);

            var threads = new List<Thread>();
            for (int i = 0; i < 10; i++)
            {
                threads.Add(new Thread(DoWorkAsync));
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
            Console.Read();
            ClusterHelper.Close();
        }

        static async void DoWorkAsync(object state)
        {
            using (var bucket = ClusterHelper.GetBucket(BucketName))
            {
                for (int x = 0; x < 1000000; x++)
                {
                    var tasks = new List<Task<IOperationResult<string>>>(100);
                    for (int i = 0; i < 10; i++)
                    {
                        tasks.Add(bucket.UpsertAsync(i.ToString(), i.ToString()));
                        tasks.Add(bucket.GetAsync<string>(i.ToString()));
                    }
                    var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                    Console.WriteLine("Completing loop #{0}", x);
                    Display(results);
                }
            }
            Console.WriteLine("done");
        }

        static async void DoWorkAsync2(object state)
        {
            var bucket = ClusterHelper.GetBucket(BucketName);
            for (int x = 0; x < 1000000; x++)
            {
                var tasks = new List<Task<IViewResult<dynamic>>>(100);
                for (int i = 0; i < 10; i++)
                {
                    var query = new ViewQuery().From("test", "testView").Limit(1);
                    tasks.Add(bucket.QueryAsync<dynamic>(query));
                }
                var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                Display(results);
                Thread.Sleep(60);
                Console.WriteLine("Completing loop #{0}", x);
            }
            Console.WriteLine("done");
        }


        static async void DoQueryWorkAsync(object state)
        {
            var bucket = ClusterHelper.GetBucket(BucketName);
            for (int x = 0; x < 1000000; x++)
            {
                var tasks = new List<Task<IQueryResult<dynamic>>>(100);
                for (int i = 0; i < 10; i++)
                {
                    var query = new QueryRequest("SELECT * FROM `default` LIMIT 1;");
                    tasks.Add(bucket.QueryAsync<dynamic>(query));
                }
                var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                Display(results);
                //Thread.Sleep(60);
                Console.WriteLine("Completing loop #{0}", x);
            }
            Console.WriteLine("done");
        }

        static void DoWork(object state)
        {

            var bucket = ClusterHelper.GetBucket(BucketName);
            for (int x = 0; x < 1000000; x++)
            {
                var results = new List<IOperationResult<string[]>>(1000);
                for (int i = 0; i < 10; i++)
                {
                    results.Add(bucket.Upsert<string[]>(i.ToString(), payload));
                    results.Add(bucket.Get<string[]>(i.ToString()));
                }

               // Console.WriteLine("Completing loop #{0}", x);
                //Display(results.ToArray());
               // Thread.Sleep(50);
            }
            Console.WriteLine("done");
        }

        static void DoWorkSubDoc(object state)
        {
            var bucket = ClusterHelper.GetBucket(BucketName);
            for (int x = 0; x < 1000000; x++)
            {
                for (int i = 0; i < 10; i++)
                {
                    var result = bucket.Upsert(i.ToString(), "{\"path\":[]}");
                    if (!result.Success)
                    {
                        Console.WriteLine(result.Status);
                    }
                }
                Console.WriteLine("Completing loop #{0}", x);

                for (int i = 0; i < 10; i++)
                {
                    bucket.Upsert(i.ToString(), "{\"path\":[]}");
                }
                Thread.Sleep(50);
            }
            Console.WriteLine("done");
        }

        static async void LoadDocsAsync(object state)
        {
            var bucket = ClusterHelper.GetBucket(BucketName);
            for (int x = 0; x < 1; x++)
            {
                var tasks = new List<Task<IOperationResult<string>>>(10);
                for (int i = 0; i < 10; i++)
                {
                    tasks.Add(bucket.UpsertAsync(i.ToString(), "{\"path\":[]}"));
                }
                var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                Console.WriteLine("Completing loop #{0}", x);
                Display(results);
                Thread.Sleep(50);
            }
            Console.WriteLine("done");
        }

        static void Display(IOperationResult<string[]>[] results)
        {
            foreach (var operationResult in results)
            {
                if (!operationResult.Success)
                {
                    Console.WriteLine(operationResult.Status);
                }
            }
        }

        static void Display(IOperationResult<string>[] results)
        {
            foreach (var operationResult in results)
            {
                if (!operationResult.Success)
                {
                    Console.WriteLine(operationResult.Status);
                }
            }
        }

        static void Display(IOperationResult<int>[] results)
        {
            foreach (var operationResult in results)
            {
                if (!operationResult.Success)
                {
                    Console.WriteLine(operationResult.Status);
                }
            }
        }

        static void Display(IViewResult<dynamic>[] results)
        {
            foreach (var operationResult in results)
            {
                if (!operationResult.Success)
                {
                    Console.WriteLine(operationResult.Error);
                }
            }
        }

        static void Display(IQueryResult<dynamic>[] results)
        {
            foreach (var operationResult in results)
            {
                if (!operationResult.Success)
                {
                    Console.WriteLine(operationResult.Status);
                }
            }
        }
    }
}
