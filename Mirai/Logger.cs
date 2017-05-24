using System;
using System.Threading;

namespace Mirai
{
    class Logger
    {
        internal static void Log(object In)
        {
//#if DEBUG
            var Text = $"[{DateTime.Now.ToString()}, {Thread.CurrentThread.ManagedThreadId}] {In}";
            Console.WriteLine(Text);
//#endif
        }
    }
}
