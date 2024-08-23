using BackupAppinfotecs.SERVICE.Services.interfaces;

namespace BackupAppinfotecs.SERVICE.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly string logFilePath;
        public LoggingService()
        {
            logFilePath = "default_log.txt";
        }
        public LoggingService(string logLevel)
        {
            logFilePath = $"log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            LogInfo($"Ведение журнала началось с уровня: {logLevel}");
        }
        public virtual void LogInfo(string message)
        {
            Log(message, "INFO");
        }

        public virtual void LogError(string message)
        {
            Log(message, "ERROR");
        }
        private void Log(string message, string level)
        {
            string logFilePath = Path.Combine("LOGGBACK", $"{DateTime.Now:yyyyMMdd}.log");
            Directory.CreateDirectory("LOGGBACK");
            string timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                //Формируем сообщение
                string logoMessage = $"[{timestamp}] [{level}] {message}";
                writer.WriteLine(logoMessage);
                Console.WriteLine(logoMessage);
            }
        }
    }
}
