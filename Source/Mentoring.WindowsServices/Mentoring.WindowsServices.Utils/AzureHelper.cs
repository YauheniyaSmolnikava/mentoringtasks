﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mentoring.WindowsServices.Logging;
using Mentoring.WindowsServices.Utils.Interfaces;

namespace Mentoring.WindowsServices.Utils
{
    public class AzureHelper : IAzureHelper
    {
        private const int msgBodySize = 100;

        private const int msgByteSize = 100 * 1024;

        public IEnumerable<AzureMessage> ConstructAzureMessage(byte[] file)
        {
            var azureMessages = new List<AzureMessage>();

            int messagesCount = (int)Math.Ceiling((double)file.Length / msgByteSize);

            for(int i = 0; i < messagesCount; i++)
            {
                var azureMsg = new AzureMessage { Amount = messagesCount, SequenceNumber = i };

                if (i == 0)
                {
                    azureMsg.Body = file.Take(msgByteSize).ToArray();
                }
                else
                {

                    if (i == messagesCount - 1)
                    {
                        azureMsg.Body = file.Skip(msgByteSize * i).ToArray();
                    }
                    else
                    {
                        azureMsg.Body = file.Skip(msgByteSize * i).Take(msgByteSize).ToArray();
                    }
                }

                azureMessages.Add(azureMsg);
            }

            return azureMessages;
        }
    }
}
