using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DiscordTools; 

namespace DiscordTools
{
    public static class GetMembersInfoHandler
    {
        public static async Task GetMembersInfo()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t\t\t===============================================");
            Console.WriteLine("\t\t\t\t             Get All Members Info");
            Console.WriteLine("\t\t\t\t===============================================");

            Console.Write("\n\t\t\t\tEnter Bot Token (or 0 to return): ");
            string token = Console.ReadLine();
            if (token == "0") return;

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\t\t\t\t[ERROR] Invalid Bot Token.");
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
                Console.WriteLine("\n\t\t\t\t[ERROR] Invalid Server ID.");
                Console.ResetColor();
                Console.WriteLine("\n\t\t\t\tPress any key to return to the menu...");
                Console.ReadKey();
                return;
            }

            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildPresences
            });

            client.Log += (LogMessage msg) => { Console.WriteLine(msg); return Task.CompletedTask; };

            try
            {
                Console.Clear();
                await client.LoginAsync(TokenType.Bot, token);
                await client.StartAsync();


                var readyTcs = new TaskCompletionSource<bool>();
                client.Ready += () =>
                {
                    readyTcs.TrySetResult(true);
                    return Task.CompletedTask;
                };
                await readyTcs.Task;

                var guild = client.GetGuild(serverId);
                if (guild == null)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n\t\t\t\t[ERROR] Server not found or bot not in that server.");
                    Console.ResetColor();
                    Console.WriteLine("\n\t\t\t\tPress any key to return to the menu...");
                    Console.ReadKey();
                    return;
                }

                
                try
                {
                    await guild.DownloadUsersAsync();
                }
                catch
                {
                    // if failed, still use cache
                }

                var members = guild.Users.OrderBy(u => u.Username).ToList();
                int total = members.Count;

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\t\t\t\t===============================================");
                Console.WriteLine("\t\t\t\t             Get All Members Info");
                Console.WriteLine("\t\t\t\t===============================================");

                Console.WriteLine("\n\t\t\t\t================================================");
                Console.WriteLine($"\t\t\t\t\t\tTotal Members: {total}");
                Console.WriteLine("\t\t\t\t================================================");
                Console.ResetColor();

                var listForJson = new List<MemberInfoData>();

                foreach (var user in members)
                {
                    string username = user.Username;
                    string discriminator = user.Discriminator;
                    string displayName = (user as SocketGuildUser)?.Nickname ?? username;
                    bool isBot = user.IsBot;
                    string avatarUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl();
                    string status = user.Status.ToString();
                    var roleNames = (user as SocketGuildUser)?.Roles
                        .Where(r => !r.Name.Equals("@everyone"))
                        .Select(r => r.Name)
                        .ToList() ?? new List<string>();

                    DateTimeOffset? joinedAt = (user as SocketGuildUser)?.JoinedAt;

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("---------------------------------------------------------------------------------");
                    Console.WriteLine($"Username      : {username}#{discriminator}");
                    Console.WriteLine($"ID            : {user.Id}");
                    Console.WriteLine($"Display Name  : {displayName}");
                    Console.WriteLine($"Bot           : {isBot}");
                    Console.WriteLine($"Status        : {status}");
                    Console.WriteLine($"Joined At     : {joinedAt?.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown"}");
                    Console.WriteLine($"Roles         : {(roleNames.Count > 0 ? string.Join(", ", roleNames) : "None")}");
                    Console.WriteLine($"Avatar URL    : {avatarUrl}");

                  
                    listForJson.Add(new MemberInfoData
                    {
                        UserId = user.Id,
                        Username = username,
                        Discriminator = discriminator,
                        DisplayName = displayName,
                        IsBot = isBot,
                        Status = status,
                        JoinedAt = joinedAt,
                        Roles = roleNames,
                        AvatarUrl = avatarUrl
                    });
                }
                Console.WriteLine("\n\t\t\t\t===============================================");
                Console.WriteLine("\t\t\t\t\tDone scanning members.");
                Console.WriteLine("\t\t\t\t===============================================");
                Console.ResetColor();

                Console.WriteLine("\n\t\t\t\t===============================================");
                Console.Write("\t\t\t\tDo you want to save data to JSON file? (Y/N): ");
                Console.WriteLine("\n\t\t\t\t===============================================");

                Console.Write("\n\t\t\t\tYour choice: ");
                string saveChoice = Console.ReadLine().ToUpper();
                if (saveChoice == "Y")
                {
                    string outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");
                    if (!Directory.Exists(outputDir))
                        Directory.CreateDirectory(outputDir);

                    string filename = Path.Combine(outputDir, $"members_{guild.Id}.json");

                    string json = JsonConvert.SerializeObject(listForJson, Formatting.Indented);
                    File.WriteAllText(filename, json);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n\t\t\tMembers data saved to {filename}");
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
    }
}
