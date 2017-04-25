using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Timers;

namespace Mentoring.Multithreading.Chat
{
    class Program
    {
        static ServerObject server; 
        static Thread listenThread;

        static void Main(string[] args)
        {
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);

            System.Timers.Timer timer = new System.Timers.Timer(15000);
            timer.Elapsed += HandleTimer;
            timer.Start();

            try
            {
                server = new ServerObject();
                listenThread = new Thread(new ThreadStart(server.Listen));
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
