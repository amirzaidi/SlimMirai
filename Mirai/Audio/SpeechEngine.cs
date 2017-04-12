using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mirai.Audio
{
    class SpeechEngine : IDisposable
    {
        private static ConcurrentQueue<SpeechEngine> Engines = new ConcurrentQueue<SpeechEngine>();
        internal static CultureInfo Culture = new CultureInfo("en-US");
        internal static GrammarBuilder GrammarBuilder;
        private static long State = long.MinValue;

        internal static async Task<SpeechEngine> Get(EventHandler<RecognizeCompletedEventArgs> RecognizeCompleted)
        {
            if (!Engines.TryDequeue(out SpeechEngine Engine))
            {
                Engine = new SpeechEngine()
                {
                    OwnState = State,
                    Service = new SpeechRecognitionEngine(Culture)
                };

                await Engine.LoadGrammar();
            }
            
            return Engine.Prepare(RecognizeCompleted);
        }

        internal static void Invalidate()
        {
            Interlocked.Increment(ref State);
            while (Engines.TryDequeue(out SpeechEngine Engine) && !Engine.IsValid)
            {
                Engine.Dispose();
            }
        }

        private SpeechRecognitionEngine Service;
        private long OwnState;
        private EventHandler<RecognizeCompletedEventArgs> RecognizeCompleted;

        private async Task LoadGrammar()
        {
            var GrammarWaiter = new TaskCompletionSource<LoadGrammarCompletedEventArgs>();
            Service.LoadGrammarCompleted += (s, e) => GrammarWaiter.SetResult(e);
            Service.LoadGrammarAsync(new Grammar(GrammarBuilder));
            await GrammarWaiter.Task;
        }

        private SpeechEngine Prepare(EventHandler<RecognizeCompletedEventArgs> RecognizeCompleted)
        {
            this.RecognizeCompleted = RecognizeCompleted;
            Service.RecognizeCompleted += RecognizeCompleted;

            return this;
        }

        internal void Recognize(MemoryStream Stream)
        {
            Service.SetInputToAudioStream(Stream, new SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));
            Service.RecognizeAsync(RecognizeMode.Single);
        }

        internal bool IsValid
        {
            get
            {
                return OwnState == State;
            }
        }

        public void Dispose()
        {
            if (IsValid)
            {
                Service.RecognizeCompleted -= RecognizeCompleted;
                Engines.Enqueue(this);
            }
            else
            {
                Service.Dispose();
            }
        }
    }
}
