namespace CleanDirectory
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var directoryPath = _configuration["FileCleanupSettings:DirectoryPath"];
                var retentionDays = int.Parse(_configuration["FileCleanupSettings:RetentionDays"]);

                try
                {
                    // Perform file cleanup logic here
                    CleanUpOldFiles(directoryPath, retentionDays);

                    _logger.LogInformation("File cleanup completed successfully.");
                }
                catch (Exception ex)
                {
                    // Log any exceptions
                    _logger.LogError(ex, "Error during file cleanup: {ErrorMessage}", ex.Message);
                }

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken); // Run every day
            }
        }

        private void CleanUpOldFiles(string directoryPath, int retentionDays)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(directoryPath);

                foreach (var file in directoryInfo.GetFiles())
                {
                    if (DateTime.UtcNow - file.LastWriteTimeUtc > TimeSpan.FromDays(retentionDays))
                    {
                        file.Delete();
                        _logger.LogInformation("Deleted file: {FileName}", file.FullName);
                    }
                }

                foreach (var subDirectory in directoryInfo.GetDirectories())
                {
                    if (DateTime.UtcNow - subDirectory.LastWriteTimeUtc > TimeSpan.FromDays(retentionDays))
                    {
                        subDirectory.Delete(true);
                        _logger.LogInformation("Deleted directory: {DirectoryName}", subDirectory.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions
                _logger.LogError(ex, "Error during cleanup: {ErrorMessage}", ex.Message);
            }
        }
    }
}