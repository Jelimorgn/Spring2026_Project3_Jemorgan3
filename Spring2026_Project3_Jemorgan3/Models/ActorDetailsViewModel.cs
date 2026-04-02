using System.Collections.Generic;

namespace Spring2026_Project3_Jemorgan3.Models
{
    public class ActorDetailsViewModel
    {
        // The Actor data from the database
        public Actor Actor { get; set; }

        // The list of 10 Tweets and their scores (replacing ViewBag.TweetList)
        public List<AiReviewItem> Tweets { get; set; } = new List<AiReviewItem>();

        // The overall average (replacing ViewBag.AverageScore)
        public double AverageSentiment { get; set; }

        // To handle AI errors safely
        public string? ErrorMessage { get; set; }
    }
}