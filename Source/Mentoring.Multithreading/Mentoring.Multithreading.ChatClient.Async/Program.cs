using System;
using System.Threading;


namespace Mentoring.Multithreading.ChatClient.Async
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = "127.0.0.1";
            int port = 8888;

            var client1 = new ClientBot(host, port);

            Thread thread1 = new Thread(new ThreadStart(client1.ProcessChatting));
            thread1.Start();

            Console.ReadLine();
        }
    }
}
