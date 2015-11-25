using Microsoft.ServiceBus.Messaging;
using ProcessMonitorPoC.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessInformationInfoSender
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: insert connection string and queue name
            var q = QueueClient.CreateFromConnectionString("<connection_string>", "<queue_name>");
            var stop = 0;
            var t = Task.Run(() => Loop(q, ref stop));

            Console.WriteLine("Spamming. Hit enter to stop.");
            Console.ReadLine();
            Console.WriteLine("Stopping");
            Interlocked.Exchange(ref stop, 1);
            t.Wait();
        }

        private static Task Loop(QueueClient q, ref int stop)
        {
            while (stop == 0)
            {
                q.SendBatch(GeneratedMessages(200));
            }

            return Task.FromResult(0);
        }

        private static IEnumerable<BrokeredMessage> GeneratedMessages(int n)
        {
            var rng = new Random();
            return Enumerable.Range(0, n)
                .Select(_ => new ProcessInformation
                {
                    CpuTime = TimeSpan.FromMinutes(rng.NextDouble() * 60.0f),
                    Installed = DateTime.UtcNow - TimeSpan.FromDays(7) - TimeSpan.FromDays(rng.NextDouble() * 30.0f),
                    LastUsed = DateTime.UtcNow - TimeSpan.FromDays(rng.NextDouble() * 7.0f),
                    Name = Guid.NewGuid().ToString()
                })
                .Select(p => new BrokeredMessage(p));
        }
    }
}
