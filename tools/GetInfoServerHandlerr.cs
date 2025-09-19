using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordTools
{
    public static class GetInfoServerHandler
    {
        public static async Task GetServerInfo()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t\t\t===============================================");
            Console.WriteLine("\t\t\t\t               Get Server Info");
            Console.WriteLine("\t\t\t\t===============================================");
            Console.WriteLine("\n\t\t\t\t 0. Return to main menu");

            Console.Write("\n\t\t\t\tEnter Bot Token (or 0 to return): ");
            string token = Console.ReadLine();

            if (token == "0")
            {
                Console.Clear();
                Console.WriteLine("\t\t\t\tReturning to main menu...");
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
            string serverIdInput = Console.ReadLine();

            if (!ulong.TryParse(serverIdInput, out ulong serverId))
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

            try
            {
                Console.Clear();
                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();

                // wait till guild registered
                var readyEvent = new TaskCompletionSource<bool>();
                client.Ready += () =>
                {
                    readyEvent.TrySetResult(true);
                    return Task.CompletedTask;
                };
                await readyEvent.Task;

                var server = client.GetGuild(serverId);
                if (server != null)
                {
                    
                    var allUsers = await server.GetUsersAsync().FlattenAsync();
                    int botCount = allUsers.Count(u => u.IsBot);

                    
                    string ownerName;
                    try
                    {
                        
                        var restOwner = await client.Rest.GetGuildUserAsync(serverId, server.OwnerId);
                        if (restOwner != null)
                            ownerName = $"{restOwner.Username}#{restOwner.Discriminator}";
                        else
                            ownerName = $"Unknown (ID: {server.OwnerId})";
                    }
                    catch (Exception)
                    {
                        
                        ownerName = $"Unknown (ID: {server.OwnerId})";
                    }

                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\t\t\t\t===============================================");
                    Console.WriteLine($"\t\t\t\t        Server {server.Name} Info");
                    Console.WriteLine("\t\t\t\t===============================================");
                    Console.ResetColor();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n\t\t\t\tServer Name: {server.Name}");
                    Console.WriteLine($"\t\t\t\tTotal Members: {server.MemberCount}");
                    Console.WriteLine($"\t\t\t\tTotal Bots: {botCount}");
                    Console.WriteLine($"\t\t\t\tTotal Roles: {server.Roles.Count()}");
                    Console.WriteLine($"\t\t\t\tTotal Channels: {server.Channels.Count}");
                    Console.WriteLine($"\t\t\t\tServer Region: {server.VoiceRegionId}");
                    Console.WriteLine($"\t\t\t\tServer Owner: {ownerName}");
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
    }
}
