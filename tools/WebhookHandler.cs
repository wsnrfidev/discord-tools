using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

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

            Console.Write("\n\t\t\t\t Webhook URL (or 0 to return): ");
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

            Console.Clear();
            Console.WriteLine("\t\t\t\t===============================================");
            Console.WriteLine("\t\t\t\t        Send Webhook Message");
            Console.WriteLine("\t\t\t\t===============================================");
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
                message = ReadMultilineInput("Message");
            }
            else if (messageType == "2" || messageType == "3")
            {
                if (messageType == "3")
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\t\t\t\t===============================================");
                    Console.WriteLine("\t\t\t\t        Send Message with Embed");
                    Console.WriteLine("\t\t\t\t===============================================");
                    message = ReadMultilineInput("Message");
                }
                else
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\t\t\t\t===============================================");
                    Console.WriteLine("\t\t\t\t        Send Embed Message");
                    Console.WriteLine("\t\t\t\t===============================================");
                }

                isEmbedMessage = true;
                Console.Write("\n\t\t\t\t Embed Title: ");
                string title = Console.ReadLine();
                string description = ReadMultilineInput("Embed Description");
                Console.Write("\n\t\t\t\t Embed Color (Hex format, e.g., #3498db): ");
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

                // added fields
                Console.Write("\n\t\t\t\t Do you want to add fields? (Y/N): ");
                string addFields = Console.ReadLine().ToUpper();
                while (addFields == "Y")
                {
                    Console.Write("\n\t\t\t\t Field Name: ");
                    string fieldName = Console.ReadLine();
                    string fieldValue = ReadMultilineInput("Field Value");
                    Console.Write("\t\t\t\t Inline? (Y/N): ");
                    string inlineChoice = Console.ReadLine().ToUpper();

                    embed.Fields.Add(new EmbedFields
                    {
                        Name = fieldName,
                        Value = fieldValue,
                        Inline = inlineChoice == "Y"
                    });

                    Console.Write("\n\t\t\t\t Add another field? (Y/N): ");
                    addFields = Console.ReadLine().ToUpper();
                }

                // added footer
                Console.Write("\n\t\t\t\t Footer Text (optional): ");
                embed.FooterText = Console.ReadLine();
                Console.Write("\t\t\t\t Footer Icon URL (optional): ");
                embed.FooterIcon = Console.ReadLine();
            }

            Console.Write("\n\t\t\t\t Would you like to preview the message before sending? (Y/N): ");
            string previewChoice = Console.ReadLine().ToUpper();

            if (previewChoice == "Y")
            {
                if (messageType == "3")
                {
                    await PreviewBothMessages(message, embed);
                }
                else if (isEmbedMessage)
                {
                    await PreviewEmbedMessage(embed);
                }
                else
                {
                    await PreviewNormalMessage(message);
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

        // added multi line
        private static string ReadMultilineInput(string prompt)
        {
            Console.WriteLine($"\n\t\t\t\t {prompt} (press ENTER twice to finish): ");
            var sb = new StringBuilder();
            string line;
            bool lastLineEmpty = false;

            while (true)
            {
                line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    if (lastLineEmpty) break; // hit enter twice -> done
                    lastLineEmpty = true;
                }
                else
                {
                    sb.AppendLine(line);
                    lastLineEmpty = false;
                }
            }

            return sb.ToString().TrimEnd().Replace("\r\n", "\n");
        }


        private static async Task PreviewNormalMessage(string message)
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

        private static async Task PreviewEmbedMessage(EmbedMessage embed)
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
            Console.WriteLine($"\t\t\t\tThumbnail Image URL: {embed.ThumbnailUrl ?? "None"}");
            Console.WriteLine($"\t\t\t\tImage URL: {embed.ImageUrl ?? "None"}");

            if (embed.Fields.Any())
            {
                Console.WriteLine("\t\t\t\tFields:");
                foreach (var field in embed.Fields)
                {
                    Console.WriteLine($"\t\t\t\t- {field.Name}: {field.Value} (Inline: {field.Inline})");
                }
            }

            if (!string.IsNullOrWhiteSpace(embed.FooterText))
            {
                Console.WriteLine($"\t\t\t\tFooter: {embed.FooterText}");
                Console.WriteLine($"\t\t\t\tFooter Icon: {embed.FooterIcon ?? "None"}");
            }

            Console.ResetColor();
        }

        private static async Task PreviewBothMessages(string message, EmbedMessage embed)
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
            Console.WriteLine($"\t\t\t\tThumbnail Image URL: {embed.ThumbnailUrl ?? "None"}");
            Console.WriteLine($"\t\t\t\tImage URL: {embed.ImageUrl ?? "None"}");

            if (embed.Fields.Any())
            {
                Console.WriteLine("\t\t\t\tFields:");
                foreach (var field in embed.Fields)
                {
                    Console.WriteLine($"\t\t\t\t- {field.Name}: {field.Value} (Inline: {field.Inline})");
                }
            }

            if (!string.IsNullOrWhiteSpace(embed.FooterText))
            {
                Console.WriteLine($"\t\t\t\tFooter: {embed.FooterText}");
                Console.WriteLine($"\t\t\t\tFooter Icon: {embed.FooterIcon ?? "None"}");
            }

            Console.ResetColor();
        }

        private static async Task SendNormalMessage(string webhook, string message)
        {
            var payload = new
            {
                content = message,
                allowed_mentions = new
                {
                    parse = new[] { "users", "roles", "everyone" }
                }
            };

            string json = JsonSerializer.Serialize(payload);

            using (HttpClient client = new HttpClient())
            {
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(webhook, content);
                PrintSendResult(response, "Message");
            }
        }


        private static async Task SendEmbedMessage(string webhook, EmbedMessage embed)
        {
            string colorHex = string.IsNullOrWhiteSpace(embed.Color) ? "3498db" :
                (embed.Color.StartsWith("#") ? embed.Color.Substring(1) : embed.Color);
            int color = int.Parse(colorHex, System.Globalization.NumberStyles.HexNumber);

            var embedObj = new
            {
                title = embed.Title,
                description = embed.Description,
                color = color,
                image = string.IsNullOrWhiteSpace(embed.ImageUrl) ? null : new { url = embed.ImageUrl },
                thumbnail = string.IsNullOrWhiteSpace(embed.ThumbnailUrl) ? null : new { url = embed.ThumbnailUrl },
                fields = embed.Fields.Any()
                    ? embed.Fields.Select(f => new { name = f.Name, value = f.Value, inline = f.Inline }).ToArray()
                    : null,
                footer = string.IsNullOrWhiteSpace(embed.FooterText) ? null : new { text = embed.FooterText, icon_url = embed.FooterIcon },
                timestamp = embed.Timestamp?.ToString("o")
            };

            var payload = new
            {
                embeds = new[] { embedObj },
                allowed_mentions = new { parse = new[] { "users", "roles", "everyone" } }
            };

            string json = JsonSerializer.Serialize(payload);

            using (HttpClient client = new HttpClient())
            {
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(webhook, content);
                PrintSendResult(response, "Embed message");
            }
        }


        private static async Task SendBothMessages(string webhook, string message, EmbedMessage embed)
        {
            string colorHex = string.IsNullOrWhiteSpace(embed.Color) ? "3498db" :
                (embed.Color.StartsWith("#") ? embed.Color.Substring(1) : embed.Color);
            int color = int.Parse(colorHex, System.Globalization.NumberStyles.HexNumber);

            var embedObj = new
            {
                title = embed.Title,
                description = embed.Description,
                c = color,
                image = string.IsNullOrWhiteSpace(embed.ImageUrl) ? null : new { url = embed.ImageUrl },
                thumbnail = string.IsNullOrWhiteSpace(embed.ThumbnailUrl) ? null : new { url = embed.ThumbnailUrl },
                fields = embed.Fields.Any()
                    ? embed.Fields.Select(f => new { name = f.Name, value = f.Value, inline = f.Inline }).ToArray()
                    : null,
                footer = string.IsNullOrWhiteSpace(embed.FooterText) ? null : new { text = embed.FooterText, icon_url = embed.FooterIcon },
                timestamp = embed.Timestamp?.ToString("o")
            };

            var payload = new
            {
                content = message,
                embeds = new[] { embedObj },
                allowed_mentions = new { parse = new[] { "users", "roles", "everyone" } }
            };

            string json = JsonSerializer.Serialize(payload);

            using (HttpClient client = new HttpClient())
            {
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(webhook, content);
                PrintSendResult(response, "Both messages");
            }
        }


        private static void PrintSendResult(HttpResponseMessage response, string type)
        {
            if (response.IsSuccessStatusCode)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n\t\t\t\t{type} sent successfully!");
            }
            else
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n\t\t\t\tFailed to send {type}. Status Code: {response.StatusCode}");
            }
        }
    }
}
