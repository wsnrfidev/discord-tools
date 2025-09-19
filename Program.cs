using System;
using System.Threading.Tasks;

namespace DiscordTools
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.Title = "DISCORD TOOLS";
                ConsoleUI.ShowBanner();
                ConsoleUI.ShowMenu();

                ConsoleKeyInfo input = Console.ReadKey();
                char option = input.KeyChar;

                switch (option)
                {
                    case '1':
                        await WebhookHandler.SendWebhookMessage();
                        break;
                    case '2':
                        await GetInfoServerHandler.GetServerInfo();
                        break;
                    case '3':
                        await ServerMonitoringHandler.MonitorServer();
                        break;
                    case '4':
                        await GetMembersInfoHandler.GetMembersInfo();
                        break;
                    case '5':
                        return;
                    default:
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n\t\t\t\t[ERROR] Invalid option. Please try again.");
                        Console.ResetColor();
                        Console.WriteLine("\n\t\t\t\tPress any key to return to the menu.");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
}
