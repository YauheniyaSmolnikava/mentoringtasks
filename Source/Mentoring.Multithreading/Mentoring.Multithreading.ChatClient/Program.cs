using System.Threading;
using System;

namespace Mentoring.Multithreading.ChatClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = "127.0.0.1";
            int port = 8888;

            var client1 = new ClientSample(host, port);

            Thread thread1 = new Thread(new ThreadStart(client1.ProcessChatting));
            thread1.Start();

            Console.ReadLine();

            //var client2 = new ClientSample(host, port);
            //client2.ProcessChatting();

            //Thread thread2 = new Thread(new ThreadStart(client2.ProcessChatting));
            //thread2.Start();
        }
    }
}
