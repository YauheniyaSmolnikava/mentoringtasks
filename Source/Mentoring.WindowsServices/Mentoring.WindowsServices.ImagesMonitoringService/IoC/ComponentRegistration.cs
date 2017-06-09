using Castle.Core;
using Mentoring.WindowsServices.Utils.Interfaces;
using Mentoring.WindowsServices.Utils;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel;
using Mentoring.WindowsServices.Logging;

namespace Mentoring.WindowsServices.CentralizedResultsCollection.IoC
{
    public class ComponentRegistration : IRegistration
    {
        public void Register(IKernelInternal kernel)
        {
            kernel.Register(
                Component.For<Logger>()
                    .ImplementedBy<Logger>());

            kernel.Register(
                Component.For<IFileHelper>()
                         .ImplementedBy<FileHelper>()
                         .Interceptors(InterceptorReference.ForType<Logger>()).Anywhere);

            kernel.Register(
                Component.For<IAzureHelper>()
                         .ImplementedBy<AzureHelper>()
                         .Interceptors(InterceptorReference.ForType<Logger>()).Anywhere);
        }
    }
}
