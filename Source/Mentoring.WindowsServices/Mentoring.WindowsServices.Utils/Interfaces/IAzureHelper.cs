using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mentoring.WindowsServices.Utils.Interfaces
{
    public interface IAzureHelper
    {
        IEnumerable<AzureMessage> ConstructAzureMessage(byte[] file);
    }
}
