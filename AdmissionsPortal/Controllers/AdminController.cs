using AdmissionsPortal.Data;
using AdmissionsPortal.Models;
using AdmissionsPortal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AdmissionsPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public AdminController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ── GET /Admin ────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            // Stats
            ViewBag.TotalStudents = (await _userManager.GetUsersInRoleAsync("Student")).Count;
            ViewBag.TotalReps = (await _userManager.GetUsersInRoleAsync("UniversityRep")).Count;
            ViewBag.TotalUniversities = await _db.Universities.CountAsync();
            ViewBag.TotalPrograms = await _db.MasterPrograms.CountAsync();
            ViewBag.TotalApplications = await _db.Applications.CountAsync();

            return View();
        }

        // ── GET /Admin/Staff ──────────────────────────────────────────────────
        public async Task<IActionResult> Staff()
        {
            var reps = await _userManager.GetUsersInRoleAsync("UniversityRep");
            var repList = reps.Select(r => new
            {
                r.Id,
                r.FullName,
                r.Email,
                University = _db.Universities.Find(r.UniversityId)?.Name ?? "—"
            }).ToList();

            ViewBag.Reps = repList;
            return View();
        }

        // ── GET /Admin/CreateStaff ────────────────────────────────────────────
        public async Task<IActionResult> CreateStaff()
        {
            var vm = new CreateStaffViewModel
            {
                Universities = await GetUniversityList()
            };
            return View(vm);
        }

        // ── POST /Admin/CreateStaff ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff(CreateStaffViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Universities = await GetUniversityList();
                return View(vm);
            }

            // Check email not already taken
            if (await _userManager.FindByEmailAsync(vm.Email) != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                vm.Universities = await GetUniversityList();
                return View(vm);
            }

            var user = new AppUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                UniversityId = vm.UniversityId,
                EmailConfirmed = true,
                ProfileCompleted = true
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                vm.Universities = await GetUniversityList();
                return View(vm);
            }

            await _userManager.AddToRoleAsync(user, "UniversityRep");
            TempData["Success"] = $"Staff account created for {vm.FirstName} {vm.LastName}.";
            return RedirectToAction(nameof(Staff));
        }

        // ── POST /Admin/DeleteStaff ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStaff(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "Staff account deleted.";
            return RedirectToAction(nameof(Staff));
        }

        // ── GET /Admin/Students ───────────────────────────────────────────────
        public async Task<IActionResult> Students()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");
            return View(students.OrderBy(s => s.LastName).ToList());
        }

        // ── POST /Admin/DeleteStudent ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "Student account deleted.";
            return RedirectToAction(nameof(Students));
        }

        // ── GET /Admin/Universities ───────────────────────────────────────────
        public async Task<IActionResult> Universities()
        {
            var unis = await _db.Universities
                .Include(u => u.MasterPrograms)
                .ToListAsync();
            return View(unis);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private async Task<IEnumerable<SelectListItem>> GetUniversityList()
        {
            return await _db.Universities
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Name
                })
                .ToListAsync();
        }
    }
}