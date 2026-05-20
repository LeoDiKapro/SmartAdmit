using AdmissionsPortal.Data;
using AdmissionsPortal.Models;
using AdmissionsPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdmissionsPortal.Controllers
{
    [Authorize(Roles = "UniversityRep")]
    public class RepController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly ScoringService _scoring;
        private readonly ClaudeService _claude;

        public RepController(AppDbContext db,
                             UserManager<AppUser> userManager,
                             ScoringService scoring,
                             ClaudeService claude)
        {
            _db = db;
            _userManager = userManager;
            _scoring = scoring;
            _claude = claude;
        }



        // ── GET /Rep/Dashboard ────────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            var rep = await _userManager.GetUserAsync(User);
            var uniId = rep!.UniversityId!.Value;

            var apps = await _db.Applications
                .Include(a => a.MasterProgram)
                .Include(a => a.Student)
                .Where(a => a.UniversityId == uniId &&
                    a.Status != ApplicationStatus.Draft &&
                    a.Status != ApplicationStatus.Withdrawn)
                .ToListAsync();

            var university = await _db.Universities
                .Include(u => u.MasterPrograms)
                .FirstOrDefaultAsync(u => u.Id == uniId);

            ViewBag.University = university;
            ViewBag.Total = apps.Count;
            ViewBag.UnderReview = apps.Count(a => a.Status == ApplicationStatus.UnderReview);
            ViewBag.Accepted = apps.Count(a => a.Status == ApplicationStatus.Accepted);
            ViewBag.Rejected = apps.Count(a => a.Status == ApplicationStatus.AutoRejected);
            ViewBag.Programs = university!.MasterPrograms.ToList();

            return View(apps);
        }

        // ── GET /Rep/Rankings/5 (programId) ───────────────────────────────────
        public async Task<IActionResult> Rankings(int programId)
        {
            var rep = await _userManager.GetUserAsync(User);
            var uniId = rep!.UniversityId!.Value;

            var program = await _db.MasterPrograms
                .FirstOrDefaultAsync(p => p.Id == programId &&
                                          p.UniversityId == uniId);

            if (program == null) return NotFound();

            // Get or create scoring weights for this program
            var weights = await _db.ScoringWeights
                .FirstOrDefaultAsync(w => w.MasterProgramId == programId);

            if (weights == null)
            {
                weights = new ScoringWeights
                {
                    MasterProgramId = programId,
                    GPAWeight = 60,
                    YearsWeight = 20,
                    LanguageWeight = 20,
                    LanguageBonus = 0.5m,
                    DiplomaBonus = 1.0m,
                    RecommendationBonus = 0.5m,
                    DocumentBonus = 0.5m,
                    FieldMatchBonus = 1.0m
                };
                _db.ScoringWeights.Add(weights);
                await _db.SaveChangesAsync();
            }

            var applications = await _db.Applications
                .Include(a => a.Student)
                .Include(a => a.MasterProgram)
                .Include(a => a.Documents)
                .Include(a => a.Languages)
                .Where(a => a.MasterProgramId == programId &&
                            a.Status != ApplicationStatus.Draft &&
                            a.Status != ApplicationStatus.Withdrawn)
                .ToListAsync();

            var ranked = _scoring.RankApplications(
                applications, weights, program.MinGPA, program.MinYears);

            ViewBag.Program = program;
            ViewBag.Weights = weights;
            ViewBag.Spots = program.AvailableSpots;
            ViewBag.MinScore = program.MinScore;

            return View(ranked);
        }

        // ── POST /Rep/UpdateWeights ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateWeights(ScoringWeights weights)
        {
            // Validate weights add up to 100
            if (weights.GPAWeight + weights.YearsWeight + weights.LanguageWeight != 100)
            {
                TempData["Error"] = "GPA, Years and Language weights must add up to 100%.";
                return RedirectToAction(nameof(Rankings),
                    new { programId = weights.MasterProgramId });
            }

            var existing = await _db.ScoringWeights
                .FirstOrDefaultAsync(w => w.MasterProgramId == weights.MasterProgramId);

            if (existing != null)
            {
                existing.GPAWeight = weights.GPAWeight;
                existing.YearsWeight = weights.YearsWeight;
                existing.LanguageWeight = weights.LanguageWeight;
                existing.LanguageBonus = weights.LanguageBonus;
                existing.DiplomaBonus = weights.DiplomaBonus;
                existing.RecommendationBonus = weights.RecommendationBonus;
                existing.DocumentBonus = weights.DocumentBonus;
                existing.FieldMatchBonus = weights.FieldMatchBonus;
            }
            else
            {
                _db.ScoringWeights.Add(weights);
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Scoring weights updated.";
            return RedirectToAction(nameof(Rankings),
                new { programId = weights.MasterProgramId });
        }

        // ── GET /Rep/ApplicationDetail/5 ──────────────────────────────────────
        public async Task<IActionResult> ApplicationDetail(int id)
        {
            var rep = await _userManager.GetUserAsync(User);
            var uniId = rep!.UniversityId!.Value;

            var app = await _db.Applications
                .Include(a => a.Student)
                .Include(a => a.University)
                .Include(a => a.MasterProgram)
                .Include(a => a.Documents)
                .Include(a => a.Languages)
                .FirstOrDefaultAsync(a => a.Id == id && a.UniversityId == uniId);

            if (app == null) return NotFound();

            // Calculate score
            var weights = await _db.ScoringWeights
                .FirstOrDefaultAsync(w => w.MasterProgramId == app.MasterProgramId)
                ?? new ScoringWeights();

            var scored = _scoring.RankApplications(
                new List<Application> { app }, weights,
                app.MasterProgram.MinGPA, app.MasterProgram.MinYears);

            var scoreResult = scored.FirstOrDefault();
            ViewBag.ScoreResult = scoreResult;

            return View(app);
        }
        // ── POST /Rep/GenerateSummary/5 ───────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateSummary(int id)
        {
            var rep = await _userManager.GetUserAsync(User);
            var uniId = rep!.UniversityId!.Value;

            var app = await _db.Applications
                .Include(a => a.Student)
                .Include(a => a.MasterProgram)
                .Include(a => a.Documents)
                .Include(a => a.Languages)
                .FirstOrDefaultAsync(a => a.Id == id && a.UniversityId == uniId);

            if (app == null) return NotFound();

            var weights = await _db.ScoringWeights
                .FirstOrDefaultAsync(w => w.MasterProgramId == app.MasterProgramId)
                ?? new ScoringWeights();

            var scored = _scoring.RankApplications(
                new List<Application> { app }, weights,
                app.MasterProgram.MinGPA, app.MasterProgram.MinYears);

            var scoreResult = scored.FirstOrDefault();

            var languages = app.Languages
                .Select(l => $"{l.Language} ({l.Level})")
                .ToList();

            var recCount = app.Documents
                .Count(d => d.Type == DocumentType.RecommendationLetter);

            var summary = await _claude.GenerateApplicationSummary(
                studentName: app.Student.FullName,
                gpa: app.GPA,
                studyYears: app.StudyYears,
                diplomaStatus: app.DiplomaStatus.ToString(),
                motherTongue: app.MotherTongue,
                languages: languages,
                documentCount: app.Documents.Count,
                recLetterCount: recCount,
                programName: app.MasterProgram.Name,
                totalScore: scoreResult?.TotalScore ?? 0
            );

            return Ok(new { summary });
        }

        // ── POST /Rep/Accept ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var rep = await _userManager.GetUserAsync(User);
            var uniId = rep!.UniversityId!.Value;

            var app = await _db.Applications
                .FirstOrDefaultAsync(a => a.Id == id && a.UniversityId == uniId);

            if (app == null) return NotFound();

            app.Status = ApplicationStatus.Accepted;
            app.ReviewedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Application accepted.";
            return RedirectToAction(nameof(ApplicationDetail), new { id });
        }

        // ── POST /Rep/Reject ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? rejectionReason)
        {
            var rep = await _userManager.GetUserAsync(User);
            var uniId = rep!.UniversityId!.Value;

            var app = await _db.Applications
                .FirstOrDefaultAsync(a => a.Id == id && a.UniversityId == uniId);

            if (app == null) return NotFound();

            app.Status = ApplicationStatus.Rejected;
            app.RejectionReason = rejectionReason;
            app.ReviewedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Application rejected.";
            return RedirectToAction(nameof(ApplicationDetail), new { id });
        }

        // ── POST /Rep/SaveNotes ───────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNotes(int id, string? repNotes)
        {
            var rep = await _userManager.GetUserAsync(User);
            var uniId = rep!.UniversityId!.Value;

            var app = await _db.Applications
                .FirstOrDefaultAsync(a => a.Id == id && a.UniversityId == uniId);

            if (app == null) return NotFound();

            app.RepNotes = repNotes;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Notes saved.";
            return RedirectToAction(nameof(ApplicationDetail), new { id });
        }
    }
}