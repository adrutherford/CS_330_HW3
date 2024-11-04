using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_adrutherford.Data;
using Fall2024_Assignment3_adrutherford.Models;
using System.Text.Json.Nodes;
using System.ClientModel;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using VaderSharp2;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using Azure.Identity;


namespace Fall2024_Assignment3_adrutherford.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _apiKey;
        private readonly string _apiEndpoint;
        private readonly ApiKeyCredential _apiKeyCredential;
        private const string AiDeployment = "gpt-35-turbo";

        public MovieController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movie
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
        }
        public async Task<IActionResult> GetMoviePhoto(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null || movie.Poster == null)
            {
                return NotFound();
            }

            var data = movie.Poster;
            return File(data, "image/jpg");
        }

        // GET: Movie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            var actors = await _context.MovieActor
               .Include(cs => cs.Actor)
               .Where(cs => cs.MovieId == movie.Id)
               .Select(cs => cs.Actor)
               .ToListAsync();

            //var (reviews, sentimentScores, sentimentAverage) = await GenerateAiReviews(movie.Title);

            var vm = new MovieDetailsViewModel(movie, actors);

            return View(vm);

        }

        

        // GET: Movie/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movie/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Genre,Link,releaseYear")] Movie movie, IFormFile? poster)
        {
            if (ModelState.IsValid)
            {
                if (poster != null && poster!.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    poster.CopyTo(memoryStream);
                    movie.Poster = memoryStream.ToArray();

                    _context.Add(movie);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(nameof(movie.Poster), "Photo is required");
                }
            }
            return View(movie);
        }
    

        // GET: Movie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movie/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Genre,Link,releaseYear")] Movie movie, IFormFile? Photo)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currMovie = await _context.Movie.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                    if (currMovie == null) {
                        return NotFound();
                    }

                    currMovie.Title = movie.Title;
                    currMovie.Link = movie.Link;
                    currMovie.Genre = movie.Genre;
                    currMovie.releaseYear = movie.releaseYear;

                    if (Photo != null && Photo.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await Photo.CopyToAsync(memoryStream);
                        currMovie.Poster = memoryStream.ToArray();
                    }

                    _context.Update(currMovie);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(movie);
        }

        // GET: Movie/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
