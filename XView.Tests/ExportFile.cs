using System;
using System.Collections.Generic;
using System.Text.Json;
namespace XView.Core
{
/// <summary>
    /// Відповідає за форматування та підготовку витягнутих даних до експорту.
    /// </summary>
    public class ExportFile
    {
        public Guid ExportId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Перетворює структуровані JSON-дані у формат CSV.
        /// </summary>
        /// <param name="jsonStructuredData">Дані у форматі JSON-масиву.</param>
        /// <returns>Текстовий рядок у форматі CSV.</returns>
        /// <exception cref="ArgumentNullException">Викидається, якщо вхідні дані відсутні.</exception>
        /// <exception cref="FormatException">Викидається при неможливості розпарсити JSON.</exception>
        public string GenerateCSV(string jsonStructuredData)
        {
            // Перевірка на порожні вхідні дані
            if (string.IsNullOrWhiteSpace(jsonStructuredData))
            {
                throw new ArgumentNullException(nameof(jsonStructuredData), "Дані для експорту відсутні.");
            }

            try
            {
                // Десеріалізація вхідного JSON-рядка у список словників (ключ-значення)
                var data = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonStructuredData);
                
                // Перевірка на порожній масив (уникнення помилок у циклах)
                if (data == null || data.Count == 0) return string.Empty;

                var csvLines = new List<string>();
                
                // Витягування ключів з першого об'єкта для формування заголовків колонок CSV
                var headers = new List<string>(data[0].Keys);
                csvLines.Add(string.Join(",", headers));

                // Зовнішній цикл: обробка кожного рядка (об'єкта) з масиву
                foreach (var row in data)
                {
                    var rowValues = new List<string>();
                    
                    // Вкладений цикл: обробка кожного значення за відповідним заголовком
                    foreach (var header in headers)
                    {
                        // Безпечне отримання значення (якщо ключа немає, ставимо порожній рядок)
                        string val = row.ContainsKey(header) ? row[header] : "";
                        
                        // Захист від пошкодження CSV: якщо текст містить кому, обгортаємо його в лапки
                        if (val.Contains(",")) val = $"\"{val}\"";
                        rowValues.Add(val);
                    }
                    // Формування рядка CSV через розділювач
                    csvLines.Add(string.Join(",", rowValues));
                }

                // Об'єднання всіх рядків символом перенесення рядка
                return string.Join("\n", csvLines);
            }
            catch (JsonException ex)
            {
                // Перехоплення помилок синтаксису JSON та прокидання зрозумілої бізнес-помилки
                throw new FormatException("Помилка парсингу даних. Відповідь LLM має невалідну JSON-структуру.", ex);
            }
        }
    }
}