using AtmyHbl2dShkleBrnCdSchedulerService;

namespace AtmyHbl2dShkleBrnCdSchedulerService.Functions.ProcessSendReciptASN
{
    public class ProcessSendReciptASNWorker : BackgroundService
    {
        private readonly ILogger<ProcessSendReciptASNWorker> _logger;
        private readonly IConfiguration _configuration;
        public ProcessSendReciptASNWorker(ILogger<ProcessSendReciptASNWorker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _ = new ProcessSendReciptASNScheduler(_configuration);

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
                    await ProcessSendReciptASNScheduler.ProcessSendReciptASN(stoppingToken);



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
