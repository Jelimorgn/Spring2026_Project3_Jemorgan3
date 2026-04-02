using System.Collections.Generic;

namespace Spring2026_Project3_Jemorgan3.Models
{
    public class MovieDetailsViewModel
    {
        // The main Movie object from the database
        public Movie Movie { get; set; }

        // The list of 5 AI Reviews and their scores
        public List<AiReviewItem> Reviews { get; set; } = new List<AiReviewItem>();

        // The overall average sentiment
        public double AverageSentiment { get; set; }

        // Error message if AI fails
        public string? ErrorMessage { get; set; }
    }
}