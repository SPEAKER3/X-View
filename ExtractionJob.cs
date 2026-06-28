using System;
using System.Threading.Tasks;
using System.Net.Http;
namespace XView.Core
{
/// <summary>
    /// Відповідає за процес обробки документа через зовнішнє LLM API.
    /// Керує статусом виконання та логікою повторних спроб (retry logic).
    /// </summary>
    
    public enum JobStatus { Pending, Processing, Completed, Failed }
    public class ExtractionJob
    {
        public Guid JobId { get; set; } = Guid.NewGuid();
        public string PromptText { get; set; } = string.Empty;
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public Document TargetDocument { get; set; }
        private static readonly Random _rnd = new Random();

        /// <summary>
        /// Запускає процес екстракції даних із використанням механізму повторних спроб у разі збою мережі.
        /// </summary>
        /// <returns>Рядок із витягнутими даними у форматі JSON.</returns>
        /// <exception cref="TimeoutException">Викидається, якщо всі спроби підключення були невдалими.</exception>
        public async Task<string> ExecuteWithLLMAsync()
        {
            // Перед початком роботи обов'язково валідуємо документ
            TargetDocument.Validate(); 

            int maxRetries = 3; // Максимальна кількість спроб звернення до API
            int currentTry = 0;
            string extractedData = null;

            // Цикл повторних спроб для забезпечення надійності системи (NFR-R-02)
            while (currentTry < maxRetries)
            {
                currentTry++;
                try
                {
                    Status = $"Processing (Attempt {currentTry}/{maxRetries})";
                    Console.WriteLine(Status);
                    
                    // Симуляція мережевого запиту до зовнішнього API (наприклад, OpenAI)
                    extractedData = await MockLLMCallAsync(PromptText);
                    
                    // Перевірка на порожню відповідь від ШІ
                    if (string.IsNullOrEmpty(extractedData))
                    {
                        throw new Exception("LLM API повернула порожню відповідь.");
                    }

                    Status = JobStatus.Completed;;
                    break; // Успішне виконання — достроковий вихід із циклу
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Помилка мережі на спробі {currentTry}: {ex.Message}");
                    
                    // Якщо це була остання спроба, перериваємо процес та викидаємо помилку
                    if (currentTry == maxRetries)
                    {
                        Status = JobStatus.Failed;
                        throw new TimeoutException("Усі 3 спроби підключення до LLM API завершилися невдачею.", ex);
                    }
                    
                    // Затримка 2 секунди перед наступною спробою
                    await Task.Delay(2000); 
                }
            }

            return extractedData;
        }
        private async Task<string> MockLLMCallAsync(string prompt)
        {
            if (_rnd.Next(0, 10) < 5) 
                throw new HttpRequestException("504 Gateway Timeout");

            await Task.Delay(500);
            return "[{\"Name\":\"John Doe\", \"Role\":\"Analyst\"}, {\"Name\":\"Jane Smith\", \"Role\":\"Developer\"}]";
        }
    }
}