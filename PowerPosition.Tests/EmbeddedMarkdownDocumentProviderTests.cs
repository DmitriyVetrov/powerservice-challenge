using PowerPosition.Web.Documents;

namespace PowerPosition.Tests;

public sealed class EmbeddedMarkdownDocumentProviderTests
{
    private readonly EmbeddedMarkdownDocumentProvider _provider = new();

    // Guards the csproj: a document dropped from the embedding fails here, not on the live page.
    [Theory]
    [InlineData(MarkdownDocument.Requirements)]
    [InlineData(MarkdownDocument.Cv)]
    public void Render_ProducesHtml_ForEveryShippedDocument(string name)
    {
        var html = _provider.Render(name).Value;

        Assert.False(string.IsNullOrWhiteSpace(html));
        Assert.Contains("<h1", html, StringComparison.Ordinal);
    }

    [Fact]
    public void Render_Throws_WhenDocumentIsNotEmbedded()
    {
        Assert.Throws<InvalidOperationException>(() => _provider.Render("not-a-document"));
    }

    // The pages render on every circuit, so the conversion must happen once and be reused.
    [Fact]
    public void Render_CachesRenderedHtml()
    {
        var first = _provider.Render(MarkdownDocument.Cv).Value;
        var second = _provider.Render(MarkdownDocument.Cv).Value;

        Assert.Same(first, second);
    }
}
