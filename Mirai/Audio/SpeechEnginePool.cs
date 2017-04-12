using Microsoft.Speech.Recognition;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks;

namespace Mirai.Audio
{
    class SpeechEnginePool
    {
        private static ConcurrentStack<SpeechRecognitionEngine> Engines = new ConcurrentStack<SpeechRecognitionEngine>();
        internal static CultureInfo Culture = new CultureInfo("en-US");
        internal static GrammarBuilder GrammarBuilder;

        internal static async Task<SpeechRecognitionEngine> Get()
        {
            if (!Engines.TryPop(out SpeechRecognitionEngine Engine))
            {
                Engine = new SpeechRecognitionEngine(Culture);

                var GrammarWaiter = new TaskCompletionSource<LoadGrammarCompletedEventArgs>();
                Engine.LoadGrammarCompleted += (s, e) => GrammarWaiter.SetResult(e);
                Engine.LoadGrammarAsync(new Grammar(GrammarBuilder));
                await GrammarWaiter.Task;
            }

            return Engine;
        }

        internal static void Return(SpeechRecognitionEngine Engine)
            => Engines.Push(Engine);
    }
}
