using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2024_Assignment3_adrutherford.Data;
using Fall2024_Assignment3_adrutherford.Models;
using System.ClientModel;

namespace Fall2024_Assignment3_adrutherford.Controllers
{
    public class ActorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string _apiKey;
        private readonly string _apiEndpoint;
        private readonly ApiKeyCredential _apiKeyCredential;
        private const string AiDeployment = "gpt-35-turbo";
        public ActorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetActorPhoto(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null || actor.Photo == null)
            {
                return NotFound();
            }

            var data = actor.Photo;
            return File(data, "image/jpg");
        }

        // GET: Actor
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actor.ToListAsync());
        }

        // GET: Actor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            var movies = await _context.MovieActor
                .Include(cs => cs.Movie)
                .Where(cs => cs.ActorId == actor.Id)
                .Select(cs => cs.Movie)
                .ToListAsync();

           // var (reviews, sentimentScores, sentimentAverage) = await GenerateAiReviews(actor.Name);

            var vm = new ActorDetailsViewModel(actor, movies);

            return View(vm);
        }

        // GET: Actor/Create
        public IActionResult Create()
        {
            return View();
        }
        
        // POST: Actor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,Link")] Actor actor, IFormFile? photo)
        {
            if (ModelState.IsValid)
            {
                if (photo != null && photo!.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    photo.CopyTo(memoryStream);
                    actor.Photo = memoryStream.ToArray();

                    _context.Add(actor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(nameof(actor.Photo), "Photo is required");
                }
            }
            return View(actor);
        }

        // GET: Actor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,Link")] Actor actor, IFormFile? Photo)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currActor = await _context.Actor.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                    if (currActor == null)
                    {
                        return NotFound();
                    }

                    currActor.Name = actor.Name;
                    currActor.Link = actor.Link;
                    currActor.Age = actor.Age;
                    currActor.Gender = actor.Gender;

                    if (Photo != null && Photo.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();
                        await Photo.CopyToAsync(memoryStream);
                        currActor.Photo = memoryStream.ToArray();
                    }
                    _context.Update(currActor);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(actor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
            }
            return View(actor);
        }

        // GET: Actor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Actor.FindAsync(id);
            if (student != null)
            {
                _context.Actor.Remove(student);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actor.Any(e => e.Id == id);
        }
    }
}
