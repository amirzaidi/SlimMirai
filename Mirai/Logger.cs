using System;
using System.Threading;

namespace Mirai
{
    class Logger
    {
        internal static void Log(object In)
        {
            Console.WriteLine($"[{DateTime.Now.ToString()}, {Thread.CurrentThread.ManagedThreadId}] {In}");
        }

        internal static void SetTitle(string Title)
        {
            Console.Title = $"Slim Mirai | {Title}";
        }
    }
}
