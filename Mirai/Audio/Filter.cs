using System.Globalization;

namespace Mirai.Audio
{
    class Filter
    {
        internal static double Volume = 1;
        internal static double Tone = 1;
        internal static int Packets = 0;

        internal static string Tag
        {
            get
            {
                var Vol = Volume.ToString(CultureInfo.InvariantCulture);
                var Rate = (44.1 * Tone).ToString(CultureInfo.InvariantCulture);
                var Tempo = (1 / Tone).ToString(CultureInfo.InvariantCulture);

                return $"volume={Vol},asetrate=r={Rate}K,atempo={Tempo},afade=t=in:d=0.4:curve=squ";
            }
        }
    }
}
