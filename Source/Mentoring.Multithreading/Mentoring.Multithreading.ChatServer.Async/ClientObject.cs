using System;
using System.Net.Sockets;
using System.Text;

namespace Mentoring.Multithreading.ChatServer.Async
{
    public class ClientObject
    {
        #region Fields

        public string Id { get; private set; }
        public NetworkStream Stream { get; private set; }
        private string userName;
        private TcpClient client;
        private ServerObject server;
        private object lockObj = new object();

        #endregion

        #region Constructors
        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        #endregion

        #region Public Methods
        public void Process()
        {
            try
            {
                Stream = client.GetStream();

                string message = GetMessage();
                userName = message;

                message = userName + " joined the chat \n";

                server.BroadcastMessage(message, Id);
                Console.WriteLine(message);

                server.BroadcastMessageHistory(this);

                StoreMessageToHistory(message);

                while (true)
                {
                    try
                    {
                        if (CheckIfClientConnected())
                        {
                            message = GetMessage();
                            if (!string.IsNullOrEmpty(message))
                            {
                                message = String.Format("{0}: {1}", userName, message);
                                Console.WriteLine(message);
                                StoreMessageToHistory(message);
                                server.BroadcastMessage(message, Id);
                            }
                        }
                        else
                        {
                            LeaveTheChatEvent();
                            break;
                        }
                    }
                    catch
                    {
                        LeaveTheChatEvent();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(Id);
                Close();
            }
        }

        public void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }

        #endregion

        #region Private Methods

        private void StoreMessageToHistory(string message)
        {
            lock (lockObj)
            {
                server.MessagesHistory.Enqueue(message);
            }
        }

        private void LeaveTheChatEvent()
        {
            var message = String.Format("{0} leave the chat \n", userName);
            Console.WriteLine(message);
            StoreMessageToHistory(message);
            server.BroadcastMessage(message, this.Id);
        }

        private bool CheckIfClientConnected()
        {
            if (client.Client.Poll(0, SelectMode.SelectRead))
            {
                byte[] buff = new byte[1];
                if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private string GetMessage()
        {
            byte[] data = new byte[64];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        #endregion
    }
}
