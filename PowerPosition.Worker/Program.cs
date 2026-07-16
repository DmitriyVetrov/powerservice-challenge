using System.Threading.Channels;
using Axpo;
using PowerPosition.Worker.Configuration;
using PowerPosition.Worker.Domain;
using PowerPosition.Worker.Extraction;
using PowerPosition.Worker.Reporting;
using PowerPosition.Worker.Pipeline;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<ExtractOptions>()
    .Bind(builder.Configuration.GetSection(ExtractOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton(TimeProvider.System);

// The domain graph, composed behind IExtractService. All pieces are stateless, so singletons.
builder.Services.AddSingleton<IPowerService, PowerService>();
builder.Services.AddSingleton<TradingDayCalendar>();
builder.Services.AddSingleton<PositionAggregator>();
builder.Services.AddSingleton<IPositionWriter, CsvPositionWriter>();
builder.Services.AddSingleton<IExtractService, ExtractService>();

// Bounded + FullMode.Wait so a slow trading system delays a tick rather than dropping it.
// Exactly one producer and one consumer, so single reader/writer.
builder.Services.AddSingleton(_ => Channel.CreateBounded<ScheduledExtract>(
    new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
        SingleWriter = true,
    }));
// Hand each service only the half of the channel it needs.
builder.Services.AddSingleton(sp => sp.GetRequiredService<Channel<ScheduledExtract>>().Reader);
builder.Services.AddSingleton(sp => sp.GetRequiredService<Channel<ScheduledExtract>>().Writer);

// Order matters for a clean shutdown: hosted services stop in reverse registration order, so the
// producer (registered last) stops first and completes the queue, then the worker drains it.
builder.Services.AddHostedService<ExtractWorker>();
builder.Services.AddHostedService<ScheduleProducer>();

var host = builder.Build();
host.Run();
