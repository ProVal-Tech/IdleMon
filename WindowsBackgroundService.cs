namespace IdleMon {
    public class WindowsBackgroundService : BackgroundService {
        private readonly IdleService _idleService;
        private readonly ILogger<WindowsBackgroundService> _logger;

        public WindowsBackgroundService(ILogger<WindowsBackgroundService> logger) {
            _idleService = new IdleService();
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                while (!stoppingToken.IsCancellationRequested) {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    _logger.LogInformation("{idleJson}", _idleService.WriteIdleFile());
                    await Task.Delay(1000, stoppingToken);
                }
                _idleService.RemoveIdleFile();
            } catch (TaskCanceledException) {
                _idleService.RemoveIdleFile();
            } catch (Exception ex) {
                _logger.LogError(ex, "{Message}", ex.Message);
                Environment.Exit(1);
            }
        }
    }
}