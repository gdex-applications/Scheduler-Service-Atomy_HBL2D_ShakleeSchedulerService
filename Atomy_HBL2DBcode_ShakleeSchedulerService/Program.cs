using ATY_HBL2D_SHKSchedulerService;
using ATY_HBL2D_SHKSchedulerService.Functions;
using ATY_HBL2D_SHKSchedulerService.Functions.AtomyHLSAPI;
using ATY_HBL2D_SHKSchedulerService.Functions.HerbalifeProcess2DBarcode;
using ATY_HBL2D_SHKSchedulerService.Functions.ProcessSendReciptASN;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

CreateLog.CheckEerrLog();
CreateLog.CheckInfLog();
try
{
    IHost host = Host.CreateDefaultBuilder(args)
        .UseContentRoot(AppContext.BaseDirectory)
        .ConfigureAppConfiguration((hostContext, builder) =>
        {
            // Add the second custom JSON file
            //var x = hostContext.Configuration["xmlConfig"];
            // builder.AddXmlFile("D:\\OneDrive\\wh-middleware-latest\\WHMiddleware\\Web.config", optional: false, reloadOnChange: true);
        })
        .ConfigureServices((hostContext, services) =>
        {
            SchedulerHelpers.filePath = hostContext.Configuration["xmlConfig"]!;
            SchedulerHelpers.fileMap = new ExeConfigurationFileMap { ExeConfigFilename = SchedulerHelpers.filePath };
            SchedulerHelpers.extConfiguration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(SchedulerHelpers.fileMap, ConfigurationUserLevel.None);
            services.AddHostedService<AtomyHLSAPIWorker>();
            services.AddHostedService<HerbalifeProcess2DBarcodeWorker>();
            services.AddHostedService<ProcessSendReciptASNWorker>();
            services.AddHostedService<BranchCodeTaskWorker>();
        })
        .UseWindowsService()
        .Build();
    host.Run();
}
catch (Exception ex)
{

    throw;
}