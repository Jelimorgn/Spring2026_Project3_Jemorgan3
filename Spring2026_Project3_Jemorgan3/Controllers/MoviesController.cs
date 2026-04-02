using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spring2026_Project3_Jemorgan3.Data;
using Spring2026_Project3_Jemorgan3.Models;
using Spring2026_Project3_Jemorgan3.Services; // For AI
using VaderSharp2; // For Sentiment
using Microsoft.Extensions.Configuration; // For Secrets

namespace Spring2026_Project3_Jemorgan3.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        // Constructor updated to include IConfiguration for the AI Service
        public MoviesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        // GET: Movies/Details/5
        // REWRITTEN TO USE VIEWMODEL (NO VIEWBAG)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // 1. Fetch the movie and include the actors via the join table
            var movie = await _context.Movies
                .Include(m => m.ActorMovies)
                    .ThenInclude(am => am.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            // 2. Initialize the ViewModel
            var viewModel = new MovieDetailsViewModel
            {
                Movie = movie
            };

            try
            {
                // 3. Initialize the AI Service and call for 5 reviews (ONE API CALL)
                var aiService = new AIService(_configuration);
                List<string> rawReviews = await aiService.GenerateFiveReviews(movie.Title);

                // 4. Analyze each review with VaderSharp2
                SentimentIntensityAnalyzer analyzer = new SentimentIntensityAnalyzer();

                foreach (var reviewText in rawReviews)
                {
                    var results = analyzer.PolarityScores(reviewText);
                    viewModel.Reviews.Add(new AiReviewItem
                    {
                        ReviewText = reviewText,
                        SentimentScore = results.Compound
                    });
                }

                // 5. Calculate overall average sentiment for the heading
                if (viewModel.Reviews.Any())
                {
                    viewModel.AverageSentiment = viewModel.Reviews.Average(r => r.SentimentScore);
                }
            }
            catch (Exception)
            {
                // 6. Handle errors within the ViewModel
                viewModel.ErrorMessage = "The Movie AI Review service is currently unavailable.";
            }

            // 7. Return the ViewModel object to the View
            return View(viewModel);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile PosterFile)
        {
            ModelState.Remove("Poster");

            if (PosterFile == null || PosterFile.Length == 0)
            {
                ModelState.AddModelError("Poster", "The Movie Poster is required.");
            }

            if (ModelState.IsValid)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await PosterFile.CopyToAsync(memoryStream);
                    movie.Poster = memoryStream.ToArray();
                }

                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/AssignActors/5
        public async Task<IActionResult> AssignActors(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            var viewModel = new MovieActorViewModel
            {
                Movie = movie,
                ActorList = new SelectList(_context.Actors, "Id", "Name")
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignActors(int id, MovieActorViewModel viewModel)
        {
            // Requirement: Prevent duplicate relationships
            var exists = _context.ActorMovies.Any(am => am.MovieId == id && am.ActorId == viewModel.SelectedActorId);

            if (!exists && viewModel.SelectedActorId != 0)
            {
                var actorMovie = new ActorMovie
                {
                    MovieId = id,
                    ActorId = viewModel.SelectedActorId
                };

                _context.Add(actorMovie);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            return View(movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie)
        {
            if (id != movie.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            return View(movie);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}