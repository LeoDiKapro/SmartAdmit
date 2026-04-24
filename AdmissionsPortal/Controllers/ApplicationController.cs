using AdmissionsPortal.Data;
using AdmissionsPortal.Models;
using AdmissionsPortal.Services;
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

        private readonly ClaudeService _claude;

        public ApplicationController(AppDbContext db,
                                     UserManager<AppUser> userManager,
                                     IWebHostEnvironment env,
                                     ClaudeService claude)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
            _claude = claude;
        }

        // GET /Application/Apply 
        public async Task<IActionResult> Apply()
        {
            var student = await _userManager.GetUserAsync(User);
            if (!student!.ProfileCompleted)
                return RedirectToAction("Complete", "Profile");
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
            vm.LanguageEntries ??= new List<string>();

            vm.LanguageCertificates ??= new List<IFormFile?>();

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

            // Prevent duplicate applications
            var existingApplication = await _db.Applications
                .FirstOrDefaultAsync(a =>
                    a.StudentId == student!.Id &&
                    a.UniversityId == vm.UniversityId &&
                    a.MasterProgramId == vm.MasterProgramId &&
                    a.Status != ApplicationStatus.AutoRejected);  // allow re-apply if rejected

            if (existingApplication != null)
            {
                ModelState.AddModelError(string.Empty,
                    "You have already applied to this program at this university. " +
                    "You can only re-apply if your previous application was rejected.");
                vm.Universities = await GetUniversityList();
                vm.MasterPrograms = await GetProgramList(vm.UniversityId);
                return View(vm);
            }

            // Save as Draft — not yet visible to admin, not yet screened
            var application = new Application
            {
                StudentId = student!.Id,
                UniversityId = vm.UniversityId,
                MasterProgramId = vm.MasterProgramId,
                GPA = vm.GPA,
                StudyYears = vm.StudyYears,
                DiplomaStatus = vm.DiplomaStatus,
                MotherTongue = vm.MotherTongue,
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

            // Save languages
            if (vm.LanguageEntries != null)
            for (int i = 0; i < vm.LanguageEntries.Count; i++)
            {
                var parts = vm.LanguageEntries[i].Split('|');
                if (parts.Length != 2) continue;

                var appLang = new ApplicationLanguage
                {
                    ApplicationId = application.Id,
                    Language = parts[0],
                    Level = parts[1]
                };

                // Save certificate if provided for this language
                var cert = i < vm.LanguageCertificates.Count ? vm.LanguageCertificates[i] : null;
                if (cert != null)
                {
                    var allowed = new[] { "application/pdf", "image/jpeg", "image/png" };
                    if (allowed.Contains(cert.ContentType))
                    {
                        var uniqueName = $"lang_{Guid.NewGuid()}{Path.GetExtension(cert.FileName)}";
                        var fullPath = Path.Combine(uploadsFolder, uniqueName);
                        using var stream = new FileStream(fullPath, FileMode.Create);
                        await cert.CopyToAsync(stream);

                        appLang.CertificatePath = Path.Combine("uploads", application.Id.ToString(), uniqueName);
                        appLang.CertificateFileName = cert.FileName;
                    }
                }

                _db.ApplicationLanguages.Add(appLang);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(UploadDocuments), new { id = application.Id });
        }


        // GET /Application/Details
        public async Task<IActionResult> Details(int id)
        {
            var student = await _userManager.GetUserAsync(User);
            var app = await _db.Applications
                .Include(a => a.University)
                .Include(a => a.MasterProgram)
                .Include(a => a.Documents)
                .Include(a => a.Languages)
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == student!.Id);

            if (app == null) return NotFound();
            return View(app);
        }

        // POST Application/Withdraw
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(int id)
        {
            var student = await _userManager.GetUserAsync(User);
            var app = await _db.Applications
                .FirstOrDefaultAsync(a => a.Id == id && a.StudentId == student!.Id);

            if (app == null) return NotFound();

            // Only allow withdrawing Draft or UnderReview applications
            if (app.Status != ApplicationStatus.Draft &&
                app.Status != ApplicationStatus.UnderReview)
            {
                TempData["Error"] = "This application cannot be withdrawn.";
                return RedirectToAction(nameof(Details), new { id });
            }

            app.Status = ApplicationStatus.Withdrawn;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Your application has been withdrawn.";
            return RedirectToAction(nameof(Dashboard));
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
                .Include(a => a.MasterProgram)
                .FirstOrDefaultAsync(a => a.Id == applicationId && a.StudentId == student!.Id);

            if (app == null) return NotFound();

            // Only allow submitting a draft
            if (app.Status != ApplicationStatus.Draft)
                return RedirectToAction(nameof(Dashboard));

            //  Auto-screening runs HERE 
            app.Status = (app.GPA < app.MasterProgram.MinGPA || app.StudyYears < app.MasterProgram.MinYears)
                ? ApplicationStatus.AutoRejected
                : ApplicationStatus.UnderReview;

            app.SubmittedAt = DateTime.UtcNow;   // official submission time
            await _db.SaveChangesAsync();

            TempData["Success"] = app.Status == ApplicationStatus.AutoRejected
                ? "Your application was submitted but did not meet the minimum requirements."
                : "Your application was submitted successfully and is under review!";

            return RedirectToAction(nameof(Dashboard));
        }
        // ── POST /Application/Chat ────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            var student = await _userManager.GetUserAsync(User);

            var apps = await _db.Applications
                .Include(a => a.University)
                .Include(a => a.MasterProgram)
                .Where(a => a.StudentId == student!.Id)
                .ToListAsync();

            var languages = await _db.ApplicationLanguages
                .Where(l => l.Application.StudentId == student!.Id)
                .Select(l => $"{l.Language} ({l.Level})")
                .Distinct()
                .ToListAsync();

            var allPrograms = await _db.MasterPrograms
                .Include(p => p.University)
                .Select(p => $"{p.Name} at {p.University.Name} " +
                             $"(Min GPA: {p.MinGPA}, Min Years: {p.MinYears})")
                .ToListAsync();

            var studentGpa = apps.Any() ? apps.Max(a => a.GPA) : 0;
            var studentYears = apps.Any() ? apps.Max(a => a.StudyYears) : 0;

                    var systemPrompt = $"""
                You are a friendly university admissions advisor chatbot helping a student
                find the best Master's program. Be concise, encouraging and specific.
                Keep responses to 2-4 sentences unless the student asks for more detail.

                Student Profile:
                Name: {student!.FullName}
                GPA: {studentGpa:F2} / 4.0
                Years of Study: {studentYears}
                Languages: {string.Join(", ", languages.Any() ? languages : new List<string> { "Not specified" })}

                Available Programs:
                {string.Join("\n", allPrograms)}

                Current Applications:
                {(apps.Any() ? string.Join("\n", apps.Select(a => $"- {a.MasterProgram.Name} at {a.University.Name} ({a.Status})")) : "None yet")}

                Help the student understand their chances, which programs suit them best,
                and what they can do to improve their application.
                """;

            var history = request.Messages
                .Select(m => (m.Role, m.Content))
                .ToList();

            var reply = await _claude.Chat(history, systemPrompt);
            return Ok(new { reply });
        }
        // ── GET /Application/Dashboard ───────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            var student = await _userManager.GetUserAsync(User);

            if (!student!.ProfileCompleted)
                return RedirectToAction("Complete", "Profile");

            var apps = await _db.Applications
                .Include(a => a.University)
                .Include(a => a.MasterProgram)
                .Where(a => a.StudentId == student.Id)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync();

            ViewBag.StudentName = student.FullName;
            ViewBag.Total = apps.Count;
            ViewBag.UnderReview = apps.Count(a => a.Status == ApplicationStatus.UnderReview);
            ViewBag.Accepted = apps.Count(a => a.Status == ApplicationStatus.Accepted);
            ViewBag.Rejected = apps.Count(a => a.Status == ApplicationStatus.AutoRejected);
            ViewBag.Drafts = apps.Count(a => a.Status == ApplicationStatus.Draft);
            ViewBag.Withdrawn = apps.Count(a => a.Status == ApplicationStatus.Withdrawn);

            return View(apps);
        }
    }
    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
    public class ChatRequest
    {
        public List<ChatMessage> Messages { get; set; } = new();
    }
}
