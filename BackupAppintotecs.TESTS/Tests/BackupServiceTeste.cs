using BackupAppinfotecs.SERVICE.Services;
using BackupAppinfotecs.SERVICE.Services.interfaces;
using Moq;
using NUnit.Framework;

namespace BackupAppintotecs.TESTS.Tests
{
    [TestFixture]
    public class BackupServiceTeste
    {
        private Mock<ILoggingService> _loggingServiceMock;
        private string testSourceDir;
        private string testTargetDir;
        private string testLogFile;
        private ConfigService? configService;
        private ILoggingService loggingService;
        private BackUpService backupService;

        private string logDirectory;
        private string logFilePath ; 
        [SetUp]
        public void Setup()
        {
            _loggingServiceMock = new Mock<ILoggingService>();
            loggingService = _loggingServiceMock.Object;
            // Создаем временные директории для тестов на диске C
            testSourceDir = Path.Combine(@"C:\", "TestSource");
            testTargetDir = Path.Combine(@"C:\", "TestBackup");

            // Создаем директории, если они не существуют
            Directory.CreateDirectory(testSourceDir);
            Directory.CreateDirectory(testTargetDir);

            // Создаем тестовый файл
            File.WriteAllText(Path.Combine(testSourceDir, "testfile.txt"), "Test content");

            var configJson = $@"
            {{
                ""SourceDirectories"": [""{testSourceDir.Replace(@"\", @"\\")}"", ""{testSourceDir.Replace(@"\", @"\\")}, SubFolder""],
                ""TargetDirectory"": ""{testTargetDir.Replace(@"\", @"\\")}"",
                ""LogLevel"": ""Info""
            }}";

            File.WriteAllText("BackupAppSettings.json", configJson);
            configService = ConfigService.GetInstance("BackupAppSettings.json");

            loggingService = new LoggingService("Info");
            backupService = new BackUpService(_loggingServiceMock.Object, configService);


            // Установка пути к директории логов и файлу
            logDirectory = "LOGGBACK";
            logFilePath = Path.Combine(logDirectory, $"{DateTime.Now:yyyyMMdd}.log");
            // Удаление файлов для чистоты теста
            if (Directory.Exists(logDirectory))
            {
                Directory.Delete(logDirectory, true);
            }
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (Directory.Exists(logDirectory))
                {
                    Directory.Delete(logDirectory, true);
                }

                if (Directory.Exists(testSourceDir))
                {
                    Directory.Delete(testSourceDir, true);
                }

                if (Directory.Exists(testTargetDir))
                {
                    Directory.Delete(testTargetDir, true);
                }

                // Удаляем конфигурацию, если необходимо
                if (File.Exists("BackupAppSettings.json"))
                {
                    File.Delete("BackupAppSettings.json");
                }
            }
            catch (IOException ex)
            {

                Assert.Fail($"Ошибка при удалении директории: {ex.Message}");

            }
            catch (UnauthorizedAccessException access)
            {
                Assert.Fail($"Нет прав доступа: {access.Message}");
            }
        }

        [Test]
        public void PrepareSourceDirectories_CreatesDirectoriesAndFiles()
        {
            // Arrange
            var sourceDir1 = configService.SourceDirectories[0];
            var sourceDir2 = configService.SourceDirectories[1];
            // Удаляем директории, если они существуют (для теста)
            if (Directory.Exists(sourceDir1))
                Directory.Delete(sourceDir1, true);
            if (Directory.Exists(sourceDir2))
            {
                Directory.Delete(sourceDir2, true);
            }
            // Act
            backupService.PrepareSourceDirectories();
            // Assert
            Assert.IsTrue(Directory.Exists(sourceDir1), "Директория не была создана.");
            for (int i = 1; i <= 5; i++)
            {
                var filePath = Path.Combine(sourceDir1, $"TestFile_{i}_Dir1.txt");
                Assert.IsTrue(File.Exists(filePath), $"Файл {filePath} не был создан.");

                // Дополнительно проверим содержимое файла
                var content = File.ReadAllText(filePath);
                Assert.AreEqual($"Это тестовый файл номер {i} в папке {sourceDir1}.", content);
            }
            for (int i = 1; i <= 5; i++)
            {
                var filePath = Path.Combine(sourceDir2, $"TestFile_{i}_Dir2.txt");
                Assert.IsTrue(File.Exists(filePath), $"Файл {filePath} не был создан.");

                // Дополнительно проверим содержимое файла
                var content = File.ReadAllText(filePath);
                Assert.AreEqual($"Это тестовый файл номер {i} в папке {sourceDir2}.", content);
            }


            _loggingServiceMock.Verify(l => l.LogInfo(It.IsAny<string>()), Times.Exactly(10), "Логирование файлов не сработало.");
        }

        [Test]
        public void StartBackUp_CopiesFilesToTargetDirectory()
        {
            // Arrange
            backupService.PrepareSourceDirectories(); // Создадим директории и файлы для теста
            string targetDir = configService.TargetDirectory;

            // Удаляем целевую директорию, если она существует (для теста)
            if (Directory.Exists(targetDir))
                Directory.Delete(targetDir, true);

            // Act
            backupService.StartBackUp();

            // Assert
            // Определяем поддиректорию, созданную с временной меткой
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupSubDir = Path.Combine(targetDir, $"Backup_{timestamp}");

            Assert.IsTrue(Directory.Exists(backupSubDir), "Целевая поддиректория не была создана.");

            // Проверим, что файлы скопированы
            foreach (var sourceDirectory in configService.SourceDirectories)
            {
                var files = Directory.GetFiles(sourceDirectory);

                foreach (var file in files)
                {
                    // Используем уникальное имя файла
                    var fileName = Path.GetFileName(file);
                    var destFile = Path.Combine(backupSubDir, fileName);

                    Assert.IsTrue(File.Exists(destFile), $"Файл {destFile} не был скопирован." );

                    // Проверим, что содержимое совпадает
                    var sourceContent = File.ReadAllText(file);
                    var destContent = File.ReadAllText(destFile);
                    Assert.AreEqual(sourceContent, destContent, "Содержимое скопированного файла не совпадает с оригиналом.");

                }
            }
            _loggingServiceMock
        .Setup(l => l.LogInfo(It.IsAny<string>()))
        .Verifiable("LogInfo был вызван");
            _loggingServiceMock.Verify(l => l.LogInfo(It.IsAny<string>()), Times.Exactly(7), "LogInfo не был вызван 7 раза.");


        }
        [Test]
        public void Log_CreatesLogFileAndWritesMessage()
        {
            // Arrange 
            var logger = new LoggingService();
            string message = "Тестовое сообщение";

            // Act
            logger.LogInfo(message); // Вызов метода логирования

            // Assert: Проверка, был ли создан файл лога
            Assert.IsTrue(File.Exists(logFilePath), "Файл лога не был создан.");

            // Проверяем содержимое файла
            string[] lines = File.ReadAllLines(logFilePath);
            Assert.IsNotEmpty(lines, "Лог-файл пуст.");

            // Получаем последнюю запись лога
            string lastLine = lines[^1];

            // Проверяем, что строка содержит уровень и сообщение. (без учета времени)
            StringAssert.Contains("[INFO]", lastLine);
            StringAssert.Contains(message, lastLine);

            // Проверяем, правильный ли формат времени в записи (только проверка времени)
            StringAssert.IsMatch(@"\[\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}:\d{2}\]", lastLine);
        }

    }
}




