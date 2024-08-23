using Newtonsoft.Json;

namespace BackupAppinfotecs.SERVICE.Services
{
    public class ConfigService
    {
        public string[] SourceDirectories { get; set; }
        public string TargetDirectory { get; set; }
        public string LogLevel { get; set; }


        // Паттерн Singleton
        private static ConfigService _instance;
        private static readonly object _lock = new object();
        
        // Конструктор для автоматической десериализации JSON
        [JsonConstructor]
        public ConfigService(string[] sourceDirectories, string targetDirectory, string logLevel)
        {
            SourceDirectories = sourceDirectories ?? throw new ArgumentNullException(nameof(sourceDirectories));
            TargetDirectory = targetDirectory ?? throw new ArgumentNullException(nameof(targetDirectory));
            LogLevel = logLevel ?? throw new ArgumentNullException(nameof(logLevel));
        }
        // Приватный конструктор для загрузки конфигурации из файла
        private ConfigService(string configFilePath)
        {
            Console.Write(configFilePath);
            if (string.IsNullOrEmpty(configFilePath ))
            {
                throw new ArgumentNullException(nameof(configFilePath), "Путь к файлу конфигурации не может быть пустым или null.");
            }

            if (!File.Exists(configFilePath))
            {
                throw new FileNotFoundException($"Файл не найден: {configFilePath}");
            }

            var json = File.ReadAllText(configFilePath);
            // Десериализация JSON напрямую в объект ConfigService
            var config = JsonConvert.DeserializeObject<ConfigService>(json);
            SourceDirectories = config.SourceDirectories;
            TargetDirectory = config.TargetDirectory;
            LogLevel = config.LogLevel;
        }
        // Метод для получения экземпляра класса
        public static ConfigService GetInstance(string configFilePath)
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ConfigService( configFilePath);
                    }
                }
            }
            return _instance ;
        }
    }
}



