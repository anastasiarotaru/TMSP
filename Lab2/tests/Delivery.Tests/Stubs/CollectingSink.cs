using Serilog.Core;
using Serilog.Events;

namespace Delivery.Tests.Stubs;

// A Serilog sink that keeps the events in memory, so the tests can inspect them.
public class CollectingSink : ILogEventSink
{
    public List<LogEvent> Events { get; } = new();

    public void Emit(LogEvent logEvent) => Events.Add(logEvent);
}
