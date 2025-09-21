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
            
            userMessageCount.Clear();
            channelMessageCount.Clear();

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
                GatewayIntents =
                    GatewayIntents.Guilds |
                    GatewayIntents.GuildMembers |
                    GatewayIntents.GuildMessages |
                    GatewayIntents.GuildPresences |  
                    GatewayIntents.MessageContent     
            });

            client.Log += LogAsync;
            client.MessageReceived += MessageReceived;

            try
            {
                Console.Clear();
                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();

                
                var readyTcs = new TaskCompletionSource<bool>();
                Task readyHandler() { readyTcs.TrySetResult(true); return Task.CompletedTask; }
                client.Ready += () => readyHandler();

                
                var readyTask = readyTcs.Task;
                if (await Task.WhenAny(readyTask, Task.Delay(10000)) != readyTask)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n\t\t\t\t[ERROR] Bot failed to become ready (timeout). Check token / intents.");
                    Console.ResetColor();
                    Console.WriteLine("\n\t\t\t\tPress any key to return to the menu...");
                    Console.ReadKey();
                    await client.LogoutAsync();
                    await client.StopAsync();
                    return;
                }

                var server = client.GetGuild(serverId);
                if (server != null)
                {
                    
                    try
                    {
                        await server.DownloadUsersAsync();
                    }
                    catch
                    {
                        // if failed, still use cache
                    }

                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\t\t\t\t===============================================");
                    Console.WriteLine($"\t\t\t\t\t   Server {server.Name} Monitoring");
                    Console.WriteLine("\t\t\t\t===============================================");
                    Console.ResetColor();

                    
                    var afkChannelId = server.AFKChannel?.Id;
                    var textChannels = server.TextChannels.OrderBy(c => c.Position).ToList();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\n\t\t\t\tScanning channel message history (this may take a while)...\n");
                    Console.ResetColor();

                    foreach (var channel in textChannels)
                    {
                        
                        if (afkChannelId.HasValue && channel.Id == afkChannelId.Value)
                            continue;

                        
                        const int limitPerChannel = 200; 
                        try
                        {
                            Console.Write($"\t\t\t\t - Scanning #{channel.Name} ... ");
                            var messages = await channel.GetMessagesAsync(limitPerChannel).FlattenAsync();
                            int counted = 0;
                            foreach (var msg in messages)
                            {
                                if (msg.Author == null) continue;
                                if (msg.Author.IsBot) continue; 

                                
                                if (!userMessageCount.ContainsKey(msg.Author.Id))
                                    userMessageCount[msg.Author.Id] = 0;
                                userMessageCount[msg.Author.Id]++;

                                
                                if (!channelMessageCount.ContainsKey(channel.Id))
                                    channelMessageCount[channel.Id] = 0;
                                channelMessageCount[channel.Id]++;

                                counted++;
                            }
                            Console.WriteLine($"done (counted {counted})");
                        }
                        catch (Exception ex)
                        {
                           
                            Console.WriteLine($"skipped ({ex.Message})");
                        }
                    }

                  
                    var members = server.Users;
                    int onlineCount = members.Count(u => u.Status == UserStatus.Online || u.Status == UserStatus.Idle || u.Status == UserStatus.DoNotDisturb);

                   
                    bool hasMessageData = userMessageCount.Count > 0;

                    ulong mostActiveId = 0;
                    ulong leastActiveId = 0;
                    ulong mostOnlineId = 0;

                    if (hasMessageData)
                    {
                        mostActiveId = userMessageCount.OrderByDescending(kv => kv.Value).First().Key;
                        leastActiveId = userMessageCount.OrderBy(kv => kv.Value).First().Key;
                    }

                    var onlineMembers = members.Where(u => u.Status == UserStatus.Online || u.Status == UserStatus.Idle || u.Status == UserStatus.DoNotDisturb).ToList();
                    var mostOnlineMember = onlineMembers.FirstOrDefault(); 

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n\n\t\t\t\t===============================================");
                    Console.WriteLine("\t\t\t\t                 Member Monitoring");
                    Console.WriteLine("\t\t\t\t===============================================");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n\t\t\t\tTotal Members Online: {onlineCount}");
                    if (hasMessageData)
                    {
                        var mostActiveUser = server.GetUser(mostActiveId);
                        var leastActiveUser = server.GetUser(leastActiveId);
                        Console.WriteLine($"\t\t\t\tMost Active Member: {(mostActiveUser != null ? $"{mostActiveUser.Username}#{mostActiveUser.Discriminator}" : mostActiveId.ToString())} ({userMessageCount[mostActiveId]} messages)");
                        Console.WriteLine($"\t\t\t\tLeast Active Member: {(leastActiveUser != null ? $"{leastActiveUser.Username}#{leastActiveUser.Discriminator}" : leastActiveId.ToString())} ({userMessageCount[leastActiveId]} messages)");
                    }
                    else
                    {
                        Console.WriteLine($"\t\t\t\tMost Active Member: None (no message data scanned)");
                        Console.WriteLine($"\t\t\t\tLeast Active Member: None (no message data scanned)");
                    }

                    Console.WriteLine($"\t\t\t\tMost Online Member: {(mostOnlineMember != null ? $"{mostOnlineMember.Username}#{mostOnlineMember.Discriminator}" : "None")}");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n\t\t\t\t===============================================");
                    Console.WriteLine("\t\t\t\t          Top 5 Active Members:");
                    Console.WriteLine("\t\t\t\t===============================================");
                    Console.ResetColor();

                    if (hasMessageData)
                    {
                        var top5 = userMessageCount.OrderByDescending(kv => kv.Value).Take(5);
                        foreach (var kv in top5)
                        {
                            var user = server.GetUser(kv.Key);
                            var display = user != null ? $"{user.Username}#{user.Discriminator}" : kv.Key.ToString();
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"\n\t\t\t\t{display}: {kv.Value} messages");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("\n\t\t\t\tNo messages found in scanned channels.");
                        Console.ResetColor();
                    }

                    var mostUsedChannel = channelMessageCount.Count > 0
                        ? server.GetChannel(channelMessageCount.OrderByDescending(kv => kv.Value).First().Key) as SocketTextChannel
                        : null;

                    var keywords = new[] { "important", "announce", "announcement", "info", "rules", "pengumuman", "notice", "news", "staff", "admin" };
                    var importantChannels = server.TextChannels
                        .Where(c =>
                            (!string.IsNullOrEmpty(c.Topic) && keywords.Any(k => c.Topic.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0))
                            || keywords.Any(k => c.Name.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0))
                        .Take(5)
                        .Select(c => c.Name)
                        .ToList();

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n\n\t\t\t\t===============================================");
                    Console.WriteLine("\t\t\t\t                Channel Monitoring");
                    Console.WriteLine("\t\t\t\t===============================================");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n\t\t\t\tMost Used Channel: {(mostUsedChannel != null ? $"#{mostUsedChannel.Name} ({channelMessageCount[mostUsedChannel.Id]} msgs)" : "None / no message data")}");
                    Console.WriteLine($"\t\t\t\tImportant Channels: {(importantChannels.Count > 0 ? string.Join(", ", importantChannels) : "None")}");
                    Console.ResetColor();

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
