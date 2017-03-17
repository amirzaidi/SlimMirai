using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Mirai
{
    class Events
    {
        internal static async Task Connected()
        {
            Console.WriteLine("Connected!");
            Command.Load(Account.Client.CurrentUser.Mention.Replace("!", ""));
        }

        internal static async Task Log(LogMessage e)
        {
            if (e.Severity != LogSeverity.Info)
            {
                var Text = e.Severity.ToString() + " ";
                if (e.Message != null)
                {
                    Text += "Message " + e.Message;
                }

                if (e.Message != "Failed to send message")
                {
                    if (e.Exception != null)
                    {
                        Text += "\nException " + e.Exception;
                    }

                    if (e.Source != null)
                    {
                        Text += "\nFrom " + e.Source;
                    }
                }

                Console.WriteLine(Text);
            }
        }

        internal static async Task MessageReceived(SocketMessage e)
        {
            if ((!e?.Author.IsBot ?? false) && e.Channel.Id == Program.TextChannel && !string.IsNullOrWhiteSpace(e.Content))
            {
                var Split = e.Content.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (Split.Length != 0)
                {
                    var Remaining = e.Content.Substring(Split[0].Length).Trim();
                    Command.Get(Split[0], Ranks.Get(e.Author.Id))?.Invoke(Remaining, e);
                }
            }
        }

        internal static async Task Disconnected(Exception e)
        {
            Console.WriteLine("Disconnected - Attempting reconnect");

            try
            {
                await Account.Logout();
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex);
            }

            try
            {
                await Account.Login();
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex);
            }
        }
    }
}
