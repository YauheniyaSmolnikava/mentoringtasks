using System;

namespace Mentoring.WindowsServices.Utils
{
    [Serializable]
    public class AzureMessage
    {
        public int Amount;

        public int SequenceNumber;

        public byte[] Body;
    }
}
