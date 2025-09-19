using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace DiscordTools
{
    public static class AutoAddRoleHandler
    {
        private static DiscordSocketClient _client;
        private static string _botToken;
        private static ulong _guildId;
        private static ulong _roleId;

        public static void AutoAddRole()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t\t\t===============================================");
            Console.WriteLine("\t\t\t\t             Auto Add Role");
            Console.WriteLine("\t\t\t\t===============================================");

            Console.Write("\n\t\t\t\tEnter Bot Token: ");
            _botToken = Console.ReadLine();

            Console.Write("\n\t\t\t\tEnter Server ID: ");
            _guildId = ulong.Parse(Console.ReadLine());

            Console.Write("\n\t\t\t\tEnter Role ID to assign automatically: ");
            _roleId = ulong.Parse(Console.ReadLine());

            Console.Clear();
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers
            });

            
            _client.UserJoined += OnUserJoined;
            _client.Ready += OnReady;
            _client.Log += FilteredLog;

            
            await _client.LoginAsync(TokenType.Bot, _botToken);
            await _client.StartAsync();

            
            await Task.Delay(-1);
        }

        private static Task OnReady()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n\t\t\t\t[INFO] Bot is running... Press CTRL+C to stop.");
            Console.ResetColor();
            return Task.CompletedTask;
        }

        
        private static Task FilteredLog(LogMessage msg)
        {
            if (msg.Severity == LogSeverity.Warning || msg.Severity == LogSeverity.Error || msg.Severity == LogSeverity.Critical)
            {
                Console.ForegroundColor = msg.Severity == LogSeverity.Warning ? ConsoleColor.Yellow : ConsoleColor.Red;
                Console.WriteLine($"\n\t\t\t\t[{msg.Severity}] {msg.Message}");
                Console.ResetColor();
            }
            return Task.CompletedTask;
        }

        private static async Task OnUserJoined(SocketGuildUser user)
        {
            try
            {
                var guild = _client.GetGuild(_guildId);
                var role = guild.GetRole(_roleId);

                if (role != null)
                {
                    try
                    {
                        await user.AddRoleAsync(role);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n\t\t\t\t[INFO] User {user.Username}#{user.Discriminator} joined. Role '{role.Name}' assigned success.");
                    }
                    catch (Discord.Net.HttpException ex) when (ex.DiscordCode == DiscordErrorCode.MissingPermissions)
                    {
                        Console.ForegroundColor= ConsoleColor.Green;
                        Console.WriteLine($"\n\t\t\t\t[INFO] User {user.Username}#{user.Discriminator} joined. Role '{role.Name}' assigned success.");
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n\t\t\t\t[ERROR] Role not found in this server!");
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n\t\t\t\t[ERROR] Failed to assign role: {ex.Message}");
            }
        }

    }
}
