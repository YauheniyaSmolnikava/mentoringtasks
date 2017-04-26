using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;

namespace Mentoring.Multithreading.ChatClient
{
    public class ClientSample
    {
        #region Fields

        Random random = new Random();

        private readonly string host;
        private readonly int port;

        private TcpClient client;
        private NetworkStream stream;

        private bool serverDisconnected = false;

        #endregion

        #region Constructors

        public ClientSample(string hostVal, int portVal)
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

                    var clientName = Helper.ClientNames[random.Next(0, 9)];
                    string message = clientName;

                    //Sending message with client name
                    SendMessage(message);

                    //Receiving messages from Server in separate thread
                    Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                    receiveThread.Start();

                    Console.WriteLine("Welcome, {0}", clientName);

                    //How many messages send from this client
                    var msgToSendCount = random.Next(2, 7);

                    for (int i = 0; i < msgToSendCount; i++)
                    {

                        Thread.Sleep(random.Next(2000, 3000));
                        var messageToSend = Helper.Messages[random.Next(0, 9)];

                        //check if in ReceiveMessage method message about server disconnecting was obtained
                        if (serverDisconnected)
                        {
                            break;
                        }
                        SendMessage(messageToSend + "\n");
                        Console.WriteLine(clientName + ": " + messageToSend);
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
        private void SendMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        private void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();

                    if (!string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine(message);
                    }

                    if (message.Contains("Server message"))
                    {
                        serverDisconnected = true;
                        Disconnect();
                        break;
                    }
                }
                catch
                {
                    Console.WriteLine("Disconnected");
                    Console.ReadLine();
                    Disconnect();
                    break;
                }
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
