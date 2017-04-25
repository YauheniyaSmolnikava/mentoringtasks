﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace Mentoring.Multithreading.Chat
{
    public class ServerObject
    {
        #region Fields

        private static TcpListener tcpListener;
        private List<ClientObject> clients = new List<ClientObject>();
        public FixedSizedQueue<string> MessagesHistory = new FixedSizedQueue<string>(3);
        private object lockObj = new object();

        #endregion

        #region Connections Methods

        protected internal void AddConnection(ClientObject clientObject)
        {
            lock (lockObj)
            {
                clients.Add(clientObject);
            }
        }
        protected internal void RemoveConnection(string id)
        {
            lock (lockObj)
            {
                ClientObject client = clients.FirstOrDefault(c => c.Id == id);

                if (client != null)
                    clients.Remove(client);
            }
        }

        #endregion

        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8888);
                tcpListener.Start();
                Console.WriteLine("Server has started. Waiting for new connections...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        #region Broadcasting Methods

        protected internal void BroadcastMessageHistory(ClientObject client)
        {
            if (MessagesHistory.Queue.Count > 0)
            {
                WriteMessageToStream(client, "\n ***** CHAT HISTORY ***** \n");

                foreach (var message in MessagesHistory.Queue)
                {
                    WriteMessageToStream(client, message);
                }

                WriteMessageToStream(client, " ***** \n");
            }
        }

        protected internal void BroadcastMessage(string message, string id)
        {
            //Sending message to all clients except sender
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != id)
                {
                    WriteMessageToStream(clients[i], message);
                }
            }
        }

        private void WriteMessageToStream(ClientObject client, string message)
        {
            var data = Encoding.Unicode.GetBytes(message);
            client.Stream.Write(data, 0, data.Length);
        }

        #endregion

        protected internal void Disconnect()
        {
            tcpListener.Stop();

            lock (lockObj)
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    WriteMessageToStream(clients[i], "Server message: Server is shutting down");
                    clients[i].Close();
                    RemoveConnection(clients[i].Id);
                }
            }

           // Environment.Exit(0);
        }
    }
}
