using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordTools
{
    public static class WebhookHandler
    {
        public static async Task SendWebhookMessage()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t\t\t===============================================");
            Console.WriteLine("\t\t\t\t        Send Webhook Message");
            Console.WriteLine("\t\t\t\t===============================================");

            Console.WriteLine("\n\t\t\t\t 0. Return to main menu");

            Console.Write("\n\t\t\t\t Webhook URL (or 0 ro return): ");
            string webhook = Console.ReadLine();

            if (webhook == "0")
            {
                return;
            }

            if (!Uri.IsWellFormedUriString(webhook, UriKind.Absolute))
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\t\t\t\t[ERROR] Invalid Webhook URL. Please try again.");
                Console.ResetColor();
                Console.WriteLine("\n\t\t\t\tPress any key to return to the menu.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("\n\t\t\t\t 1. Normal Message");
            Console.WriteLine("\n\t\t\t\t 2. Embed Message");
            Console.WriteLine("\n\t\t\t\t 3. Send Message with Embed");

            Console.Write("\n\t\t\t\t Choose message type: ");
            string messageType = Console.ReadLine();

            string message = string.Empty;
            EmbedMessage embed = null;
            bool isEmbedMessage = false;

            if (messageType == "1")
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\t\t\t\t===============================================");
                Console.WriteLine("\t\t\t\t        Send Normal Message");
                Console.WriteLine("\t\t\t\t===============================================");
                Console.Write("\n\t\t\t\t Message: ");
                message = Console.ReadLine();
            }
            else if (messageType == "2")
            {
                isEmbedMessage = true;
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\t\t\t\t===============================================");
                Console.WriteLine("\t\t\t\t        Send Embed Message");
                Console.WriteLine("\t\t\t\t===============================================");
                Console.Write("\n\t\t\t\t Embed Title: ");
                string title = Console.ReadLine();
                Console.Write("\n\t\t\t\t Embed Description: ");
                string description = Console.ReadLine();
                Console.Write("\n\t\t\t\t Embed Color (Hex format, e.g., #FF5733): ");
                string color = Console.ReadLine();
                Console.Write("\n\t\t\t\t Thumbnail Image URL (optional): ");
                string thumbnailUrl = Console.ReadLine();
                Console.Write("\n\t\t\t\t Embed Image URL (optional): ");
                string imageUrl = Console.ReadLine();

                embed = new EmbedMessage
                {
                    Title = title,
                    Description = description,
                    Color = color,
                    ImageUrl = imageUrl,
                    ThumbnailUrl = thumbnailUrl,
                };
            }
            else if (messageType == "3")
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\t\t\t\t===============================================");
                Console.WriteLine("\t\t\t\t        Send Message with Embed");
                Console.WriteLine("\t\t\t\t===============================================");

                Console.Write("\n\t\t\t\t Message: ");
                message = Console.ReadLine();

                isEmbedMessage = true;
                Console.Write("\n\t\t\t\t Embed Title: ");
                string title = Console.ReadLine();
                Console.Write("\n\t\t\t\t Embed Description: ");
                string description = Console.ReadLine();
                Console.Write("\n\t\t\t\t Embed Color (Hex format, e.g., #FF5733): ");
                string color = Console.ReadLine();
                Console.Write("\n\t\t\t\t Thumbnail Image URL (optional): ");
                string thumbnailUrl = Console.ReadLine();
                Console.Write("\n\t\t\t\t Embed Image URL (optional): ");
                string imageUrl = Console.ReadLine();

                embed = new EmbedMessage
                {
                    Title = title,
                    Description = description,
                    Color = color,
                    ImageUrl= imageUrl,
                    ThumbnailUrl = thumbnailUrl,
                };
            }

            Console.Write("\n\t\t\t\t Would you like to preview the message before sending? (Y/N): ");
            string previewChoice = Console.ReadLine().ToUpper();

            if (previewChoice == "Y")
            {
                if (messageType == "3")
                {
                    await PreviewBothMessages(webhook, message, embed);
                }
                else if (isEmbedMessage)
                {
                    await PreviewEmbedMessage(webhook, embed);
                }
                else
                {
                    await PreviewNormalMessage(webhook, message);
                }
            }

            Console.Write("\n\t\t\t\t Do you want to send the message? (Y/N): ");
            string sendChoice = Console.ReadLine().ToUpper();

            if (sendChoice == "Y")
            {
                if (messageType == "3")
                {
                    await SendBothMessages(webhook, message, embed);
                }
                else if (isEmbedMessage)
                {
                    await SendEmbedMessage(webhook, embed);
                }
                else
                {
                    await SendNormalMessage(webhook, message);
                }
            }
            else
            {
                Console.Clear();
                Console.WriteLine("\n\t\t\t\t Message sending cancelled.");
                Console.WriteLine("\n\t\t\t\tPress any key to return to the menu.");
                Console.ReadKey();
            }
        }

        private static async Task PreviewNormalMessage(string webhook, string message)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t\t\t===============================================");
            Console.WriteLine("\t\t\t\t        Preview Normal Message");
            Console.WriteLine("\t\t\t\t===============================================");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n\t\t\t\tPreview of your Normal Message:");
            Console.WriteLine($"\n\t\t\t\tMessage: {message}");
            Console.ResetColor();
        }

        private static async Task PreviewEmbedMessage(string webhook, EmbedMessage embed)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t\t\t===============================================");
            Console.WriteLine("\t\t\t\t        Preview Embed Message");
            Console.WriteLine("\t\t\t\t===============================================");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n\t\t\t\tPreview of your Embed Message:");

            Console.WriteLine($"\n\t\t\t\tTitle: {embed.Title}");
            Console.WriteLine($"\t\t\t\tDescription: {embed.Description}");
            Console.WriteLine($"\t\t\t\tColor: {embed.Color}");
            Console.WriteLine($"\t\t\t\tThumbnail Image URL: {embed.ImageUrl ?? "None"}");
            Console.WriteLine($"\t\t\t\tImage URL: {embed.ImageUrl ?? "None"}");
            Console.ResetColor();
        }

        private static async Task PreviewBothMessages(string webhook, string message, EmbedMessage embed)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\t\t\t\t===============================================");
            Console.WriteLine("\t\t\t\t     Preview Message with Embed");
            Console.WriteLine("\t\t\t\t===============================================");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n\t\t\t\tPreview of your Messages:");
            Console.WriteLine($"\n\t\t\t\tMessage: {message}");
            Console.WriteLine("\t\t\t\tEmbed Message:");
            Console.WriteLine($"\t\t\t\tTitle: {embed.Title}");
            Console.WriteLine($"\t\t\t\tDescription: {embed.Description}");
            Console.WriteLine($"\t\t\t\tColor: {embed.Color}");
            Console.WriteLine($"\t\t\t\tThumbnail Image URL: {embed.ImageUrl ?? "None"}");
            Console.WriteLine($"\t\t\t\tImage URL: {embed.ImageUrl ?? "None"}");
            Console.ResetColor();
        }

        private static async Task SendNormalMessage(string webhook, string message)
        {
            string json = $"{{\"content\":\"{message}\"}}";
            using (HttpClient client = new HttpClient())
            {
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(webhook, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n\t\t\t\tMessage sent successfully!");
                }
                else
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n\t\t\t\tFailed to send message. Status Code: {response.StatusCode}");
                }
            }
        }

        private static async Task SendEmbedMessage(string webhook, EmbedMessage embed)
        {
            string colorHex = embed.Color.StartsWith("#") ? embed.Color.Substring(1) : embed.Color;
            int color = int.Parse(colorHex, System.Globalization.NumberStyles.HexNumber);

            string json = $"{{\"embeds\":[{{\"title\":\"{embed.Title}\",\"description\":\"{embed.Description}\",\"color\":{color},\"image\":{{\"url\":\"{embed.ImageUrl ?? ""}\"}},\"thumbnail\":{{\"url\":\"{embed.ThumbnailUrl ?? ""}\"}}}}]}}";

            using (HttpClient client = new HttpClient())
            {
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(webhook, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n\t\t\t\tEmbed message sent successfully!");
                }
                else
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n\t\t\t\tFailed to send embed message. Status Code: {response.StatusCode}");
                }
            }

        }

        private static async Task SendBothMessages(string webhook, string message, EmbedMessage embed)
        {
            string colorHex = embed.Color.StartsWith("#") ? embed.Color.Substring(1) : embed.Color;
            int color = int.Parse(colorHex, System.Globalization.NumberStyles.HexNumber);

            string json = $"{{\"content\":\"{message}\",\"embeds\":[{{\"title\":\"{embed.Title}\",\"description\":\"{embed.Description}\",\"color\":{color},\"image\":{{\"url\":\"{embed.ImageUrl ?? ""}\"}},\"thumbnail\":{{\"url\":\"{embed.ThumbnailUrl ?? ""}\"}}}}]}}";


            using (HttpClient client = new HttpClient())
            {
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(webhook, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n\t\t\t\tBoth messages sent successfully!");
                }
                else
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n\t\t\t\tFailed to send messages. Status Code: {response.StatusCode}");
                }
            }
        }
    }
}