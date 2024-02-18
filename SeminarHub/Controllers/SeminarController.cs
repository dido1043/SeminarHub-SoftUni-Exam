using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeminarHub.Data;
using SeminarHub.Data.Models;
using SeminarHub.Models.ViewModels;
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
