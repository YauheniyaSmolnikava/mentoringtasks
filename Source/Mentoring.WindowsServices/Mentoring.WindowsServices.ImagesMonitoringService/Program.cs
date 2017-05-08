using System.Diagnostics;
using System.IO;
using Topshelf;

namespace Mentoring.WindowsServices.ImagesMonitoringService
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(
                    serv =>
                    {
                        serv.Service<FilesGluingService>();
                        serv.SetServiceName("FilesGluingService");
                        serv.SetDisplayName("Images Monitoring Service");
                        serv.StartAutomaticallyDelayed();
                        serv.RunAsLocalService();
                    }
                );
        }
    }
}
