using Takap.RpcIpc.Samples;

namespace Takap
{
    public class MyService : BackgroundService
    {
        // Injections
        readonly ILogger<MyService> _logger;
        readonly ServerSample _server;

        public MyService(ILogger<MyService> logger, ServerSample server)
        {
            _logger = logger;
            _server = server;
        }

        // サービスが開始されたときの処理
        public override async Task StartAsync(CancellationToken ct)
        {
            _logger.LogInformation("Start Service");
            await base.StartAsync(ct);
        }

        // サービスが終了したときの処理
        public override async Task StopAsync(CancellationToken ct)
        {
            _logger.LogInformation("Stop Service");
            await base.StopAsync(ct);
        }

        // サービスが実行中の処理
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
