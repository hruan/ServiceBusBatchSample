using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System.Threading;
using ProcessMonitorPoC.Common;
using System.Diagnostics;
using System.Collections.Generic;

namespace ProcessMonitorInfoWriterJob
{
    public class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            // TODO: insert connection string and queue name
            var q = QueueClient.CreateFromConnectionString("<connection_string>", "<queue_name>");

            var t = Task.Run(() => Loop(q, cts.Token));
            Console.WriteLine("Running. Hit enter to stop.");
            Console.ReadLine();

            cts.Cancel();
            Console.WriteLine("Waiting for cancellation to complete.");
            t.Wait();
            Console.WriteLine("Done");
        }

        private static async Task Loop(QueueClient q, CancellationToken t)
        {
            var bs = new List<double>();
            var sw = new Stopwatch();
            sw.Start();
            while (!t.IsCancellationRequested)
            {
                var msgs = await q.ReceiveBatchAsync(200, TimeSpan.FromSeconds(30));
                var completes = msgs.Select(m =>
                {
                    var p = m.GetBody<ProcessInformation>();
                    return m.CompleteAsync();
                });
                await Task.WhenAll(completes);

                sw.Stop();
                var batch = (double)msgs.Count() / (sw.ElapsedMilliseconds / 1000f);
                bs.Add(batch);
                Console.WriteLine($"Batch:   {batch:f2} msgs/s.");
                Console.WriteLine($"Average: {bs.Average():f2} msgs/s.");
                sw.Restart();
            }
        }
    }
}
