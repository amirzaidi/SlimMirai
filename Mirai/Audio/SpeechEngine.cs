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
        internal readonly static string[] Trigger = new[] { "music", "bot" };
        internal static Choices Commands;
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
                if (Service.Grammars.Count != 0)
                {
                    Service.UnloadAllGrammars();
                }

                var Main = new GrammarBuilder(string.Join(" ", Trigger));
                if (Commands != null)
                {
                    Main.Append(Commands);
                }

                var Waiter = new TaskCompletionSource<LoadGrammarCompletedEventArgs>();
                Service.LoadGrammarCompleted += (s, e) => Waiter.SetResult(e);
                Service.LoadGrammarAsync(new Grammar(Main));
                await Waiter.Task;

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
