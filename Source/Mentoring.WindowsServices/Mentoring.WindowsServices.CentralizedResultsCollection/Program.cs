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
        }
    }
}
