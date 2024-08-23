


using BackupAppinfotecs.SERVICE.Services;

var configService = ConfigService.GetInstance("BackupAppSettings.json");
var loggingService = new LoggingService(configService.LogLevel);
var backupService = new BackUpService(loggingService, configService);

try
{
    PrintHeader("C# Резервное копирование - Балыкин");
    backupService.PrepareSourceDirectories();
    backupService.StartBackUp();
}
catch (Exception ex)
{
    loggingService.LogError($"Неизвестная ошибка :{ex.Message}");
}
static void PrintHeader(string title)
{
    // Определяем ширину рамки
    int width = title.Length + 4;

    // Создаем верхнюю и нижнюю границы рамки
    Console.WriteLine(new string('—', width));
    Console.WriteLine($"| {title} |");
    Console.WriteLine(new string('—', width));
}