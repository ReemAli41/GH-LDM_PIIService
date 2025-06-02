using GH_LDM_PIIService;
using GH_LDM_PIIService.DSL;
using GH_LDM_PIIService.Helpers;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        string logPath = hostContext.Configuration.GetValue<string>("LogPath");
        File_Logger.GetLogFilePath_Event += () => logPath;

        services.AddSingleton<UpdatePdfDSL>();
        services.AddSingleton<TimerDSL>();
        services.AddSingleton<ConfigManager>();
        services.AddSingleton<HttpHelper>();

        services.AddHostedService<Worker>();
    })
    .UseWindowsService()
    .Build();

await host.RunAsync();
