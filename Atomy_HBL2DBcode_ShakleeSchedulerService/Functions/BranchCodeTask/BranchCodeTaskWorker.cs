using AtmyHbl2dShkleBrnCdSchedulerService;

namespace AtmyHbl2dShkleBrnCdSchedulerService.Functions.BranchCodeTask
{
    public class BranchCodeTaskWorker : BackgroundService
    {
        private readonly ILogger<BranchCodeTaskWorker> _logger;
        private readonly IConfiguration _configuration;
        public BranchCodeTaskWorker(ILogger<BranchCodeTaskWorker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _ = new BranchCodeTaskScheduler(_configuration);

        }
        /// <summary>
        /// IMPORTANT!!!
        /// PLEASE UPDATE DeleteService.bat and InsertIntoService.bat "TEMPLATE" to be same as project name
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {


            while (!stoppingToken.IsCancellationRequested)
            {

               // await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                try
                {

                    // 1. Calculate how long to wait until the next 9am or 3pm
                    DateTime now = DateTime.Now;
                    DateTime nextRun = GetNextRunTime(now);
                    TimeSpan delay = nextRun - now;

                    // 2. Wait until that time
                    await Task.Delay(delay, stoppingToken);


                    //contoh run bila2
                    //UpdatingBranchCode
                    await BranchCodeTaskScheduler.UpdateLatestBranchCodeAsyncHLS(stoppingToken);
                    //ProcessBranchCode
                    await BranchCodeTaskScheduler.ProcessBranchCodeAsyncHLS(stoppingToken);

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
        // Helper to determine the next occurrence of 9am or 3pm
        private static DateTime GetNextRunTime(DateTime now)
        {
            var today9am = now.Date.AddHours(9);
            var today3pm = now.Date.AddHours(15);

            if (now < today9am) return today9am;
            if (now < today3pm) return today3pm;

            // If it's already past 3pm, the next run is 9am tomorrow
            return today9am.AddDays(1);
        }
    }
}
