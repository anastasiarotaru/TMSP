using Delivery.App;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

// The Serilog logger is the "adaptee" world: a real third-party library.
using Serilog.Core.Logger logger = new LoggerConfiguration()
    .WriteTo.Console(
        outputTemplate: "  [serilog {Level:u3}] {Message:lj}  (channel: {Channel})\n",
        theme: ConsoleTheme.None)
    .CreateLogger();

if (args.Contains("--interactive"))
{
    InteractiveSession.Run(logger);
}
else
{
    Demo.Run(logger);
}
