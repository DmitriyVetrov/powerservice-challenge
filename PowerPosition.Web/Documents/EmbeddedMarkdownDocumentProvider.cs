using System.Collections.Concurrent;
using System.Reflection;
using Markdig;
using Microsoft.AspNetCore.Components;

namespace PowerPosition.Web.Documents;

/// <summary>
/// Serves the documents embedded from <c>/docs</c> by the csproj, under the resource names
/// <c>PowerPosition.Web.{name}.md</c>.
/// </summary>
/// <remarks>
/// Embedding rather than reading from disk keeps one authored copy in <c>/docs</c> and ships it
/// unchanged in dev and when published, so the pages never depend on the content root layout.
///
/// Registered as a singleton: the documents are fixed at build time, so each one is read and
/// converted on first request and the HTML is then reused for the life of the process.
/// </remarks>
public sealed class EmbeddedMarkdownDocumentProvider : IMarkdownDocumentProvider
{
    // Advanced extensions cover the pipe tables; the documents need no raw HTML.
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    private readonly Assembly assembly = typeof(EmbeddedMarkdownDocumentProvider).Assembly;
    private readonly ConcurrentDictionary<string, MarkupString> rendered = new(StringComparer.Ordinal);

    public MarkupString Render(string name) => rendered.GetOrAdd(name, RenderCore);

    private MarkupString RenderCore(string name)
    {
        var resource = $"PowerPosition.Web.{name}.md";

        // Missing means the csproj no longer embeds it — a build-time mistake, not a user error.
        using var stream = assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"No embedded document named '{resource}'.");
        using var reader = new StreamReader(stream);

        return new MarkupString(Markdown.ToHtml(reader.ReadToEnd(), Pipeline));
    }
}
