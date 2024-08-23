using BackupAppinfotecs.SERVICE.Services.interfaces;

namespace BackupAppinfotecs.SERVICE.Services
{
    public class BackUpService
    {
        private readonly ILoggingService _loggingService;
        private readonly ConfigService _configService;
        public BackUpService(ILoggingService loggingService, ConfigService configService)
        {
            _loggingService = loggingService;
            _configService = configService;
        }
        public void PrepareSourceDirectories()
        {
            for (int dirIndex = 0; dirIndex < _configService.SourceDirectories.Length; dirIndex++)
            {
                var sourceDirectory = _configService.SourceDirectories[dirIndex];

                try
                {
                    // Если директория не существует, создаем ее
                    if (!Directory.Exists(sourceDirectory))
                    {
                        Directory.CreateDirectory(sourceDirectory);
                        // Создаем тестовые файлы в новой директории
                        for (int i = 1; i <= 5; i++)
                        {
                            // Генерируем уникальное имя файла, добавляя индекс директории
                            var filePath = Path.Combine(sourceDirectory, $"TestFile_{i}_Dir{dirIndex + 1}.txt");
                            File.WriteAllText(filePath, $"Это тестовый файл номер {i} в папке {sourceDirectory}.");
                            _loggingService.LogInfo($"Создан файл: {filePath}");
                           
                        }
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.LogError($"Ошибка при создании директории {sourceDirectory}: {ex.Message}");
                }
            }
        }
        public void StartBackUp()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string targetPath = Path.Combine(_configService.TargetDirectory, $"Backup_{timestamp}");
            Directory.CreateDirectory(targetPath);
            _loggingService.LogInfo($"Начато резеврное копирование. Целевой каталог: {targetPath}");

            foreach (var sourceDirectory in _configService.SourceDirectories)
            {
                try
                {
                    if (Directory.Exists(sourceDirectory))
                    {
                        var files = Directory.GetFiles(sourceDirectory);
                        foreach (var file in files)
                        {
                            var destFile = Path.Combine(targetPath, Path.GetFileName(file));
                            File.Copy(file, destFile);
                            _loggingService.LogInfo($"Скопирован файл: {file} в {destFile}");
                        }
                    }
                    else
                    {
                        _loggingService.LogError($"Исходная директория не найдена: {sourceDirectory}");
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.LogError($"Ошибка при копировании из {sourceDirectory}: {ex.Message}");
                }
            }
        }
    }

}


