using GH_LDM_PIIService;
using GH_LDM_PIIService.Helpers;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<ConfigManager>();

        services.AddHostedService<Worker>();
    })
    .UseWindowsService()
    .Build();

await host.RunAsync();
