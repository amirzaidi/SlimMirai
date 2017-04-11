using System;
/*using System.Diagnostics;
using System.IO;
using System.Text;*/
using System.Threading;
using System.Threading.Tasks;

namespace Mirai
{
    class Logger
    {
        //private static SemaphoreSlim Waiter = new SemaphoreSlim(1);
        //private static FileStream Writer = File.OpenWrite($"log.{Process.GetCurrentProcess().Id}.txt");

        internal static async Task Log(object In)
        {
            var Text = $"[{DateTime.Now.ToString()}, {Thread.CurrentThread.ManagedThreadId}] {In}";
            Console.WriteLine(Text);

            /*var Buffer = Encoding.ASCII.GetBytes(Text + "\r\n");
            await Waiter.WaitAsync();
            await Writer.WriteAsync(Buffer, 0, Buffer.Length);
            Waiter.Release();*/
        }
    }
}
