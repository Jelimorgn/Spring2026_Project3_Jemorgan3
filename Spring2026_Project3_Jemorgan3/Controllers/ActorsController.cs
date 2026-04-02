using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_Jemorgan3.Data;
using Spring2026_Project3_Jemorgan3.Models;
using Spring2026_Project3_Jemorgan3.Services;
using VaderSharp2;
using Microsoft.Extensions.Configuration;

namespace Spring2026_Project3_Jemorgan3.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ActorsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actors.ToListAsync());
        }

        // GET: Actors/Details/5
        // REWRITTEN TO USE VIEWMODEL (NO VIEWBAG)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors
                .Include(a => a.ActorMovies)
                    .ThenInclude(am => am.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (actor == null) return NotFound();

            // 1. Create the ViewModel instance
            var viewModel = new ActorDetailsViewModel
            {
                Actor = actor
            };

            try
            {
                // 2. AI Logic
                var aiService = new AIService(_configuration);
                List<string> rawTweets = await aiService.GenerateTenTweets(actor.Name);

                // 3. Sentiment Logic
                SentimentIntensityAnalyzer analyzer = new SentimentIntensityAnalyzer();

                foreach (var tweet in rawTweets)
                {
                    var results = analyzer.PolarityScores(tweet);
                    viewModel.Tweets.Add(new AiReviewItem
                    {
                        ReviewText = tweet,
                        SentimentScore = results.Compound
                    });
                }

                // 4. Calculate Average into the ViewModel
                if (viewModel.Tweets.Any())
                {
                    viewModel.AverageSentiment = viewModel.Tweets.Average(t => t.SentimentScore);
                }
            }
            catch (Exception)
            {
                // 5. Catch errors inside the ViewModel
                viewModel.ErrorMessage = "AI Twitter Service is currently unavailable.";
            }

            // 6. Return the ViewModel object to the View
            return View(viewModel);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Actor actor, IFormFile PhotoFile)
        {
            ModelState.Remove("Photo");

            if (PhotoFile == null || PhotoFile.Length == 0)
            {
                ModelState.AddModelError("Photo", "An Actor Photo is required.");
            }

            if (ModelState.IsValid)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await PhotoFile.CopyToAsync(memoryStream);
                    actor.Photo = memoryStream.ToArray();
                }

                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors.FindAsync(id);
            if (actor == null) return NotFound();

            return View(actor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Actor actor, IFormFile? PhotoFile)
        {
            if (id != actor.Id) return NotFound();

            ModelState.Remove("Photo");

            if (ModelState.IsValid)
            {
                try
                {
                    if (PhotoFile != null && PhotoFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await PhotoFile.CopyToAsync(memoryStream);
                            actor.Photo = memoryStream.ToArray();
                        }
                    }
                    else
                    {
                        var existingActor = await _context.Actors.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                        if (existingActor != null)
                        {
                            actor.Photo = existingActor.Photo;
                        }
                    }

                    _context.Update(actor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var actor = await _context.Actors
                .FirstOrDefaultAsync(m => m.Id == id);

            if (actor == null) return NotFound();

            return View(actor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actors.FindAsync(id);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actors.Any(e => e.Id == id);
        }
    }
}