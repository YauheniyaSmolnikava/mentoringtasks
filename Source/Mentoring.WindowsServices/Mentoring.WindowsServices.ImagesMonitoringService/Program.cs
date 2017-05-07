using System.Diagnostics;
using System.IO;
using Topshelf;

namespace Mentoring.WindowsServices.ImagesMonitoringService
{
    class Program
    {
        static void Main(string[] args)
        {
            var curDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            var inDir = Path.Combine(curDir, "in");
            var outSuccessDir = Path.Combine(curDir, "successOut");
            var outFailedDir = Path.Combine(curDir, "failedOut");

            HostFactory.Run(
                conf => conf.Service<FilesGluingService>(
                    serv =>
                    {
                        serv.ConstructUsing(() => new FilesGluingService(inDir, outSuccessDir, outFailedDir));
                        serv.WhenStarted(s => s.Start());
                        serv.WhenStopped(s => s.Stop());
                    }
                    )
                );
        }
    }
}
