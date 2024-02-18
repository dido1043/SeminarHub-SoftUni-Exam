using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeminarHub.Data;
using SeminarHub.Data.Models;
using SeminarHub.Models.ViewModels;
using System.Globalization;
using System.Security.Claims;

namespace SeminarHub.Controllers
{
    [Authorize]
    public class SeminarController : Controller
    {
        private readonly SeminarHubDbContext data;
        public SeminarController(SeminarHubDbContext context)
        {
            data = context;
        }
        [HttpGet]
        public async Task<IActionResult> All()
        {
            var model = await data.Seminars
                .AsNoTracking()
                .Select(s => new SeminarInfoViewModel(
                    s.Id,
                    s.Topic,
                    s.Lecturer,
                    s.Details,
                    s.DateAndTime,
                    s.Category.Name,
                    s.Organizer.UserName))
                .ToListAsync();
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Joined()
        {
            var userId = GetUser();
            var seminars = await data.SeminarParticipants
                .AsNoTracking()
                .Where(sp => sp.ParticipantId == userId)
                .Select(p => new SeminarInfoViewModel(
                    p.Seminar.Id,
                    p.Seminar.Topic,
                    p.Seminar.Lecturer,
                    p.Seminar.Details,
                    p.Seminar.DateAndTime,
                    p.Seminar.Category.Name,
                    p.Seminar.Organizer.UserName))
                .ToListAsync();

            return View(seminars);
        }
        [HttpPost]
        public async Task<IActionResult> Join(int id)
        {
            //Get seminar
            var seminar = await data.Seminars
                .Where(s => s.Id == id)
                .Include(sp => sp.SeminarParticipants)
                .FirstOrDefaultAsync();
            //Check validity
            if (seminar == null)
            {
                return BadRequest();
            }
            //Get user
            var userId = GetUser();
            //Check if user is is joined in this seminar
            if (!seminar.SeminarParticipants.Any(p => p.ParticipantId == userId))
            {
                seminar.SeminarParticipants.Add(new SeminarParticipant()
                {
                    SeminarId = seminar.Id,
                    ParticipantId = userId
                });
            }
            await data.SaveChangesAsync();
            return RedirectToAction("Joined", "Seminar");
        }


        [HttpPost]
        public async Task<IActionResult> Leave(int id)
        {
            var seminar = await data.Seminars
                .Where(s => s.Id == id)
                .Include(sp => sp.SeminarParticipants)
                .FirstOrDefaultAsync();
            if (seminar == null)
            {
                return BadRequest();
            }

            var userId = GetUser();
            var sp = seminar.SeminarParticipants.FirstOrDefault(p => p.ParticipantId == userId);

            if (sp == null)
            {
                return BadRequest();
            }

            data.SeminarParticipants.Remove(sp);
            await data.SaveChangesAsync();
            return RedirectToAction("Joined", "Seminar");
        }
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var seminarForm = new FormViewModel();
            seminarForm.Categories = await GetCategories();
            return View(seminarForm);

        }
        [HttpPost]
        public async Task<IActionResult> Add(FormViewModel model)
        {
            DateTime dateAndTime = DateTime.Now;

            //Check date and time format
            if (!DateTime.TryParseExact(
                model.DateAndTime,
                ValidationConstants.DateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dateAndTime))
            {
                ModelState.AddModelError(nameof(model.DateAndTime), $"Invalid date: Format must be {ValidationConstants.DateTimeFormat}");
            }
            if (model.Duration < 30 || model.Duration > 180)
            {
                ModelState.AddModelError(nameof(model.Duration), "Invalid duration time: Duration time must be between 30 and 180 min");
            }
            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategories();
                return View(model);
            }

            var entity = new Seminar()
            {
                Topic = model.Topic,
                Lecturer = model.Lecturer,
                Details = model.Details,
                DateAndTime = dateAndTime,
                Duration = model.Duration,
                CategoryId = model.CategoryId,
                OrganizerId = GetUser()
            };
            await data.Seminars.AddAsync(entity);
            await data.SaveChangesAsync();
            return RedirectToAction("All", "Seminar");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var seminar = await data.Seminars.FindAsync(id);
            if (seminar == null)
            {
                return BadRequest();
            }

            var userId = GetUser();
            if (seminar.OrganizerId != userId)
            {
                return Unauthorized();
            }

            var model = new FormViewModel()
            {
                Topic = seminar.Topic,
                Lecturer = seminar.Lecturer,
                Details = seminar.Details,
                DateAndTime = seminar.DateAndTime.ToString(ValidationConstants.DateTimeFormat),
                Duration = seminar.Duration,
                CategoryId = seminar.CategoryId
            };
            model.Categories = await GetCategories();

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(FormViewModel model, int id)
        {
            var seminar = await data.Seminars.FindAsync(id);
            if (seminar == null)
            {
                return BadRequest();
            }
            var userId = GetUser();
            if (seminar.OrganizerId != userId)
            {
                return Unauthorized();
            }

            DateTime dateAndTime = DateTime.Now;

            //Check date and time format
            if (!DateTime.TryParseExact(
                model.DateAndTime,
                ValidationConstants.DateTimeFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dateAndTime))
            {
                ModelState.AddModelError(nameof(model.DateAndTime), $"Invalid date: Format must be {ValidationConstants.DateTimeFormat}");
            }

            if (model.Duration < 30 || model.Duration > 180)
            {
                ModelState.AddModelError(nameof(model.Duration), "Invalid duration time: Duration time must be between 30 and 180 min");
            }
            seminar.Topic = model.Topic;
            seminar.Lecturer = model.Lecturer;
            seminar.Details = model.Details;
            seminar.DateAndTime = dateAndTime;
            seminar.Duration = model.Duration;
            seminar.CategoryId = model.CategoryId;

            await data.SaveChangesAsync();
            return RedirectToAction("All", "Seminar");
        }
        //Details method
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var model = await data.Seminars
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new DetailsViewModel()
                {
                    Id = s.Id,
                    Topic = s.Topic,
                    DateAndTime = s.DateAndTime.ToString(ValidationConstants.DateTimeFormat),
                    Duration = s.Duration,
                    Lecturer = s.Lecturer,
                    Category = s.Category.Name,
                    Details = s.Details,
                    Organizer = s.Organizer.UserName

                }).FirstOrDefaultAsync();
            return View(model);
        }
        //Delete method
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var seminar = await data.Seminars.FindAsync(id);
            if (seminar == null)
            {
                return BadRequest();
            }
            if (seminar.OrganizerId != GetUser())
            {
                return Unauthorized();
            }
            var model = new DeleteViewModel()
            {
                Id = seminar.Id,
                Topic = seminar.Topic,
                DateAndTime = seminar.DateAndTime
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var seminar = await data.Seminars.FindAsync(id);
            if (seminar == null)
            {
                return NotFound();
            }
            if (seminar.OrganizerId != GetUser())
            {
                return Unauthorized();
            }
            data.Seminars.Remove(seminar);
            await data.SaveChangesAsync();
            return RedirectToAction("All", "Seminar");
        }
        private string GetUser()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }
        private async Task<IEnumerable<CategoryViewModel>> GetCategories()
        {
            return await data.Categories
                .AsNoTracking()
                .Select(t => new CategoryViewModel()
                {
                    Id = t.Id,
                    Name = t.Name,
                })
                .ToListAsync();
        }
    }
}
