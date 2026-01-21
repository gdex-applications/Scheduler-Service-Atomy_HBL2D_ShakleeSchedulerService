namespace Atomy_HBL2DBcode_ShakleeSchedulerService.Functions.HerbalifeProcess2DBarcode
{
    public class HerbalifeProcess2DBarcodeWorker : BackgroundService
    {
        private readonly ILogger<HerbalifeProcess2DBarcodeWorker> _logger;
        private readonly IConfiguration _configuration;
        public HerbalifeProcess2DBarcodeWorker(ILogger<HerbalifeProcess2DBarcodeWorker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _ = new HerbalifeProcess2DBarcodeScheduler(_configuration);

        }
        /// <summary>
        /// IMPORTANT!!!
        /// PLEASE UPDATE DeleteService.bat and InsertIntoService.bat "TEMPLATE" to be same as project name
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {


            while (!stoppingToken.IsCancellationRequested)
            {

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                try
                {

                    //contoh run bila2
                    await HerbalifeProcess2DBarcodeScheduler.HerbalifeProcessBarcode(stoppingToken);



                }
                catch (Exception ex)
                {
                    await CreateLog.UpdateErrLogAsync(ex.ToString());
                    await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                }

            }
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
