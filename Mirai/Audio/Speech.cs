using Discord.Audio;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mirai.Audio
{
    class Speech
    {
        private static ConcurrentDictionary<ulong, CancellationTokenSource> Cancel = new ConcurrentDictionary<ulong, CancellationTokenSource>();

        internal static async Task RestartListenService(ulong s, AudioInStream In)
        {
            var x = SpeechRecognitionEngine.InstalledRecognizers();

            StopListenService(s);
            var Source = new CancellationTokenSource();
            Cancel.TryAdd(s, Source);

            var Queue = new Queue<RTPFrame>();
            var Timer = new Timer(e =>
            {
                if (!Source.IsCancellationRequested)
                {
                    var Array = Queue.ToArray();
                    Task.Run(() => ProcessVoiceAsync(s, Array));
                    Queue.Clear();
                }
            }, null, Timeout.Infinite, Timeout.Infinite);

            while (!Source.IsCancellationRequested)
            {
                try
                {
                    Queue.Enqueue(await In.ReadFrameAsync(Source.Token));
                    Timer.Change(150, 0);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception Ex)
                {
                    Logger.Log(Ex);
                }
            }
        }

        private static async Task ProcessVoiceAsync(ulong UserId, RTPFrame[] Frames)
        {
            var RecognizeWaiter = new TaskCompletionSource<RecognizeCompletedEventArgs>();
            EventHandler<RecognizeCompletedEventArgs> Event = (s, e) => RecognizeWaiter.SetResult(e);

            var Engine = await SpeechEnginePool.Get();
            Engine.RecognizeCompleted += Event;

            try
            {
                using (var Memory = new MemoryStream())
                {
                    for (int i = 0; i < Frames.Length; i++)
                    {
                        await Memory.WriteAsync(Frames[i].Payload, 0, Frames[i].Payload.Length);
                    }

                    Memory.Position = 0;

                    Engine.SetInputToAudioStream(Memory, new SpeechAudioFormatInfo(44100, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));
                    Engine.RecognizeAsync(RecognizeMode.Single);
                    var Args = await RecognizeWaiter.Task;

                    if (Args.Result?.Text != null)
                    {
                        Logger.Log($"{UserId} said {Args.Result.Text} {Args.Result.Confidence} confidence");

                        var Cmd = Args.Result.Text.Split(' ')[0];
                        var Remain = Args.Result.Text.Substring(Cmd.Length).Trim();
                        Command.GetVoice(Cmd, Ranks.Get(UserId))?.Invoke(Remain, UserId);
                    }
                }
            }
            catch (Exception Ex)
            {
                Logger.Log(Ex);
            }
            finally
            {
                Engine.RecognizeCompleted -= Event;
                SpeechEnginePool.Return(Engine);
            }
        }

        internal static async Task StopListenService(ulong s)
        {
            if (Cancel.TryRemove(s, out CancellationTokenSource Source))
            {
                Source.Cancel();
            }
        }
    }
}
