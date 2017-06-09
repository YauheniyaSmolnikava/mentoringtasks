using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using Mentoring.WindowsServices.Utils;
using Microsoft.ServiceBus.Messaging;
using Topshelf;
using Newtonsoft.Json;
using Mentoring.WindowsServices.Logging;

namespace Mentoring.WindowsServices.CentralizedResultsCollection
{
    public class QueueListener : ServiceControl
    {
        #region Fields

        string outSuccessDir;
        Thread workingThread;
        Thread serviceStatusThread;
        Settings settings;
        System.Timers.Timer timer;

        string curDir;
        string queueName = "FileQueue";
        string topicName = "ServiceSettingsTopic";
        string subscriptonName = "ServiceStatusSubscription";
        string subscriptionTopicName = "ServiceStatusTopic";
        string statusFileName = "CurrentServiceStatus.txt";
        string fileSettingsName = "Settings.txt";

        AzureMessage[] messageParts;
        bool stop  = false;

        QueueClient client;
        SubscriptionClient subscriptionClient;
        TopicClient topicClient;

        #endregion

        #region Public Methods and Constructors

        public QueueListener()
        {
            curDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            outSuccessDir = Path.Combine(curDir, "successOut");

            if (!Directory.Exists(outSuccessDir))
                Directory.CreateDirectory(outSuccessDir);

            settings = new Settings { UpdateStatus = false, Timeout = 15000, Barcode = "Document Breaker" };
            if (!File.Exists(Path.Combine(curDir, fileSettingsName)))
            {
                using (StreamWriter sw = File.AppendText(Path.Combine(curDir, fileSettingsName)))
                {
                    var json = JsonConvert.SerializeObject(settings);
                    sw.Write(json);
                }
            }

            workingThread = new Thread(FileQueueMonitoring);
            serviceStatusThread = new Thread(ServiceStatusMonitoring);

            client = QueueClient.Create(queueName);
            topicClient = TopicClient.Create(topicName);
            subscriptionClient = SubscriptionClient.Create(subscriptionTopicName, subscriptonName);

            timer = new System.Timers.Timer(10000);
            timer.Elapsed += FileMonitoring;
        }

        [LoggerPostSharpAspect]
        public bool Start(HostControl hostControl)
        {
            workingThread.Start();
            serviceStatusThread.Start();
            timer.Start();
            return true;
        }

        [LoggerPostSharpAspect]
        public bool Stop(HostControl hostControl)
        {
            stop = true;
            workingThread.Join();
            serviceStatusThread.Join();
            return true;
        }

        #endregion

        #region Private Methods

        private async void FileQueueMonitoring()
        {
            //listening to new messages in queue
            do
            {
                var brokeredMsg = await client.ReceiveAsync();
                if (brokeredMsg != null)
                {
                    var azureMsg = brokeredMsg.GetBody<AzureMessage>();

                    if (azureMsg.SequenceNumber == 0 || messageParts == null)
                    {
                        messageParts = new AzureMessage[azureMsg.Amount];
                    }

                    messageParts[azureMsg.SequenceNumber] = azureMsg;

                    brokeredMsg.Complete();

                    if (azureMsg.SequenceNumber == azureMsg.Amount - 1)
                    {
                        SaveDocumentToFolder();
                    }
                }

            } while (!stop);
        }

        private async void ServiceStatusMonitoring()
        {
            //listening to messages with service status and saving it to text file
            do
            {
                var brokeredMsg = await subscriptionClient.ReceiveAsync();
                var status = brokeredMsg.GetBody<ServiceStatus>();
                using (StreamWriter sw = File.AppendText(Path.Combine(curDir, statusFileName)))
                {
                    sw.WriteLine("Updated time:" + DateTime.Now.ToLongTimeString());
                    sw.WriteLine("Status: {0}; Timeout: {1}; Barcode value: {2}", status.Status, status.Timeout, status.Barcode);
                }

                brokeredMsg.Complete();

            } while (!stop);
        }

        [LoggerPostSharpAspect]
        private void SaveDocumentToFolder()
        {
            //constructing file from separated messages and saving it to pdf file
            byte[] documentBytes = messageParts[0].Body;

            for(int i = 1; i < messageParts.Length; i++)
            {
                documentBytes = documentBytes.Concat(messageParts[i].Body).ToArray();
            }

            File.WriteAllBytes(Path.Combine(outSuccessDir, Guid.NewGuid() + ".pdf"), documentBytes);
        }

        private void FileMonitoring(Object source, ElapsedEventArgs e)
        {
            //monitoring file with settings and sending updates to topic
            string fileSettingsStr = File.ReadAllText(Path.Combine(curDir, fileSettingsName));
            Settings fileSettingsObj = JsonConvert.DeserializeObject<Settings>(fileSettingsStr);

            if(settings.UpdateStatus != fileSettingsObj.UpdateStatus 
                || settings.Timeout != fileSettingsObj.Timeout 
                || settings.Barcode != fileSettingsObj.Barcode)
            {
                settings.UpdateStatus = fileSettingsObj.UpdateStatus;
                settings.Timeout = fileSettingsObj.Timeout;
                settings.Barcode = fileSettingsObj.Barcode;

                topicClient.Send(new BrokeredMessage(settings));
            }
        }

        #endregion
    }
}
