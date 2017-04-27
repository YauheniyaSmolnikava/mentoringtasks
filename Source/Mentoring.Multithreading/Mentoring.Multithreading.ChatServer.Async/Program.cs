using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Timers;

namespace Mentoring.Multithreading.ChatServer.Async
{
    class Program
    {
        static ServerObject server;
        static Thread listenThread;

        static void Main(string[] args)
        {
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            try
            {
                server = new ServerObject();
                listenThread = new Thread(server.Listen);
                listenThread.Start();
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }

        private static void HandleTimer(Object source, ElapsedEventArgs e)
        {
            server.Disconnect();
        }

        static bool ConsoleEventCallback(int eventType)
        {
            server.Disconnect();
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                               // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
    }
}

