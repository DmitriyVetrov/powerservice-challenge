using PowerPosition.Web.Components;
using PowerPosition.Web.Configuration;
using PowerPosition.Web.Documents;
using PowerPosition.Web.Reports;

var builder = WebApplication.CreateBuilder(args);

// The Home page selects reports without a page reload, so it needs an interactive circuit.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services
    .AddOptions<ReportOptions>()
    .Bind(builder.Configuration.GetSection(ReportOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<ICsvReportParser, CsvReportParser>();
builder.Services.AddSingleton<IReportCatalog, ReportCatalog>();
builder.Services.AddSingleton<IMarkdownDocumentProvider, EmbeddedMarkdownDocumentProvider>();

var app = builder.Build();

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
