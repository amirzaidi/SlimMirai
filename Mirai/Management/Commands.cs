using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mirai.Management
{
    class Commands
    {
        internal static async Task Shutdown(ulong User, Queue<string> Args)
        {
            Environment.Exit(0);
        }
    }
}
