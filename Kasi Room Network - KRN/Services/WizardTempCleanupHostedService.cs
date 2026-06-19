namespace Kasi_Room_Network___KRN.Services
{
    public class WizardTempCleanupHostedService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public WizardTempCleanupHostedService(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();

                var photoService =
                    scope.ServiceProvider
                        .GetRequiredService<IPhotoStorageService>();

                photoService.CleanupExpiredTemporaryPhotos(
                    TimeSpan.FromHours(24));

                await Task.Delay(
                    TimeSpan.FromHours(1),
                    stoppingToken);
            }
        }
    }
}
