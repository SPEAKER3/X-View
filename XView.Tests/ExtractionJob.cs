using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace XView.Core
{
    public class ExtractionJob
    {
        public Guid JobId { get; set; } = Guid.NewGuid();
        public string PromptText { get; set; }
        public string Status { get; set; } = "Pending";
        public Document TargetDocument { get; set; }

        public async Task<string> ExecuteWithLLMAsync()
        {
            TargetDocument.Validate(); 

            int maxRetries = 3; 
            int currentTry = 0;
            string extractedData = null;

            while (currentTry < maxRetries)
            {
                currentTry++;
                try
                {
                    Status = $"Processing (Attempt {currentTry}/{maxRetries})";
                    Console.WriteLine(Status);
                    
                    extractedData = await MockLLMCallAsync(PromptText);
                    
                    if (string.IsNullOrEmpty(extractedData))
                    {
                        throw new Exception("LLM API повернула порожню відповідь.");
                    }

                    Status = "Completed";
                    break; 
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Помилка мережі на спробі {currentTry}: {ex.Message}");
                    
                    if (currentTry == maxRetries)
                    {
                        Status = "Failed";
                        throw new TimeoutException("Усі 3 спроби підключення до LLM API завершилися невдачею.", ex);
                    }
                    
                    await Task.Delay(2000); 
                }
            }

            return extractedData;
        }

        private async Task<string> MockLLMCallAsync(string prompt)
        {
            var rnd = new Random();
            if (rnd.Next(0, 10) < 5) 
                throw new HttpRequestException("504 Gateway Timeout");

            await Task.Delay(500); 
            return "[{\"Name\":\"John Doe\", \"Role\":\"Analyst\"}, {\"Name\":\"Jane Smith\", \"Role\":\"Developer\"}]";
        }
    }
}