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
    [Authorize(Roles = "Student")]
    public class ApplicationController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ApplicationController(AppDbContext db,
                                     UserManager<AppUser> userManager,
                                     IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        // GET /Application/Apply 
        public async Task<IActionResult> Apply()
        {
            var vm = new ApplicationViewModel
            {
                Universities = await GetUniversityList()
            };
            return View(vm);
        }

        // POST /Application/Apply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(ApplicationViewModel vm)
        {
            if (vm.Transcript == null)
                ModelState.AddModelError("Transcript", "Transcript is required.");

            if (vm.DiplomaStatus == DiplomaStatus.Completed && vm.Diploma == null)
                ModelState.AddModelError("Diploma", "Diploma is required when status is Completed.");

            if (!ModelState.IsValid)
            {
                vm.Universities = await GetUniversityList();
                vm.MasterPrograms = await GetProgramList(vm.UniversityId);
                return View(vm);
            }

            var student = await _userManager.GetUserAsync(User);

            // Save as Draft — not yet visible to admin, not yet screened
            var application = new Application
            {
                StudentId = student!.Id,
                UniversityId = vm.UniversityId,
                MasterProgramId = vm.MasterProgramId,
                GPA = vm.GPA,
                StudyYears = vm.StudyYears,
                DiplomaStatus = vm.DiplomaStatus,
                Status = ApplicationStatus.Draft,  
                SubmittedAt = DateTime.UtcNow
            };

            _db.Applications.Add(application);
            await _db.SaveChangesAsync();

            // Save required documents
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", application.Id.ToString());
            Directory.CreateDirectory(uploadsFolder);
            await SaveDocument(vm.Transcript!, DocumentType.Transcript, application.Id, uploadsFolder);

            if (vm.Diploma != null)
                await SaveDocument(vm.Diploma, DocumentType.Diploma, application.Id, uploadsFolder);

            await _db.SaveChangesAsync();

            // Go to step 2 — upload additional documents
            return RedirectToAction(nameof(UploadDocuments), new { id = application.Id });
        }

        // GET /Application/MyApplications 
        public async Task<IActionResult> MyApplications()
        {
            var student = await _userManager.GetUserAsync(User);
            var apps = await _db.Applications
                .Include(a => a.University)
                .Include(a => a.MasterProgram)
                .Where(a => a.StudentId == student!.Id)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            return View(apps);
        }

        // GET /Application/Details/5 
        public async Task<IActionResult> Details(int id)
        {
            var student = await _userManager.GetUserAsync(User);
            var app = await _db.Applications
                .Include(a => a.University)
                .Include(a => a.MasterProgram)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == student!.Id);

            if (app == null) return NotFound();
            return View(app);
        }

        // AJAX: GET /Application/GetPrograms?universityId=1 
        [HttpGet]
        public async Task<IActionResult> GetPrograms(int universityId)
        {
            var programs = await _db.MasterPrograms
                .Where(p => p.UniversityId == universityId)
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();

            return Json(programs);
        }

        // Helpers 
        private async Task<IEnumerable<SelectListItem>> GetUniversityList()
        {
            return await _db.Universities
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Name })
                .ToListAsync();
        }

        private async Task<IEnumerable<SelectListItem>> GetProgramList(int universityId)
        {
            return await _db.MasterPrograms
                .Where(p => p.UniversityId == universityId)
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToListAsync();
        }

        private async Task SaveDocument(IFormFile file, DocumentType type,
                                        int applicationId, string folder)
        {
            // Validate MIME type — only PDF and images accepted
            var allowed = new[] { "application/pdf", "image/jpeg", "image/png" };
            if (!allowed.Contains(file.ContentType))
                return;

            var uniqueName = $"{type}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(folder, uniqueName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            _db.Documents.Add(new Document
            {
                ApplicationId = applicationId,
                Type = type,
                FileName = file.FileName,
                FilePath = Path.Combine("uploads", applicationId.ToString(), uniqueName)
            });
        }
        //  GET /Application/UploadDocuments
        public async Task<IActionResult> UploadDocuments(int id)
        {
            var student = await _userManager.GetUserAsync(User);
            var app = await _db.Applications
                .Include(a => a.University)
                .Include(a => a.MasterProgram)
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == student!.Id);

            if (app == null) return NotFound();

            ViewBag.Application = app;
            ViewBag.ApplicationId = id;
            return View(app.Documents.ToList());
        }

        // POST /Application/UploadDocuments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocuments(AdditionalDocumentViewModel vm)
        {
            var student = await _userManager.GetUserAsync(User);
            var app = await _db.Applications
                .Include(a => a.Documents)
                .FirstOrDefaultAsync(a => a.Id == vm.ApplicationId && a.StudentId == student!.Id);

            if (app == null) return NotFound();

            if (vm.File == null)
            {
                TempData["Error"] = "Please select a file.";
                return RedirectToAction(nameof(UploadDocuments), new { id = vm.ApplicationId });
            }

            var allowed = new[] { "application/pdf", "image/jpeg", "image/png" };
            if (!allowed.Contains(vm.File.ContentType))
            {
                TempData["Error"] = "Only PDF, JPG and PNG files are accepted.";
                return RedirectToAction(nameof(UploadDocuments), new { id = vm.ApplicationId });
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", app.Id.ToString());
            Directory.CreateDirectory(uploadsFolder);
            await SaveDocument(vm.File, vm.Type, app.Id, uploadsFolder);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Document uploaded successfully.";
            return RedirectToAction(nameof(UploadDocuments), new { id = vm.ApplicationId });
        }

        // POST /Application/FinalSubmit 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalSubmit(int applicationId)
        {
            var student = await _userManager.GetUserAsync(User);
            var app = await _db.Applications
                .Include(a => a.University)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.StudentId == student!.Id);

            if (app == null) return NotFound();

            // Only allow submitting a draft
            if (app.Status != ApplicationStatus.Draft)
                return RedirectToAction(nameof(MyApplications));

            // ── Auto-screening runs HERE ──────────────────────────────────────────────
            app.Status = (app.GPA < app.University.MinGPA || app.StudyYears < app.University.MinYears)
                ? ApplicationStatus.AutoRejected
                : ApplicationStatus.UnderReview;

            app.SubmittedAt = DateTime.UtcNow;   // official submission time
            await _db.SaveChangesAsync();

            TempData["Success"] = app.Status == ApplicationStatus.AutoRejected
                ? "Your application was submitted but did not meet the minimum requirements."
                : "Your application was submitted successfully and is under review!";

            return RedirectToAction(nameof(MyApplications));
        }
    }
}
