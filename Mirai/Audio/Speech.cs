using Google.Cloud.Speech.V1Beta1;
using System;
using System.Threading.Tasks;

namespace Mirai.Audio
{
    class Speech
    {
        private static SpeechClient Client;

        internal static async Task Init()
        {
            Client = await SpeechClient.CreateAsync();
        }
    }
}
