using System.Collections.Generic;

namespace Mirai
{
    class Ranks
    {
        static Dictionary<ulong, int> Predefined = new Dictionary<ulong, int>
        {
            { 74779725393825792, 3 },
            { 109007493279014912, 2 }
        };

        internal static int Get(ulong Id)
        {
            if (Predefined.ContainsKey(Id))
            {
                return Predefined[Id];
            }

            return 1;
        }
    }
}
