using System;
using System.Threading.Tasks;
using Xunit;
using XView.Core;
using System.Diagnostics.CodeAnalysis;

namespace XView.Tests
{
    [ExcludeFromCodeCoverage]
    public class DocumentTests
    {
        // TC-01: Техніка EP (Негативний) - Порожнє ім'я файлу
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_EmptyFileName_ShouldThrowArgumentException(string badName)
        {
            // Arrange
            var doc = new Document { FileName = badName, FileSizeMB = 10.0, Format = ".pdf" };

            // Act
            Action act = () => doc.Validate();

            // Assert
            Assert.Throws<ArgumentException>(act);
        }

        // TC-02: Техніка BVA (Позитивний) - Максимально допустимий розмір
        [Theory]
        [InlineData(49.9)] // Трохи менше межі
        [InlineData(50.0)] // Точна межа
        public void Validate_ValidFileSize_ShouldNotThrowException(double fileSize)
        {
            // Arrange
            var doc = new Document { FileName = "report.pdf", FileSizeMB = fileSize, Format = ".pdf" };

            // Act
            var exception = Record.Exception(() => doc.Validate());

            // Assert
            Assert.Null(exception); 
        }

        // TC-03: Техніка BVA (Негативний) - Перевищення ліміту розміру
        [Fact]
        public void Validate_FileSizeExceedsLimit_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var doc = new Document { FileName = "big.pdf", FileSizeMB = 50.1, Format = ".pdf" };

            // Act
            Action act = () => doc.Validate();

            // Assert
            var ex = Assert.Throws<InvalidOperationException>(act);
            Assert.Contains("завеликий", ex.Message);
        }

        // TC-04: Техніка EP (Позитивний) - Допустимі формати файлів
        [Theory]
        [InlineData(".pdf")]
        [InlineData(".txt")]
        public void Validate_ValidFormat_ShouldNotThrowException(string validFormat)
        {
            // Arrange
            var doc = new Document { FileName = "doc" + validFormat, FileSizeMB = 5.0, Format = validFormat };

            // Act
            var exception = Record.Exception(() => doc.Validate());

            // Assert
            Assert.Null(exception);
        }

        // TC-05: Техніка EP (Негативний) - Недопустимі формати файлів
        [Theory]
        [InlineData(".docx")]
        [InlineData(".jpg")]
        public void Validate_InvalidFormat_ShouldThrowNotSupportedException(string invalidFormat)
        {
            // Arrange
            var doc = new Document { FileName = "image" + invalidFormat, FileSizeMB = 10.0, Format = invalidFormat };

            // Act
            Action act = () => doc.Validate();

            // Assert
            Assert.Throws<NotSupportedException>(act);
        }
    }

    [ExcludeFromCodeCoverage]
    public class ExtractionJobTests
    {
        // TC-06: Техніка EP (Негативний) - Валідація документа блокує виконання Job-а
        [Fact]
        public async Task ExecuteWithLLMAsync_DocumentIsInvalid_ThrowsExceptionBeforeExecution()
        {
            // Arrange
            var badDoc = new Document { FileName = "", FileSizeMB = 100.0, Format = ".exe" };
            var job = new ExtractionJob { TargetDocument = badDoc, PromptText = "Find names" };

            // Act
            Func<Task> act = async () => await job.ExecuteWithLLMAsync();

            // Assert
            // Має викинути ArgumentException через порожнє ім'я, ще до початку HTTP запитів
            await Assert.ThrowsAsync<ArgumentException>(act); 
        }

        // TC-07: Техніка EP (Негативний) - Документ взагалі не передано (NullReference)
        [Fact]
        public async Task ExecuteWithLLMAsync_DocumentIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            var job = new ExtractionJob { TargetDocument = null, PromptText = "Find names" };

            // Act
            Func<Task> act = async () => await job.ExecuteWithLLMAsync();

            // Assert
            await Assert.ThrowsAsync<NullReferenceException>(act);
        }
    }

    [ExcludeFromCodeCoverage]
    public class ExportFileTests
    {
        // TC-08: Техніка EP (Негативний) - Порожні вхідні дані для CSV
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GenerateCSV_EmptyData_ShouldThrowArgumentNullException(string emptyData)
        {
            // Arrange
            var export = new ExportFile();

            // Act
            Action act = () => export.GenerateCSV(emptyData);

            // Assert
            Assert.Throws<ArgumentNullException>(act);
        }

        // TC-09: Техніка EP (Негативний) - Невалідний синтаксис JSON
        [Fact]
        public void GenerateCSV_InvalidJsonSyntax_ShouldThrowFormatException()
        {
            // Arrange
            var export = new ExportFile();
            string badJson = "[{ Name: John"; // Зламаний синтаксис (без закриваючих дужок і лапок)

            // Act
            Action act = () => export.GenerateCSV(badJson);

            // Assert
            Assert.Throws<FormatException>(act);
        }

        // TC-10: Техніка BVA (Позитивний) - Порожній масив JSON (0 елементів)
        [Fact]
        public void GenerateCSV_EmptyJsonArray_ShouldReturnEmptyString()
        {
            // Arrange
            var export = new ExportFile();
            string emptyJsonArray = "[]";

            // Act
            string result = export.GenerateCSV(emptyJsonArray);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        // TC-11: Техніка EP (Позитивний) - Стандартний масив даних створює правильний CSV
        [Fact]
        public void GenerateCSV_ValidJsonArray_ShouldReturnCorrectCsvString()
        {
            // Arrange
            var export = new ExportFile();
            string jsonInput = "[{\"Name\":\"John\",\"Role\":\"Admin\"},{\"Name\":\"Jane\",\"Role\":\"Dev\"}]";
            string expectedHeader = "Name,Role";
            string expectedRow1 = "John,Admin";
            string expectedRow2 = "Jane,Dev";

            // Act
            string result = export.GenerateCSV(jsonInput);

            // Assert
            Assert.Contains(expectedHeader, result);
            Assert.Contains(expectedRow1, result);
            Assert.Contains(expectedRow2, result);
        }

        // TC-12: Техніка EP (Позитивний) - Екранування ком всередині значень
        [Fact]
        public void GenerateCSV_ValueWithComma_ShouldWrapInQuotes()
        {
            // Arrange
            var export = new ExportFile();
            string jsonInput = "[{\"City\":\"Kharkiv, UA\", \"Population\":\"1.4M\"}]";

            // Act
            string result = export.GenerateCSV(jsonInput);

            // Assert
            Assert.Contains("\"Kharkiv, UA\"", result); // Перевіряємо, що значення обгорнуте в лапки
        }
    }
}