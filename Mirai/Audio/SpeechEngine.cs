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
        internal static int Count
        {
            get
            {
                return Engines.Count;
            }
        }

        internal static CultureInfo Culture = new CultureInfo("en-US");
        internal static string[] Trigger;
        private static long State = long.MinValue;

        internal static Task<SpeechEngine> Get(EventHandler<RecognizeCompletedEventArgs> RecognizeCompleted)
        {
            if (!Engines.TryDequeue(out SpeechEngine Engine))
            {
                Engine = new SpeechEngine
                {
                    Service = new SpeechRecognitionEngine(Culture)
                };
            }
            
            return Engine.Prepare(RecognizeCompleted);
        }

        internal static void Invalidate()
        {
            Interlocked.Increment(ref State);
        }

        private SpeechRecognitionEngine Service;
        private long OwnState;
        private EventHandler<RecognizeCompletedEventArgs> RecognizeCompleted;

        private async Task<SpeechEngine> Prepare(EventHandler<RecognizeCompletedEventArgs> RecognizeCompleted)
        {
            this.RecognizeCompleted = RecognizeCompleted;
            Service.RecognizeCompleted += RecognizeCompleted;
            
            if (!IsValid)
            {
                Service.UnloadAllGrammars();

                var Main = new GrammarBuilder(string.Join(" ", Trigger));
                Main.Append(Command.GetChoices());

                var Waiter = new TaskCompletionSource<LoadGrammarCompletedEventArgs>();
                EventHandler<LoadGrammarCompletedEventArgs> Event = (s, e) => Waiter.SetResult(e);
                Service.LoadGrammarCompleted += Event;
                Service.LoadGrammarAsync(new Grammar(Main));
                await Waiter.Task;
                Service.LoadGrammarCompleted -= Event;

                OwnState = State;
            }

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
            Service.RecognizeCompleted -= RecognizeCompleted;
            Engines.Enqueue(this);
        }
    }
}
