using IdleMon;
using CliWrap;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

if (args is { Length: >= 1 }) {
    string executablePath = Path.Combine(AppContext.BaseDirectory, "IdleMon.exe");
    if (args[0].ToLower() is "/install") {
        await IdleService.Uninstall();
        try {
            _ = await Cli.Wrap("sc")
            .WithArguments(new[] { "create", IdleService.SERVICE_NAME, $"binPath={executablePath}", "type=userown", "DisplayName=IdleMon", "start=auto" })
            .ExecuteAsync();
            Console.WriteLine("Successfully installed IdleMon service.");
        } catch (Exception ex) {
            Console.WriteLine($"Installation of IdleMon service failed: {ex}");
        }
    } else if (args[0].ToLower() is "/uninstall") {
        await IdleService.Uninstall();
        Console.WriteLine("Successfully uninstalled IdleMon service.");
    }
    return;
}

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options => { options.ServiceName = IdleService.SERVICE_NAME; });
LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(builder.Services);
builder.Services.AddSingleton<IdleService>();
builder.Services.AddHostedService<WindowsBackgroundService>();

IHost host = builder.Build();
host.Run();