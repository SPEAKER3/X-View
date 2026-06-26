/// <summary>
    /// Представляє завантажений користувачем документ для подальшої обробки.
    /// Відповідає за зберігання метаданих файлу та їх базову валідацію.
    /// </summary>
    public class Document
    {
        public Guid DocId { get; set; } = Guid.NewGuid();
        public string FileName { get; set; }
        public double FileSizeMB { get; set; }
        public string Format { get; set; }

        /// <summary>
        /// Перевіряє документ на відповідність бізнес-правилам (наявність імені, розмір, формат).
        /// </summary>
        /// <exception cref="ArgumentException">Викидається, якщо ім'я файлу порожнє.</exception>
        /// <exception cref="InvalidOperationException">Викидається, якщо розмір перевищує 50 МБ.</exception>
        /// <exception cref="NotSupportedException">Викидається, якщо формат не підтримується.</exception>
        public void Validate()
        {
            // Перевірка наявності імені файлу
            if (string.IsNullOrWhiteSpace(FileName))
            {
                throw new ArgumentException("Ім'я файлу не може бути порожнім.");
            }

            // Перевірка ліміту розміру файлу (граничне значення: 50.0 МБ)
            if (FileSizeMB > 50.0)
            {
                throw new InvalidOperationException($"Файл '{FileName}' завеликий ({FileSizeMB} МБ). Максимальний розмір — 50 МБ.");
            }

            // Перевірка допустимих розширень файлів (класи еквівалентності)
            if (Format != ".pdf" && Format != ".txt")
            {
                throw new NotSupportedException($"Формат {Format} не підтримується. Дозволені лише .pdf та .txt.");
            }
        }
    }
    