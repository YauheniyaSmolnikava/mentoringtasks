using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using Mentoring.Multithreading.Utils;
using System.IO;

namespace Mentoring.Multithreading.ChatClient.Async
{
    public class ClientBot
    {
        #region Fields

        Random random = new Random();

        private readonly string host;
        private readonly int port;

        private TcpClient client;
        private NetworkStream stream;
        private string clientName;

        private bool serverDisconnected = false;
        private string fileReceivedMessageName = "ClientIncomeMessages.txt";

        #endregion

        #region Constructors

        public ClientBot(string hostVal, int portVal)
        {
            host = hostVal;
            port = portVal;
        }

        #endregion

        #region Public Methods
        public void ProcessChatting()
        {
            while (!serverDisconnected)
            {
                try
                {
                    //New Tcp Client creation and gettig stream
                    client = new TcpClient();
                    client.Connect(host, port);

                    stream = client.GetStream();

                    clientName = SamplesHelper.ClientNames[random.Next(0, 9)];
                    string message = clientName;

                    //Sending the first message with client name
                    BeginSendMessage(message);

                    //Receiving messages from Server in separate thread
                    BeginRead();

                    Console.WriteLine("Welcome, {0}\n", clientName);

                    //How many messages send from this client
                    var msgToSendCount = random.Next(2, 7);

                    for (int i = 0; i < msgToSendCount; i++)
                    {

                        Thread.Sleep(random.Next(2000, 3000));
                        var messageToSend = SamplesHelper.Messages[random.Next(0, 9)];

                        //check if in ReceiveMessage method message about server disconnecting was obtained
                        if (serverDisconnected)
                        {
                            break;
                        }
                        BeginSendMessage(messageToSend + "\n");
                        Console.WriteLine(clientName + ": " + messageToSend + "\n");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
                finally
                {
                    Disconnect();
                }
            }
        }

        #endregion

        #region Private Methods
        public void BeginSendMessage(string message)
        {
            var bytes = Encoding.Unicode.GetBytes(message);
            stream.BeginWrite(bytes, 0, bytes.Length, EndSend, bytes);
        }

        public void EndSend(IAsyncResult result)
        {
            var bytes = (byte[])result.AsyncState;
        }

        public void BeginRead()
        {
            var buffer = new byte[4096];
            stream.BeginRead(buffer, 0, buffer.Length, EndRead, buffer);
        }

        public void EndRead(IAsyncResult result)
        {
            try
            {
                var buffer = (byte[])result.AsyncState;
                var bytesAvailable = stream.EndRead(result);

                var message = Encoding.Unicode.GetString(buffer, 0, bytesAvailable);

                if (!string.IsNullOrEmpty(message))
                {
                    Console.WriteLine(message);

                    using (StreamWriter sw = File.AppendText(@".//" + fileReceivedMessageName))
                    {
                        sw.WriteLine("Received from Server: {0} : {1}", clientName, message);
                    }
                }

                if (message.Contains("Server message"))
                {
                    serverDisconnected = true;
                    Disconnect();
                }

                if (!serverDisconnected)
                {
                    BeginRead();
                }
            }
            catch
            {
                Console.WriteLine("Disconnected\n");
                Console.ReadLine();
                Disconnect();
            }
        }

        private void Disconnect()
        {
            if (stream != null)
                stream.Close();

            if (client != null)
                client.Close();
        }

        #endregion
    }
}
