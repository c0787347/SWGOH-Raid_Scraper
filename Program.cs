using System.Diagnostics;
using System.Text;
using HtmlAgilityPack;

class Program
{
    //Guild’s raid-history base URL
    private const string GuildRaidBaseUrl = "https://swgoh.gg/g/nA8P3kQJRMyA7VqglhVOqQ/raid-history/";

    //Folder where local HTML saves are stored
    private const string HtmlFolder = "html_files";

    //Discord webhook URL
    private const string DiscordWebhookUrl = "https://discord.com/api/webhooks/1441898139081637918/cwSMpH5T9yxj2IJnrZvlWtP-HrwL_GMZ0RhqE1KK86HAprn8PrGXv1NlKFcL4tlGYmfc";

    static async Task Main()
    {
        
        string html = File.ReadAllText(filePath);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rows = doc.DocumentNode.SelectNodes("//table//tbody//tr");
        if (rows == null)
        {
            Console.WriteLine("ERROR: Could not find raid results table in the HTML file.");
            return;
        }

        Console.WriteLine("\n=== Players with no raid score ===\n");

        bool foundMissing = false;
        var missingList = new StringBuilder();

        foreach (var row in rows)
        {
            var cells = row.SelectNodes("td");
            if (cells == null || cells.Count < 3)
                continue;

            string score = cells[1].InnerText.Trim();
            string name = cells[2].InnerText.Trim();

            if (score == "--")
            {
                Console.WriteLine(name);
                missingList.AppendLine($"- {name}");
                foundMissing = true;
            }
        }

        //Build Discord message
        string discordMessage;
        if (!foundMissing)
        {
            discordMessage = $"**Raid `{raidId}` Report:**\nAll members contributed! 🎉";
        }
        else
        {
            discordMessage = $"**Raid `{raidId}` - Players with no raid score:**\n{missingList}";
        }

        //Send to Discord via webhook
        Console.WriteLine("\nSending report to Discord...");
        try
        {
            await SendDiscordMessage(DiscordWebhookUrl, discordMessage);
            Console.WriteLine("Posted to Discord!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to send message to Discord:");
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine("\nDone. Press any key to exit.");
        Console.ReadKey();
    }

    private static async Task SendDiscordMessage(string webhookUrl, string text)
    {
        using var client = new HttpClient();
        var json = $"{{\"content\": \"{EscapeJson(text)}\"}}";
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(webhookUrl, data);
        response.EnsureSuccessStatusCode();
    }

    private static string EscapeJson(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return input
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "")
            .Replace("\n", "\\n");
    }
}
