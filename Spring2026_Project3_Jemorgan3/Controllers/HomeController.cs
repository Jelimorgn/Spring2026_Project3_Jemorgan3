using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Spring2026_Project3_Jemorgan3.Models;
using Spring2026_Project3_Jemorgan3.Services;
using VaderSharp2; // Required for sentiment analysis

namespace Spring2026_Project3_Jemorgan3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateReview(string movieTitle)
        {
            if (string.IsNullOrEmpty(movieTitle)) return View("Index");

            try
            {
                var aiService = new AIService(_configuration);

                // 1. Get the 5 raw reviews from Azure (Phase 2 change)
                List<string> rawReviews = await aiService.GenerateFiveReviews(movieTitle);

                // 2. Setup VaderSharp2
                SentimentIntensityAnalyzer analyzer = new SentimentIntensityAnalyzer();
                var processedReviews = new List<AiReviewItem>();

                // 3. Loop through each review and calculate sentiment
                foreach (var text in rawReviews)
                {
                    var results = analyzer.PolarityScores(text);

                    processedReviews.Add(new AiReviewItem
                    {
                        ReviewText = text,
                        SentimentScore = results.Compound // Vader's score (-1.0 to 1.0)
                    });
                }

                // 4. Calculate the average for the heading
                double average = processedReviews.Count > 0 ? processedReviews.Average(r => r.SentimentScore) : 0;

                // 5. Send to View (Temporary: Using ViewBag until we make the full ViewModel)
                ViewBag.ReviewList = processedReviews;
                ViewBag.AverageScore = average;
                ViewBag.MovieName = movieTitle;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating reviews");
                ViewBag.Error = "AI Service failed. Check your Azure connection.";
            }

            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}