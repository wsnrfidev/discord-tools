using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DiscordTools
{
    public static class ServerMonitoringHandler
    {
        private static Dictionary<ulong, int> userMessageCount = new Dictionary<ulong, int>();
        private static Dictionary<ulong, int> channelMessageCount = new Dictionary<ulong, int>();

        public static async Task MonitorServer()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t\t\t===============================================");
            Console.WriteLine("\t\t\t\t               Server Monitoring");
            Console.WriteLine("\t\t\t\t===============================================");
            Console.WriteLine("\n\t\t\t\t 0. Return to main menu");

            Console.Write("\n\t\t\t\tEnter Bot Token (or 0 to return): ");
            string token = Console.ReadLine();

            if (token == "0")
            {
                Console.Clear();
                Console.WriteLine("\t\t\t\tReturning to main menu...");
                await Task.Delay(1000);
                return;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\t\t\t\t[ERROR] Invalid Bot Token. Please try again.");
                Console.ResetColor();
                Console.WriteLine("\n\t\t\t\tPress any key to return to the menu...");
                Console.ReadKey();
                return;
            }

            Console.Write("\n\t\t\t\tEnter Server ID: ");
            if (!ulong.TryParse(Console.ReadLine(), out ulong serverId))
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\t\t\t\t[ERROR] Invalid Server ID. Please try again.");
                Console.ResetColor();
                Console.WriteLine("\n\t\t\t\tPress any key to return to the menu...");
                Console.ReadKey();
                return;
            }

            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages
            });

            client.Log += LogAsync;
            client.MessageReceived += MessageReceived;

            try
            {
                Console.Clear();
                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();

                await Task.Delay(2000);

                if (client.ConnectionState != ConnectionState.Connected)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n\t\t\t\t[ERROR] Bot failed to connect. Invalid Bot Token.");
                    Console.ResetColor();
                    Console.WriteLine("\n\t\t\t\tPress any key to return to the menu...");
                    Console.ReadKey();
                    return;
                }

                var server = client.GetGuild(serverId);
                if (server != null)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\t\t\t\t===============================================");
                    Console.WriteLine($"\t\t\t\t   Server {server.Name} Monitoring");
                    Console.WriteLine("\t\t\t\t===============================================");
                    Console.ResetColor();

                    var members = server.Users;
                    int onlineCount = members.Count(u => u.Status != UserStatus.Offline);
                    var mostActiveMember = members.OrderByDescending(u => userMessageCount.ContainsKey(u.Id) ? userMessageCount[u.Id] : 0).FirstOrDefault();
                    var leastActiveMember = members.OrderBy(u => userMessageCount.ContainsKey(u.Id) ? userMessageCount[u.Id] : 0).FirstOrDefault();
                    var mostOnlineMember = members.OrderByDescending(u => u.Status != UserStatus.Offline).FirstOrDefault();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n\n\t\t\t\t===============================================");
                    Console.WriteLine($"\t\t\t\t                 Member Monitoring");
                    Console.WriteLine("\t\t\t\t===============================================");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n\t\t\t\tTotal Members Online: {onlineCount}");
                    Console.WriteLine($"\t\t\t\tMost Active Member: {(mostActiveMember?.Username ?? "Unknown")}");
                    Console.WriteLine($"\t\t\t\tLeast Active Member: {(leastActiveMember?.Username ?? "Unknown")}");
                    Console.WriteLine($"\t\t\t\tMost Online Member: {(mostOnlineMember?.Username ?? "Unknown")}");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n\t\t\t\t===============================================");
                    Console.WriteLine("\t\t\t\t          Top 5 Active Members:");
                    Console.WriteLine("\t\t\t\t===============================================");
                    Console.ResetColor();

                    var top5ActiveMembers = members
                        .OrderByDescending(u => userMessageCount.ContainsKey(u.Id) ? userMessageCount[u.Id] : 0)
                        .Take(5); 

                    foreach (var member in top5ActiveMembers)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"\n\t\t\t\t{member.Username}: {(userMessageCount.ContainsKey(member.Id) ? userMessageCount[member.Id] : 0)} messages");
                        Console.ResetColor();
                    }


                    var channels = server.TextChannels;
                    var mostUsedChannel = channels.OrderByDescending(c => channelMessageCount.ContainsKey(c.Id) ? channelMessageCount[c.Id] : 0).FirstOrDefault();
                    var importantChannels = channels.Where(c => !string.IsNullOrEmpty(c.Topic) && c.Topic.IndexOf("important", StringComparison.OrdinalIgnoreCase) >= 0).Take(5).ToList();  // Batasi ke 5 channel penting

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n\n\t\t\t\t===============================================");
                    Console.WriteLine($"\t\t\t\t                Channel Monitoring");
                    Console.WriteLine("\t\t\t\t===============================================");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n\t\t\t\tMost Used Channel: {(mostUsedChannel?.Name ?? "Unknown")}");
                    Console.WriteLine($"\t\t\t\tImportant Channels: {string.Join(", ", importantChannels.Select(c => c.Name))}");

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n\t\t\t\tMonitoring completed successfully!");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n\t\t\t\t[ERROR] Server not found or bot doesn't have the required permissions.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n\t\t\t\t[ERROR] {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\n\t\t\t\tPress any key to return to the menu...");
            Console.ReadKey();
        }

        static Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }

        public static Task MessageReceived(SocketMessage arg)
        {
            if (arg is SocketUserMessage message)
            {
                var user = message.Author;
                if (!user.IsBot)
                {

                    if (!userMessageCount.ContainsKey(user.Id))
                        userMessageCount[user.Id] = 0;
                    userMessageCount[user.Id]++;


                    if (!channelMessageCount.ContainsKey(message.Channel.Id))
                        channelMessageCount[message.Channel.Id] = 0;
                    channelMessageCount[message.Channel.Id]++;
                }
            }
            return Task.CompletedTask;
        }
    }
}
