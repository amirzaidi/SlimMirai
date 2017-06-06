using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Search
{
    static class Extensions
    {
        internal static string ToHMS(this TimeSpan Span)
        {
            var TimeStr = $"{Span.Minutes.ToString().PadLeft(2, '0')}:{Span.Seconds.ToString().PadLeft(2, '0')}";
            var Hours = Span.Days * 24 + Span.Hours;
            if (Hours != 0)
            {
                TimeStr = $"{Hours}:{TimeStr}";
            }

            return TimeStr;
        }

        internal static StringBuilder UcWords(this object In)
        {
            var Output = new StringBuilder();
            var Pieces = In.ToString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var piece in Pieces)
            {
                var Chars = piece.ToCharArray();
                Chars[0] = char.ToUpper(Chars[0]);
                Output.Append(' ');
                Output.Append(Chars);
            }

            return Output;
        }

        internal static async Task<string> WebResponseRetryLoop(this string Url, WebHeaderCollection Headers = null)
        {
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        var Request = WebRequest.Create(Url);
                        if (Headers != null)
                        {
                            Request.Headers = Headers;
                        }

                        return await new StreamReader(
                                (await Request.GetResponseAsync())
                                .GetResponseStream()
                            )
                            .ReadToEndAsync();
                    }
                    catch (Exception Ex2)
                    {
                        if (i == 4)
                        {
                            throw Ex2;
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex);
            }

            return string.Empty;
        }
    }
}
