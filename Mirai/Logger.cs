using System;
using System.Threading;

namespace Mirai
{
    class Logger
    {
        internal static void Log(object In)
        {
            var Text = $"[{DateTime.Now.ToString()}, {Thread.CurrentThread.ManagedThreadId}] {In}";
            Console.WriteLine(Text);
        }

        internal static void SetTitle(string Title)
        {
            Console.Title = $"Slim Mirai | {Title}";
        }
    }
}
