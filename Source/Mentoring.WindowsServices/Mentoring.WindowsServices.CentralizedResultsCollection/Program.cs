using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mentoring.WindowsServices.Utils;
using Microsoft.ServiceBus.Messaging;
using Topshelf;

namespace Mentoring.WindowsServices.CentralizedResultsCollection
{
    class Program
    {
        static void Main(string[] args)
        {
           // SendBatch();

            HostFactory.Run(
                    serv =>
                    {
                        serv.Service<QueueListener>();
                        serv.SetServiceName("QueueListener");
                        serv.SetDisplayName("Azure Queue Listener");
                        serv.StartAutomaticallyDelayed();
                        serv.RunAsLocalService();
                    }
                );
            //SendBatch();
        }

        private static void SendBatch()
        {
            var client = QueueClient.Create("FileQueue");
            byte[] bytes = System.IO.File.ReadAllBytes(@"D:\Smth\111.pdf");

            var key = Guid.NewGuid();
            var messages = AzureHelper.ConstructAzureMessage(bytes);
            IEnumerable<BrokeredMessage> brokeredMessages = messages
                .Select(msg => new BrokeredMessage(msg) { PartitionKey = key.ToString() });

            foreach (var msg in brokeredMessages)
            {
                client.Send(msg);
            }
        }
    }
}
