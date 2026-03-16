using Fermetta.Data;
using Fermetta.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Fermetta.Services
{
    public class ProductAssistantService : IProductAssistantService
    {
        private readonly ApplicationDbContext _db;
        private readonly HttpClient _http;
        private readonly string _apiKey;

        private const string UnknownAnswer = "At the moment we don't have details about this.";

        public ProductAssistantService(ApplicationDbContext db, IConfiguration config)
        {
            _db = db;
            _apiKey = config["OpenAI:ApiKey"] ?? "";

            _http = new HttpClient { BaseAddress = new Uri("https://api.openai.com/v1/") };
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> AskAsync(int productId, string question)
        {
            question = (question ?? "").Trim();
            if (question.Length < 2) return "Please write a longer question.";

            // 1) produs + description
            var product = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Product_Id == productId);

            if (product == null) return UnknownAnswer;

            // 2) FAQ (top)
            var faqs = await _db.ProductFaqs
                .AsNoTracking()
                .Where(f => f.Product_Id == productId)
                .OrderByDescending(f => f.AskedCount)
                .Take(10)
                .ToListAsync();

            // 3) context strict
            var contextText =
$@"PRODUCT:
Name: {product.Name}
Category: {product.Category?.Name ?? "-"}
Description: {product.Description ?? "-"}

FAQ:
{string.Join("\n", faqs.Select(f => $"- Q: {f.Question}\n  A: {(string.IsNullOrWhiteSpace(f.Answer) ? "[no info]" : f.Answer)}"))}
";

            var systemPrompt =
$@"You are Product Assistant for an online store.
Answer ONLY using the information from the CONTEXT.
If the answer is not in the context, reply exactly: ""{UnknownAnswer}"".
Keep it short and clear. Do not invent details.";

            var body = new
            {
                model = "gpt-4o-mini",
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = $"CONTEXT:\n{contextText}\n\nQUESTION:\n{question}" }
                },
                temperature = 0.2,
                max_tokens = 120
            };

            // dacă nu ai cheie pusă încă, nu crăpăm aplicația
            if (string.IsNullOrWhiteSpace(_apiKey))
                return UnknownAnswer;

            var resp = await _http.PostAsync(
                "chat/completions",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

            if (!resp.IsSuccessStatusCode)
                return UnknownAnswer;

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var answer = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            answer = (answer ?? "").Trim();
            if (string.IsNullOrWhiteSpace(answer)) answer = UnknownAnswer;

            return answer;
        }
    }
}
