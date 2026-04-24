using AdmissionsPortal.Models;

namespace AdmissionsPortal.Services
{
    public class ScoreResult
    {
        public int ApplicationId { get; set; }
        public Application Application { get; set; } = null!;

        // Component scores
        public decimal GPAScore { get; set; }
        public decimal YearsScore { get; set; }
        public decimal LanguageScore { get; set; }

        // Bonuses
        public decimal LanguageBonus { get; set; }
        public decimal DiplomaBonus { get; set; }
        public decimal RecommendationBonus { get; set; }
        public decimal DocumentBonus { get; set; }

        // Final
        public decimal TotalScore { get; set; }
        public int Rank { get; set; }
        public decimal Percentile { get; set; }

        // Breakdown string for display
        public string Breakdown => string.Join(" + ", new[]
        {
            $"GPA ({GPAScore:F2})",
            $"Years ({YearsScore:F2})",
            $"Language ({LanguageScore:F2})",
            LanguageBonus > 0    ? $"Lang Bonus (+{LanguageBonus:F2})"    : null,
            DiplomaBonus > 0     ? $"Diploma Bonus (+{DiplomaBonus:F2})"  : null,
            RecommendationBonus > 0 ? $"Rec Letters (+{RecommendationBonus:F2})" : null,
            DocumentBonus > 0    ? $"Docs Bonus (+{DocumentBonus:F2})"    : null,
        }.Where(s => s != null));
    }

    public class ScoringService
    {
        // Language level scale
        private static readonly Dictionary<string, int> LanguageLevelScale = new()
        {
            { "A1", 0 }, { "A2", 1 }, { "B1", 2 },
            { "B2", 3 }, { "C1", 4 }, { "C2", 5 }, { "Native", 6 }
        };

        public List<ScoreResult> RankApplications(
            List<Application> applications,
            ScoringWeights weights,
            decimal programMinGPA,
            int programMinYears)
        {
            var results = applications
                .Select(app => CalculateScore(app, weights, programMinGPA, programMinYears))
                .OrderByDescending(r => r.TotalScore)
                .ToList();

            // Assign rank and percentile
            for (int i = 0; i < results.Count; i++)
            {
                results[i].Rank = i + 1;
                results[i].Percentile = results.Count == 1
                    ? 100
                    : Math.Round(100m - ((decimal)i / (results.Count - 1) * 100), 1);
            }

            return results;
        }

        private ScoreResult CalculateScore(
            Application app,
            ScoringWeights weights,
            decimal programMinGPA,
            int programMinYears)
        {
            var result = new ScoreResult
            {
                ApplicationId = app.Id,
                Application = app
            };

            // ── GPA Score ─────────────────────────────────────────────────────
            // Normalise GPA to 0-10 scale relative to program max (4.0)
            result.GPAScore = (app.GPA / 4.0m) * 10 * (weights.GPAWeight / 100m);

            // ── Years Score ───────────────────────────────────────────────────
            // Normalise years: more years = higher score, capped at 6
            var normYears = Math.Min(app.StudyYears / 6.0m, 1.0m);
            result.YearsScore = normYears * 10 * (weights.YearsWeight / 100m);

            // ── Language Score ────────────────────────────────────────────────
            // Take the best language level the applicant has
            var bestLevel = app.Languages
                .Select(l => LanguageLevelScale.GetValueOrDefault(l.Level, 0))
                .DefaultIfEmpty(0)
                .Max();
            var normLanguage = bestLevel / 6.0m;   // 6 = Native (max)
            result.LanguageScore = normLanguage * 10 * (weights.LanguageWeight / 100m);

            // ── Bonuses ───────────────────────────────────────────────────────
            // Extra languages beyond the first
            var extraLanguages = Math.Max(0, app.Languages.Count - 1);
            result.LanguageBonus = extraLanguages * weights.LanguageBonus;

            // Completed diploma
            result.DiplomaBonus = app.DiplomaStatus == DiplomaStatus.Completed
                ? weights.DiplomaBonus : 0;

            // Recommendation letters
            var recCount = app.Documents
                .Count(d => d.Type == DocumentType.RecommendationLetter);
            result.RecommendationBonus = recCount * weights.RecommendationBonus;

            // Document completeness — submitted transcript + diploma + at least 1 extra
            var hasTranscript = app.Documents.Any(d => d.Type == DocumentType.Transcript);
            var hasDiploma = app.Documents.Any(d => d.Type == DocumentType.Diploma);
            var hasExtra = app.Documents.Any(d =>
                d.Type != DocumentType.Transcript && d.Type != DocumentType.Diploma);
            result.DocumentBonus = (hasTranscript && hasDiploma && hasExtra)
                ? weights.DocumentBonus : 0;

            // ── Total ─────────────────────────────────────────────────────────
            result.TotalScore = Math.Round(
                result.GPAScore +
                result.YearsScore +
                result.LanguageScore +
                result.LanguageBonus +
                result.DiplomaBonus +
                result.RecommendationBonus +
                result.DocumentBonus, 2);

            return result;
        }
    }
}