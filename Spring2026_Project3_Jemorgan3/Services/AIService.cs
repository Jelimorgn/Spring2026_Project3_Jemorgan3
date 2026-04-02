using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;

namespace Spring2026_Project3_Jemorgan3.Services
{
    public class AIService
    {
        private readonly IConfiguration _configuration;

        public AIService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Method for Movies (5 Reviews)
        public async Task<List<string>> GenerateFiveReviews(string movieTitle)
        {
            string endpoint = _configuration["AzureOpenAI:Endpoint"];
            string key = _configuration["AzureOpenAI:ApiKey"];
            string deployment = _configuration["AzureOpenAI:DeploymentName"];

            AzureOpenAIClient client = new(new Uri(endpoint), new AzureKeyCredential(key));
            ChatClient chatClient = client.GetChatClient(deployment);

            string prompt = $"Write 5 distinct, 1-sentence reviews for the movie '{movieTitle}'. " +
                            "Separate each review with the symbol ##. Do not include numbers or bullet points.";

            ChatCompletion completion = await chatClient.CompleteChatAsync(prompt);
            string response = completion.Content[0].Text;

            // Improved split to handle potential new lines from the AI
            return response.Split(new[] { "##", "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(r => r.Trim())
                           .ToList();
        }

        // IMPROVED: Method for Actors (10 Tweets - One API Call)
        public async Task<List<string>> GenerateTenTweets(string actorName)
        {
            string endpoint = _configuration["AzureOpenAI:Endpoint"];
            string key = _configuration["AzureOpenAI:ApiKey"];
            string deployment = _configuration["AzureOpenAI:DeploymentName"];

            AzureOpenAIClient client = new(new Uri(endpoint), new AzureKeyCredential(key));
            ChatClient chatClient = client.GetChatClient(deployment);

            // Refined Prompt: Explicitly banning hashtags and asking for clear separation
            string prompt = $"Write 10 short, realistic Twitter-style posts about the actor '{actorName}'. " +
                            "Requirements: No hashtags, no emojis, no numbers, and no bullet points. " +
                            "Crucial: Place the symbol ## between every single tweet so I can split them.";

            ChatCompletion completion = await chatClient.CompleteChatAsync(prompt);
            string response = completion.Content[0].Text;

            // Smarter Split: This handles cases where the AI might use ## AND a new line
            var tweets = response.Split(new[] { "##", "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(t => t.Trim())
                                 .Where(t => !string.IsNullOrWhiteSpace(t)) // Removes any empty items
                                 .Take(10) // Ensures we don't accidentally get 11 if there's a trailing ##
                                 .ToList();

            return tweets;
        }
    }
}