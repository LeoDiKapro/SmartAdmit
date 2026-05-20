using System.Text;
using System.Text.Json;

namespace AdmissionsPortal.Services
{
    public class ClaudeService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public ClaudeService(IConfiguration config, HttpClient http)
        {
            _http = http;
            _apiKey = config["Anthropic:ApiKey"]!;
        }

        //  Core API call 
        private async Task<string> CallClaude(string prompt)
        {
            var request = new
            {
                model = "claude-haiku-4-5-20251001",
                max_tokens = 1024,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var response = await _http.PostAsync(
                "https://api.anthropic.com/v1/messages", content);

            var responseJson = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(responseJson);

            return doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? "No response.";
        }

        // Application Summary 
        public async Task<string> GenerateApplicationSummary(
            string studentName,
            decimal gpa,
            int studyYears,
            string diplomaStatus,
            string motherTongue,
            List<string> languages,
            int documentCount,
            int recLetterCount,
            string programName,
            decimal totalScore)
        {
            var prompt = $"""
                You are an admissions officer assistant. Write a concise 3-4 sentence 
                professional summary of this Master's program applicant. Be objective and factual.

                Applicant: {studentName}
                Applying for: {programName}
                GPA: {gpa:F2} / 4.0
                Years of Study: {studyYears}
                Diploma Status: {diplomaStatus}
                Mother Tongue: {motherTongue}
                Languages: {string.Join(", ", languages)}
                Documents submitted: {documentCount}
                Recommendation Letters: {recLetterCount}
                Overall Score: {totalScore:F2} / 10

                Write only the summary, no headers or bullet points.
                """;

            return await CallClaude(prompt);
        }

        // Program Recommender 
        public async Task<string> Chat(
        List<(string Role, string Content)> history,
        string systemPrompt)
        {
            var messages = history.Select(h => new
            {
                role = h.Role,
                content = h.Content
            }).ToArray();

            var request = new
            {
                model = "claude-haiku-4-5-20251001",
                max_tokens = 1024,
                system = systemPrompt,
                messages
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

            var response = await _http.PostAsync("https://api.anthropic.com/v1/messages", content);
            var responseJson = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(responseJson);

            return doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? "No response.";
        }
    }
}