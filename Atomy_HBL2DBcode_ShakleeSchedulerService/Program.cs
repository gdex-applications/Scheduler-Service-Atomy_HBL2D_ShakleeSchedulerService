using Atomy_HBL2DBcode_ShakleeSchedulerService;
using Atomy_HBL2DBcode_ShakleeSchedulerService.Functions.AtomyHLSAPI;
using Atomy_HBL2DBcode_ShakleeSchedulerService.Functions.HerbalifeProcess2DBarcode;
using Atomy_HBL2DBcode_ShakleeSchedulerService.Functions.ProcessSendReciptASN;

CreateLog.CheckEerrLog();
CreateLog.CheckInfLog();
IHost host = Host.CreateDefaultBuilder(args)
    .UseContentRoot(AppContext.BaseDirectory)
    .ConfigureServices(services =>
    {
        services.AddHostedService<AtomyHLSAPIWorker>();
        services.AddHostedService<HerbalifeProcess2DBarcodeWorker>();
        services.AddHostedService<ProcessSendReciptASNWorker>();
    })
    .UseWindowsService()
    .Build();
host.Run();
